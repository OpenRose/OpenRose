// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using AutoMapper;
using ItemzApp.API.Helper;
using ItemzApp.API.Models;
using ItemzApp.API.Models.BetweenControllerAndRepository;
using ItemzApp.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace ItemzApp.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")] // e.g. http://HOST:PORT/api/export
	public class ExportController : ControllerBase
	{
		private readonly IBaselineHierarchyRepository _baselineHierarchyRepository;
		private readonly IBaselineItemzTraceExportService _baselineItemzTraceExportService;
		private readonly IItemzTraceExportService _itemzTraceExportService;
		private readonly IExportNodeMapper _exportNodeMapper;
		private readonly IProjectRepository _projectRepository;
		private readonly IHierarchyRepository _hierarchyRepository;
		private readonly IMapper _mapper;
		private readonly ILogger<ExportController> _logger;

		public ExportController(IBaselineHierarchyRepository baselineHierarchyRepository,
								IBaselineItemzTraceExportService baselineItemzTraceExportService,
								IItemzTraceExportService itemzTraceExportService,
								IExportNodeMapper exportNodeMapper,
								IProjectRepository projectRepository,
								IHierarchyRepository hierarchyRepository,
								IMapper mapper,
								ILogger<ExportController> logger)
		{
			_baselineHierarchyRepository = baselineHierarchyRepository ?? throw
				new ArgumentNullException(nameof(baselineHierarchyRepository));
			_baselineItemzTraceExportService = baselineItemzTraceExportService ?? throw
				new ArgumentNullException(nameof(baselineItemzTraceExportService));
			_itemzTraceExportService = itemzTraceExportService ?? throw new ArgumentNullException(nameof(itemzTraceExportService));
			_exportNodeMapper = exportNodeMapper ?? throw new ArgumentNullException(nameof(exportNodeMapper));
			_projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
			_hierarchyRepository = hierarchyRepository ?? throw new ArgumentNullException(nameof(hierarchyRepository));
			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// Exporting Data based on provided RecordId along with it's hierarchy breakdown structure
		/// </summary>
		/// <param name="exportRecordId">Record ID for the main record to be exported along with it's hierarchy breakdown structure</param>
		/// <param name="exportIncludedBaselineItemzOnly">Boolean value to decide if excluded BaselineItemz should be exported or not</param>
		/// <returns></returns>

		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[Produces("application/json")]
		[HttpGet("ExportHierarchy", Name = "__Export_Hierarchy__")]
		public async Task<IActionResult> ExportHierarchy([FromQuery] Guid exportRecordId,
										[FromQuery] bool exportIncludedBaselineItemzOnly = false)
		{
			_logger.LogDebug("{FormattedControllerAndActionNames} Processing request to export hierarchy as RepositoryExportDTO. Id: {ExportRecordId}, exportIncludedBaselineItemzOnly: {ExportIncludedBaselineItemzOnly}",
				ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), exportRecordId, exportIncludedBaselineItemzOnly);

			if (exportRecordId == Guid.Empty)
			{
				return BadRequest("ExportRecordId must be a valid GUID.");
			}

			try
			{
				var repositoryRecordDto = await _hierarchyRepository.GetRepositoryHierarchyRecord();
				if (repositoryRecordDto == null)
					return NotFound("Repository (root) not found.");

				RepositoryExportDTO? exportDto = null;
				string? recordType = null;

				// Try live hierarchy (Project/ItemzType/Itemz)
				try
				{
					var parentHierarchyRecord = await _hierarchyRepository.GetHierarchyRecordDetailsByID(exportRecordId);
					if (parentHierarchyRecord != null)
					{
						var hierarchyTree = await _hierarchyRepository.GetAllChildrenOfItemzHierarchy(exportRecordId);
						var rootNode = new NestedHierarchyIdRecordDetailsDTO
						{
							RecordId = parentHierarchyRecord.RecordId,
							HierarchyId = parentHierarchyRecord.HierarchyId,
							Level = parentHierarchyRecord.Level,
							RecordType = parentHierarchyRecord.RecordType,
							Name = parentHierarchyRecord.Name,
							Children = (List<NestedHierarchyIdRecordDetailsDTO>)hierarchyTree.AllRecords
						};
						recordType = parentHierarchyRecord.RecordType?.ToLowerInvariant();

						exportDto = new RepositoryExportDTO
						{
							RepositoryId = repositoryRecordDto.RecordId
						};

						HashSet<Guid> exportedItemzIds = CollectExportedIdsByType(rootNode, "Itemz");
						var itemzTraces = await _itemzTraceExportService.GetTracesForExportAsync(exportedItemzIds);
						exportDto.ItemzTraces = itemzTraces;

						switch (recordType)
						{
							case "project":
								exportDto.Projects = new List<ProjectExportNode> { await _exportNodeMapper.ConvertToProjectExportNode(rootNode) };
								break;
							case "itemztype":
								exportDto.ItemzTypes = new List<ItemzTypeExportNode> { await _exportNodeMapper.ConvertToItemzTypeExportNode(rootNode) };
								break;
							case "itemz":
								exportDto.Itemz = new List<ItemzExportNode> { await _exportNodeMapper.ConvertToItemzExportNode(rootNode) };
								break;
							default:
								return BadRequest($"Unsupported RecordType: {recordType}");
						}
					}
				}
				catch (Exception ex)
				{
					_logger.LogDebug("Live hierarchy lookup failed: {0}", ex.Message);
				}

				// If not found in live hierarchy, try baseline hierarchy
				if (exportDto == null)
				{
					try
					{
						var baselineHierarchyRecord = await _baselineHierarchyRepository.GetBaselineHierarchyRecordDetailsByID(exportRecordId);
						if (baselineHierarchyRecord != null)
						{
							var rootRecordType = baselineHierarchyRecord.RecordType?.ToLowerInvariant();

							// Only apply 404 logic for BaselineItemz nodes
							if (rootRecordType == "baselineitemz"
								&& exportIncludedBaselineItemzOnly
								&& baselineHierarchyRecord.IsIncluded == false)
							{
								return NotFound(
									$"Requested BaselineItemz (ID: {exportRecordId}) is excluded and cannot be exported with exportIncludedBaselineItemzOnly=true."
								);
							}

							var baselineHierarchyTree = await _baselineHierarchyRepository.GetAllChildrenOfBaselineItemzHierarchy(
								exportRecordId, exportIncludedBaselineItemzOnly
							);

							var rootNode = new NestedBaselineHierarchyIdRecordDetailsDTO
							{
								RecordId = baselineHierarchyRecord.RecordId,
								BaselineHierarchyId = baselineHierarchyRecord.BaselineHierarchyId,
								Level = baselineHierarchyRecord.Level,
								RecordType = baselineHierarchyRecord.RecordType,
								Name = baselineHierarchyRecord.Name,
								isIncluded = baselineHierarchyRecord.IsIncluded,
								Children = (List<NestedBaselineHierarchyIdRecordDetailsDTO>)baselineHierarchyTree.AllRecords
							};
							recordType = baselineHierarchyRecord.RecordType?.ToLowerInvariant();

							exportDto = new RepositoryExportDTO
							{
								RepositoryId = repositoryRecordDto.RecordId
							};

							var exportedBaselineItemzIds = CollectExportedIdsByType(rootNode, "BaselineItemz");
							var baselineItemzTraces = await _baselineItemzTraceExportService.GetTracesForExportAsync(exportedBaselineItemzIds);
							exportDto.BaselineItemzTraces = baselineItemzTraces;

							switch (recordType)
							{
								case "baseline":
									exportDto.Baselines = new List<BaselineExportNode> { await _exportNodeMapper.ConvertToBaselineExportNode(rootNode) };
									break;
								case "baselineitemztype":
									exportDto.BaselineItemzTypes = new List<BaselineItemzTypeExportNode> { await _exportNodeMapper.ConvertToBaselineItemzTypeExportNode(rootNode) };
									break;
								case "baselineitemz":
									exportDto.BaselineItemz = new List<BaselineItemzExportNode> { await _exportNodeMapper.ConvertToBaselineItemzExportNode(rootNode) };
									break;
								default:
									return BadRequest($"Unsupported RecordType: {recordType}");
							}
						}
					}
					catch (Exception ex)
					{
						_logger.LogDebug("Baseline hierarchy lookup failed: {0}", ex.Message);
					}
				}

				// If we got here and still no exportDto, record was not found
				if (exportDto == null)
				{
					return NotFound($"Record with ID '{exportRecordId}' not found across Itemz OR Baseline Hierarchy data.");
				}

				// Serialize JSON Export Data
				var json = System.Text.Json.JsonSerializer.Serialize(exportDto, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

				// Validate JSON before returning
				var schemaJson = await System.IO.File.ReadAllTextAsync("Schemas/OpenRose.Export.schema.1.0.json");
				var schema = JSchema.Parse(schemaJson);

				var exportJObject = JObject.Parse(json);
				if (!exportJObject.IsValid(schema, out IList<string> errors))
				{
					_logger.LogError("Export JSON failed schema validation: {Errors}", string.Join("; ", errors));
					return UnprocessableEntity(new
					{
						error = "Export JSON failed schema validation.",
						details = errors
					});
				}

				var content = System.Text.Encoding.UTF8.GetBytes(json);
				var fileName = $"OpenRose_Export_{recordType}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
				Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{fileName}\"");
				return File(content, "application/json; charset=utf-8", fileName);
			}
			catch (Exception ex)
			{
				_logger.LogError("Exception during hierarchy export: {0}", ex.Message);
				return StatusCode(StatusCodes.Status500InternalServerError, "Error exporting hierarchy.");
			}
		}

		/// <summary>
		/// Returning GUID dataset found in the hierarchy records for the given record type
		/// </summary>
		/// <param name="node">We look for node in the live data belonging to Project, ItemzType or Requirements Itemz </param>
		/// <param name="recordTypeToCollect"></param>
		/// <returns></returns>
		private HashSet<Guid> CollectExportedIdsByType(NestedHierarchyIdRecordDetailsDTO node, string recordTypeToCollect)
		{
			var ids = new HashSet<Guid>();

			void Traverse(NestedHierarchyIdRecordDetailsDTO current)
			{
				if (string.Equals(current.RecordType, recordTypeToCollect, StringComparison.OrdinalIgnoreCase))
				{
					ids.Add(current.RecordId);
				}
				if (current.Children != null)
				{
					foreach (var child in current.Children)
					{
						Traverse(child);
					}
				}
			}

			Traverse(node);
			return ids;
		}


		/// <summary>
		/// Returning GUID dataset found in the hierarchy records for the given record type
		/// </summary>
		/// <param name="node">We look for node in the live data belonging to Baseline, BaselineItemzType or BaselineItemz </param>
		/// <param name="recordTypeToCollect"></param>
		/// <returns></returns>


		private HashSet<Guid> CollectExportedIdsByType(NestedBaselineHierarchyIdRecordDetailsDTO node, string recordTypeToCollect)
		{
			var ids = new HashSet<Guid>();

			void Traverse(NestedBaselineHierarchyIdRecordDetailsDTO current)
			{
				if (string.Equals(current.RecordType, recordTypeToCollect, StringComparison.OrdinalIgnoreCase))
				{
					ids.Add(current.RecordId);
				}
				if (current.Children != null)
				{
					foreach (var child in current.Children)
					{
						Traverse(child);
					}
				}
			}

			Traverse(node);
			return ids;
		}

		// TODO :: Like we have 'ExportHierarchy' implemented in this controller, similartly we should also support exporting 
		// Mermaid Flow Chart Diagram text for 
		//  - Project
		//	 - ItemzType
		//	 - Itemz
		//	 - Baseline
		//	 - BaselineItemzType
		//	 - BaselineItemz
		//	We should process all the required data on the server side and then return finalized 
		// Mermaid Flow Chart Diagram text to the calling client!  
		// This way all the heavy lifting work can be perform on the server side and in the future
		// this activity could also be offloaded to a distributed containerized worker service if needed!
		// For now, there is no need to create a separate controller for this functionality as this can be
		// easily handled in this existing ExportController itself!

	}
}
