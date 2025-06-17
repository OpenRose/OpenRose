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
		private readonly IExportRepository _exportRepository;
		private readonly IMapper _mapper;
		private readonly ILogger<ExportController> _logger;

		public ExportController(IBaselineHierarchyRepository baselineHierarchyRepository, 
								IBaselineItemzTraceExportService baselineItemzTraceExportService,
								IItemzTraceExportService itemzTraceExportService,
								IExportNodeMapper exportNodeMapper, 
								IProjectRepository projectRepository, 
								IExportRepository exportRepository,
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
			_exportRepository = exportRepository ?? throw new ArgumentNullException(nameof(exportRepository));
			_hierarchyRepository = hierarchyRepository ?? throw new ArgumentNullException(nameof(hierarchyRepository));
			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// Exports data based on the selected entity type (Project, ItemzType, Itemz).
		/// </summary>
		/// <param name="recordId">Unique identifier for the selected entity.</param>
		/// <returns>Returns exported JSON file as stream that contains data.</returns>
		[HttpGet("{entityType}/{entityId}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> ExportData(Guid recordId)
		{

			// TODO : We need to decide on the input metadata for this method. At this 
			// point, we only need 'exportRecordId' in GUID form as we are going to 
			// export entire requirements breakdown structure under it. Also 'exportRecordID' 
			// should be present in ItemzHierarcy and it should be at Level 1 or above so that 
			// we know that it's certainly a project or an Itemztype or a Requirement Itemz.
			// We have to decide if we want to take in any other information. For example, if we
			// allow user to pass in any parameter that decides if traces are exported or not
			// OR Include summary of number of records being exported
			// OR Exclude metadata for drop down fields
			// OR Do not export description
			// OR Do not export Attachments
			// etc. 



			_logger.LogInformation($"Export requested for ID {recordId}");

			try
			{
				string filePath = await _exportRepository.GenerateExportFileAsync(recordId);

				if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
				{
					_logger.LogWarning($"Export failed: File not found for ID {recordId}");
					return NotFound("Export file generation failed.");
				}

				return File(System.IO.File.OpenRead(filePath), "application/json", $"_export.json"); // TODO : Perhaps add Date time to export file to make it unique.
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error exporting data");
				return StatusCode(500, $"Internal Server Error: {ex.Message}");
			}
		}






		///// <summary>
		///// Exports the root Repository Hierarchy record as a downloadable JSON file.
		///// </summary>
		///// <returns>JSON file of the Repository Hierarchy record</returns>
		///// <response code="200">Returns the JSON file</response>
		///// <response code="404">Repository hierarchy record not found</response>
		//[ProducesResponseType(StatusCodes.Status404NotFound)]
		//[ProducesResponseType(StatusCodes.Status200OK)]
		//[HttpGet("ExportRepositoryRoot", Name = "__Export_Repository_Root__")] // e.g. http://HOST:PORT/api/Hierarchy/ExportRepositoryRoot
		//public async Task<IActionResult> ExportRepositoryRoot()
		//{
		//	_logger.LogDebug("{FormattedControllerAndActionNames} Processing request to export Repository root as JSON.",
		//		ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext));

		//	try
		//	{
		//		var repositoryRecordDto = await _hierarchyRepository.GetRepositoryHierarchyRecord();

		//		if (repositoryRecordDto == null)
		//		{
		//			_logger.LogDebug("{FormattedControllerAndActionNames} No Repository (root) Hierarchy record found for export.",
		//				ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext));
		//			return NotFound();
		//		}

		//		var json = System.Text.Json.JsonSerializer.Serialize(
		//			repositoryRecordDto,
		//			new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
		//		);

		//		var content = System.Text.Encoding.UTF8.GetBytes(json);

		//		var fileName = $"RepositoryRootExport_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";

		//		_logger.LogDebug("{FormattedControllerAndActionNames} Returning Repository root as file: {FileName}",
		//			ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), fileName);

		//		return File(content, "application/json", fileName);
		//	}
		//	catch (Exception ex)
		//	{
		//		_logger.LogError("{FormattedControllerAndActionNames} Exception during repository root export: {Error}",
		//			ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), ex.Message);
		//		return StatusCode(StatusCodes.Status500InternalServerError, "Error exporting repository root hierarchy.");
		//	}
		//}




		///// <summary>
		///// Exports the specified Project as a RepositoryExportDTO JSON file.
		///// </summary>
		///// <param name="exportRecordType">Type of record to export (must be "Project")</param>
		///// <param name="exportRecordId">GUID of the Project to export</param>
		///// <returns>JSON file of the exported project in RepositoryExportDTO format</returns>
		///// <response code="200">Returns the JSON file</response>
		///// <response code="404">Repository or Project not found</response>
		//[ProducesResponseType(StatusCodes.Status404NotFound)]
		//[ProducesResponseType(StatusCodes.Status200OK)]
		//[Produces("application/json")]
		//[HttpGet("ExportProject", Name = "__Export_Project__")]
		//public async Task<IActionResult> ExportProject([FromQuery] string exportRecordType, [FromQuery] Guid exportRecordId)
		//{
		//	_logger.LogDebug("{FormattedControllerAndActionNames} Processing request to export Project as RepositoryExportDTO. ProjectId: {ProjectId}",
		//		ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), exportRecordId);

		//	if (!string.Equals(exportRecordType, "Project", StringComparison.OrdinalIgnoreCase))
		//	{
		//		return BadRequest("ExportRecordType must be 'Project'.");
		//	}
		//	if (exportRecordId == Guid.Empty)
		//	{
		//		return BadRequest("ExportRecordId must be a valid GUID.");
		//	}

		//	try
		//	{
		//		// 1. Get Repository root (for RepositoryId)
		//		var repositoryRecordDto = await _hierarchyRepository.GetRepositoryHierarchyRecord();
		//		if (repositoryRecordDto == null)
		//		{
		//			_logger.LogDebug("{FormattedControllerAndActionNames} No Repository (root) Hierarchy record found for export.",
		//				ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext));
		//			return NotFound("Repository (root) not found.");
		//		}

		//		// 2. Get Project entity
		//		var projectEntity = await _projectRepository.GetProjectAsync(exportRecordId);
		//		if (projectEntity == null)
		//		{
		//			return NotFound($"Project with ID '{exportRecordId}' not found.");
		//		}

		//		// 3. Map Project entity to GetProjectDTO (implement mapping as needed)
		//		GetProjectDTO projectDto = _mapper.Map<GetProjectDTO>(projectEntity);
		//		if (projectDto == null)
		//		{
		//			_logger.LogError("{FormattedControllerAndActionNames} Mapping Project entity to DTO failed for ProjectId: {ProjectId}",
		//				ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), exportRecordId);
		//			return StatusCode(StatusCodes.Status500InternalServerError, "Failed to map Project entity to DTO.");
		//		}

		//		// 4. Create ProjectExportNode
		//		var projectExportNode = new ProjectExportNode
		//		{
		//			Project = projectDto,
		//			ItemzTypes = null // TODO :: Populate if needed in the future
		//		};

		//		// 5. Create RepositoryExportDTO
		//		var exportDto = new RepositoryExportDTO
		//		{
		//			RepositoryId = repositoryRecordDto.RecordId,
		//			Projects = new List<ProjectExportNode> { projectExportNode },
		//			// Other properties remain null/empty
		//		};

		//		// 6. Serialize to JSON and return as file
		//		var json = System.Text.Json.JsonSerializer.Serialize(
		//			exportDto,
		//			new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
		//		);
		//		var content = System.Text.Encoding.UTF8.GetBytes(json);

		//		var fileName = $"RepositoryExport_Project_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";

		//		_logger.LogDebug("{FormattedControllerAndActionNames} Returning Project export as file: {FileName}",
		//			ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), fileName);

		//		return File(content, "application/json", fileName);
		//	}
		//	catch (Exception ex)
		//	{
		//		_logger.LogError("{FormattedControllerAndActionNames} Exception during project export: {Error}",
		//			ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), ex.Message);
		//		return StatusCode(StatusCodes.Status500InternalServerError, "Error exporting project.");
		//	}
		//}



		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[Produces("application/json")]
		[HttpGet("ExportHierarchy", Name = "__Export_Hierarchy__")]
		public async Task<IActionResult> ExportHierarchy([FromQuery] Guid exportRecordId)
		{
			_logger.LogDebug("{FormattedControllerAndActionNames} Processing request to export hierarchy as RepositoryExportDTO. Id: {ExportRecordId}",
				ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), exportRecordId);

			if (exportRecordId == Guid.Empty)
			{
				return BadRequest("ExportRecordId must be a valid GUID.");
			}

			try
			{

				var repositoryRecordDto = await _hierarchyRepository.GetRepositoryHierarchyRecord();
				if (repositoryRecordDto == null)
					return NotFound("Repository (root) not found.");

				// Export Project OR ItemzType OR Itemz Data 

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
						var recordType = parentHierarchyRecord.RecordType?.ToLowerInvariant();

						var exportDto = new RepositoryExportDTO
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

						// Serialize and return
						var json = System.Text.Json.JsonSerializer.Serialize(exportDto, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
						var content = System.Text.Encoding.UTF8.GetBytes(json);
						var fileName = $"RepositoryExport_{recordType}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
						return File(content, "application/json", fileName);
					}
				}
				catch (Exception ex)
				{
					_logger.LogDebug("Live hierarchy lookup failed: {0}", ex.Message);
				}

				// Export Baseline OR BaselineItemzType OR BaselineItemz Data 

				try
				{
					var baselineHierarchyRecord = await _baselineHierarchyRepository.GetBaselineHierarchyRecordDetailsByID(exportRecordId);
					if (baselineHierarchyRecord != null)
					{


						var baselineHierarchyTree = await _baselineHierarchyRepository.GetAllChildrenOfBaselineItemzHierarchy(exportRecordId);

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
						var recordType = baselineHierarchyRecord.RecordType?.ToLowerInvariant();


						var exportDto = new RepositoryExportDTO
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

						// Serialize and return
						var json = System.Text.Json.JsonSerializer.Serialize(exportDto, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
						var content = System.Text.Encoding.UTF8.GetBytes(json);
						var fileName = $"RepositoryExport_{recordType}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
						return File(content, "application/json", fileName);
					}
				}
				catch (Exception ex)
				{
					_logger.LogDebug("Baseline hierarchy lookup failed: {0}", ex.Message);
				}

				// If we got here, then it means provided ID is not found for any of the data type that we support for exporting
				return NotFound($"Record with ID '{exportRecordId}' not found across Itemz OR Baseline Hierarchy data.");
			}
			catch (Exception ex)
			{
				_logger.LogError("Exception during hierarchy export: {0}", ex.Message);
				return StatusCode(StatusCodes.Status500InternalServerError, "Error exporting hierarchy.");
			}
		}


		private HashSet<Guid> CollectExportedIdsByType(NestedHierarchyIdRecordDetailsDTO node, string typeToCollect)
		{
			var ids = new HashSet<Guid>();

			void Traverse(NestedHierarchyIdRecordDetailsDTO current)
			{
				if (string.Equals(current.RecordType, typeToCollect, StringComparison.OrdinalIgnoreCase))
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


		private HashSet<Guid> CollectExportedIdsByType(NestedBaselineHierarchyIdRecordDetailsDTO node, string typeToCollect)
		{
			var ids = new HashSet<Guid>();

			void Traverse(NestedBaselineHierarchyIdRecordDetailsDTO current)
			{
				if (string.Equals(current.RecordType, typeToCollect, StringComparison.OrdinalIgnoreCase))
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

	}
}
