// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using AutoMapper;
using ItemzApp.API.Helper;
using ItemzApp.API.Models;
using ItemzApp.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
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
			_logger.LogDebug("{FormattedControllerAndActionNames} Processing request to export hierarchy as RepositoryExportForJsonDTO. Id: {ExportRecordId}, exportIncludedBaselineItemzOnly: {ExportIncludedBaselineItemzOnly}",
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

				RepositoryExportForJsonDTO? exportDto = null;
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

						exportDto = new RepositoryExportForJsonDTO
						{
							RepositoryId = repositoryRecordDto.RecordId
						};

						HashSet<Guid> exportedItemzIds = CollectExportedIdsByType(rootNode, "Itemz");
						var itemzTraces = await _itemzTraceExportService.GetTracesForExportAsync(exportedItemzIds);

						//exportDto.ItemzTraces = itemzTraces;

						// Recursively collect all nodes under root
						Dictionary<Guid, string> BuildIdToNameMap(NestedHierarchyIdRecordDetailsDTO node)
						{
							var map = new Dictionary<Guid, string>();
							void Traverse(NestedHierarchyIdRecordDetailsDTO current)
							{
								if ( !(string.IsNullOrWhiteSpace(current.Name)) )
								{ 
									map[current.RecordId] = current.Name;
								}
								foreach (var child in current.Children ?? Enumerable.Empty<NestedHierarchyIdRecordDetailsDTO>())
								{
									Traverse(child);
								}
							}
							Traverse(node);
							return map;
						}

						var idToNameMap = BuildIdToNameMap(rootNode);


						// Project into ItemzTraceExportNodeDTO objects
						exportDto.ItemzTraces = itemzTraces.Select(t => new ItemzTraceExportNodeDTO
						{
							FromTraceName = idToNameMap.TryGetValue(t.FromTraceItemzId, out var fromName) ? fromName : null,
							ToTraceName = idToNameMap.TryGetValue(t.ToTraceItemzId, out var toName) ? toName : null,
							FromTraceItemzId = t.FromTraceItemzId,
							ToTraceItemzId = t.ToTraceItemzId,
							TraceLabel = t.TraceLabel
						}).ToList();



						switch (recordType)
						{
							case "project":
								exportDto.Projects = new List<ProjectExportNodeForJson> { await _exportNodeMapper.ConvertToProjectExportNode(rootNode) };
								break;
							case "itemztype":
								exportDto.ItemzTypes = new List<ItemzTypeExportNodeForJson> { await _exportNodeMapper.ConvertToItemzTypeExportNode(rootNode) };
								break;
							case "itemz":
								exportDto.Itemz = new List<ItemzExportNodeForJson> { await _exportNodeMapper.ConvertToItemzExportNode(rootNode) };
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

							exportDto = new RepositoryExportForJsonDTO
							{
								RepositoryId = repositoryRecordDto.RecordId
							};

							//var exportedBaselineItemzIds = CollectExportedIdsByType(rootNode, "BaselineItemz");
							//var baselineItemzTraces = await _baselineItemzTraceExportService.GetTracesForExportAsync(exportedBaselineItemzIds);
							//exportDto.BaselineItemzTraces = baselineItemzTraces;

							// Recursively collect all nodes under baseline root
							Dictionary<Guid, string> BuildBaselineIdToNameMap(NestedBaselineHierarchyIdRecordDetailsDTO node)
							{
								var map = new Dictionary<Guid, string>();
								void Traverse(NestedBaselineHierarchyIdRecordDetailsDTO current)
								{
									if (!string.IsNullOrWhiteSpace(current.Name))
									{
										map[current.RecordId] = current.Name;
									}
									foreach (var child in current.Children ?? Enumerable.Empty<NestedBaselineHierarchyIdRecordDetailsDTO>())
									{
										Traverse(child);
									}
								}
								Traverse(node);
								return map;
							}

							var idToNameMap = BuildBaselineIdToNameMap(rootNode);

							var exportedBaselineItemzIds = CollectExportedIdsByType(rootNode, "BaselineItemz");
							var baselineItemzTraces = await _baselineItemzTraceExportService.GetTracesForExportAsync(exportedBaselineItemzIds);

							// Project into BaselineItemzTraceExportNodeDTO objects
							exportDto.BaselineItemzTraces = baselineItemzTraces.Select(t => new BaselineItemzTraceExportNodeDTO
							{
								FromTraceBaselineItemzId = t.FromTraceBaselineItemzId,
								ToTraceBaselineItemzId = t.ToTraceBaselineItemzId,
								TraceLabel = t.TraceLabel,
								FromTraceBaselineItemzName = idToNameMap.TryGetValue(t.FromTraceBaselineItemzId, out var fromName) ? fromName : null,
								ToTraceBaselineItemzName = idToNameMap.TryGetValue(t.ToTraceBaselineItemzId, out var toName) ? toName : null
							}).ToList();


							switch (recordType)
							{
								case "baseline":
									exportDto.Baselines = new List<BaselineExportNodeForJson> { await _exportNodeMapper.ConvertToBaselineExportNode(rootNode) };
									break;
								case "baselineitemztype":
									exportDto.BaselineItemzTypes = new List<BaselineItemzTypeExportNodeForJson> { await _exportNodeMapper.ConvertToBaselineItemzTypeExportNode(rootNode) };
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


		#region START ExportMermaidFlowChart

		// EXPLANATION :: Like we have 'ExportHierarchy' implemented in this controller, similartly we should also support exporting 
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

		/// <summary>
		/// Exports a Mermaid flowchart diagram (in plain text format) that represents the hierarchy
		/// of a record identified by <paramref name="exportRecordId"/>. The diagram includes
		/// parent-child relationships and traceability information for Itemz or BaselineItemz records.
		/// </summary>
		/// <remarks>
		/// This endpoint attempts to locate the record in two possible hierarchies:
		/// <list type="bullet">
		///   <item>
		///     <description><b>Live hierarchy</b> – Standard Itemz records and their children.</description>
		///   </item>
		///   <item>
		///     <description><b>Baseline hierarchy</b> – BaselineItemz records, which may be marked as included or excluded.</description>
		///   </item>
		/// </list>
		/// If found, the hierarchy is traversed, traceability data is collected, and Mermaid diagram text
		/// is generated. The response is returned as <c>text/plain</c>.
		/// </remarks>
		/// <param name="exportRecordId">
		/// The unique identifier (<see cref="Guid"/>) of the record to export.
		/// Must be a non-empty GUID. If empty, the request will return <c>400 Bad Request</c>.
		/// </param>
		/// <param name="exportIncludedBaselineItemzOnly">
		/// When <c>true</c>, only BaselineItemz records marked as "included" will be exported.
		/// When <c>false</c>, both included and excluded BaselineItemz records are considered.
		/// </param>
		/// <returns>
		/// An <see cref="IActionResult"/> representing one of the following outcomes:
		/// <list type="bullet">
		///   <item>
		///     <description><c>200 OK</c> – Mermaid diagram text is successfully generated and returned as <c>text/plain</c>.</description>
		///   </item>
		///   <item>
		///     <description><c>400 Bad Request</c> – The provided <paramref name="exportRecordId"/> is empty or invalid.</description>
		///   </item>
		///   <item>
		///     <description><c>404 Not Found</c> – No matching record exists in either hierarchy, or the requested BaselineItemz is excluded.</description>
		///   </item>
		///   <item>
		///     <description><c>500 Internal Server Error</c> – An unexpected error occurred during export.</description>
		///   </item>
		/// </list>
		/// </returns>
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[Produces("text/plain")]
		[HttpGet("ExportMermaidFlowChart", Name = "__Export_Mermaid_FlowChart__")]
		public async Task<IActionResult> ExportMermaidFlowChart([FromQuery] Guid exportRecordId,
																[FromQuery] bool exportIncludedBaselineItemzOnly = false,
																[FromQuery] string? baseUrl = null,
																[FromQuery] bool showTraceabilityOnly = false)
		{
			if (exportRecordId == Guid.Empty)
			{
				return BadRequest("ExportRecordId must be a valid GUID.");
			}


			baseUrl = string.IsNullOrWhiteSpace(baseUrl) ? null : baseUrl.TrimEnd('/');


			try
			{
				// --- LIVE hierarchy branch ---
				HierarchyIdRecordDetailsDTO? parentHierarchyRecord = null;
				try
				{
					parentHierarchyRecord = await _hierarchyRepository.GetHierarchyRecordDetailsByID(exportRecordId);
				}
				catch (Exception ex)
				{
					// If repository throws "Expected 1 record but found 0", treat as not found
					_logger.LogDebug("Live hierarchy record not found for ID {ExportRecordId}: {Message}", exportRecordId, ex.Message);
					parentHierarchyRecord = null;
				}

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

					var exportedItemzIds = CollectExportedIdsByType(rootNode, "Itemz");
					var itemzTraces = await _itemzTraceExportService.GetTracesForExportAsync(exportedItemzIds);


					//var tracedIds = new HashSet<Guid>(
					//	itemzTraces.SelectMany(t => new[] { t.FromTraceItemzId, t.ToTraceItemzId }));


					//var rootNodeToExport = showTraceabilityOnly
					//	? FilterForTraceability(rootNode, tracedIds) ?? rootNode
					//	: rootNode;


					//NestedHierarchyIdRecordDetailsDTO rootNodeToExport;

					//if (showTraceabilityOnly)
					//{
					//	var tracedIds = new HashSet<Guid>(
					//		itemzTraces.SelectMany(t => new[] { t.FromTraceItemzId, t.ToTraceItemzId }));

					//	rootNodeToExport = FilterForTraceability(rootNode, tracedIds) ?? rootNode;
					//}
					//else
					//{
					//	rootNodeToExport = rootNode;
					//}

					NestedHierarchyIdRecordDetailsDTO rootNodeToExport;

					if (showTraceabilityOnly)
					{
						var tracedIds = new HashSet<Guid>(
							itemzTraces.SelectMany(t => new[] { t.FromTraceItemzId, t.ToTraceItemzId }));

						if (tracedIds.Count > 0)
						{
							rootNodeToExport = FilterForTraceability(rootNode, tracedIds) ?? rootNode;
						}
						else
						{
							// No trace entries found, export only the root node without children
							rootNodeToExport = new NestedHierarchyIdRecordDetailsDTO
							{
								RecordId = rootNode.RecordId,
								HierarchyId = rootNode.HierarchyId,
								Level = rootNode.Level,
								RecordType = rootNode.RecordType,
								Name = rootNode.Name,
								Children = new List<NestedHierarchyIdRecordDetailsDTO>() // empty
							};
						}
					}
					else
					{
						rootNodeToExport = rootNode;
					}

					var mermaidText = MermaidExporter.Generate(rootNodeToExport, itemzTraces, exportRecordId, baseUrl);

					// var mermaidText = MermaidExporter.Generate(rootNode, itemzTraces, exportRecordId, baseUrl);
					return Content(mermaidText, "text/plain");
				}

				// --- BASELINE hierarchy branch ---
				BaselineHierarchyIdRecordDetailsDTO? baselineHierarchyRecord = null;
				try
				{
					baselineHierarchyRecord = await _baselineHierarchyRepository.GetBaselineHierarchyRecordDetailsByID(exportRecordId);
				}
				catch (Exception ex)
				{
					_logger.LogDebug("Baseline hierarchy record not found for ID {ExportRecordId}: {Message}", exportRecordId, ex.Message);
					baselineHierarchyRecord = null;
				}

				if (baselineHierarchyRecord != null)
				{
					var rootRecordType = baselineHierarchyRecord.RecordType?.ToLowerInvariant();

					if (rootRecordType == "baselineitemz"
						&& exportIncludedBaselineItemzOnly
						&& baselineHierarchyRecord.IsIncluded == false)
					{
						return NotFound($"Requested BaselineItemz (ID: {exportRecordId}) is excluded.");
					}

					var baselineHierarchyTree = await _baselineHierarchyRepository.GetAllChildrenOfBaselineItemzHierarchy(
						exportRecordId, exportIncludedBaselineItemzOnly);

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

					var exportedBaselineItemzIds = CollectExportedIdsByType(rootNode, "BaselineItemz");
					var baselineItemzTraces = await _baselineItemzTraceExportService.GetTracesForExportAsync(exportedBaselineItemzIds);

					//NestedBaselineHierarchyIdRecordDetailsDTO rootNodeToExport;

					//if (showTraceabilityOnly)
					//{
					//	var tracedBaselineIds = new HashSet<Guid>(
					//			baselineItemzTraces.SelectMany(t => new[] { t.FromTraceBaselineItemzId, t.ToTraceBaselineItemzId}));

					//	rootNodeToExport = FilterBaselineForTraceability(rootNode, tracedBaselineIds) ?? rootNode;
					//}
					//else
					//{
					//	rootNodeToExport = rootNode;
					//}

					//var mermaidText = MermaidExporter.GenerateBaseline(rootNodeToExport, baselineItemzTraces, exportRecordId, baseUrl);


					NestedBaselineHierarchyIdRecordDetailsDTO rootNodeToExport;

					if (showTraceabilityOnly)
					{
						var tracedBaselineIds = new HashSet<Guid>(
							baselineItemzTraces.SelectMany(t => new[] { t.FromTraceBaselineItemzId, t.ToTraceBaselineItemzId }));

						if (tracedBaselineIds.Count > 0)
						{
							rootNodeToExport = FilterBaselineForTraceability(rootNode, tracedBaselineIds) ?? rootNode;
						}
						else
						{
							// No trace entries found, export only the root node without children
							rootNodeToExport = new NestedBaselineHierarchyIdRecordDetailsDTO
							{
								RecordId = rootNode.RecordId,
								BaselineHierarchyId = rootNode.BaselineHierarchyId,
								Level = rootNode.Level,
								RecordType = rootNode.RecordType,
								Name = rootNode.Name,
								isIncluded = rootNode.isIncluded,
								Children = new List<NestedBaselineHierarchyIdRecordDetailsDTO>() // empty
							};
						}
					}
					else
					{
						rootNodeToExport = rootNode;
					}

					var mermaidText = MermaidExporter.GenerateBaseline(rootNodeToExport, baselineItemzTraces, exportRecordId, baseUrl);


					//var mermaidText = MermaidExporter.GenerateBaseline(rootNode, baselineItemzTraces, exportRecordId, baseUrl);
					return Content(mermaidText, "text/plain");
				}

				// --- Neither hierarchy found ---
				return NotFound($"Record with ID '{exportRecordId}' not found across Itemz OR Baseline Hierarchy data.");
			}
			catch (Exception ex)
			{
				_logger.LogError("Exception during Mermaid export: {Message}", ex.Message);
				return StatusCode(StatusCodes.Status500InternalServerError, "Error exporting Mermaid flowchart.");
			}
		}

		#endregion END ExportMermaidFlowChart


		#region FilterForTraceability

		// EXPLANATION :: This helper method is used to filter the live hierarchy tree so that only records
		// which are part of traceability links are included in the final export. A record is retained if it
		// is directly traced or if it is an ancestor of a traced record. Children are checked recursively
		// and if any child is kept then its parent is also kept. Records that are neither traced nor ancestors
		// of traced records are excluded. This ensures that when the ShowTraceabilityOnly option is enabled
		// the exported Mermaid diagram remains focused on traceability relationships while still preserving
		// the necessary parent chain for context.

		/// <summary>
		/// Filters the live hierarchy tree to include only nodes that are part of traceability links.
		/// </summary>
		/// <remarks>
		/// A node is retained if it is directly traced or if it is an ancestor of a traced node.
		/// Children are processed recursively, and if any child is retained then its parent is also retained.
		/// Nodes that are neither traced nor ancestors of traced nodes are excluded.
		/// </remarks>
		/// <param name="node">
		/// The current hierarchy node being evaluated. This node may represent a Project, ItemzType, or Itemz.
		/// </param>
		/// <param name="tracedIds">
		/// A set of unique identifiers (<see cref="Guid"/>) representing all Itemz records that are part of
		/// traceability links. These identifiers are used to determine which nodes should be retained.
		/// </param>
		/// <returns>
		/// A <see cref="NestedHierarchyIdRecordDetailsDTO"/> representing the filtered node and its children
		/// if the node is retained, or <c>null</c> if the node is excluded.
		/// </returns>
		private NestedHierarchyIdRecordDetailsDTO? FilterForTraceability(
			NestedHierarchyIdRecordDetailsDTO node,
			HashSet<Guid> tracedIds)
		{
			bool keepThis = tracedIds.Contains(node.RecordId);
			var keptChildren = new List<NestedHierarchyIdRecordDetailsDTO>();

			if (node.Children != null)
			{
				foreach (var child in node.Children)
				{
					var filteredChild = FilterForTraceability(child, tracedIds);
					if (filteredChild != null)
					{
						keptChildren.Add(filteredChild);
						keepThis = true; // parent must be kept if child is kept
					}
				}
			}

			if (keepThis)
			{
				return new NestedHierarchyIdRecordDetailsDTO
				{
					RecordId = node.RecordId,
					HierarchyId = node.HierarchyId,
					Level = node.Level,
					RecordType = node.RecordType,
					Name = node.Name,
					Children = keptChildren
				};
			}

			return null; // drop this node entirely
		}

		#endregion


		#region FilterBaselineForTraceability

		// EXPLANATION :: This helper method is used to filter the baseline hierarchy tree so that only records
		// which are part of traceability links are included in the final export. A record is retained if it
		// is directly traced or if it is an ancestor of a traced record. Children are checked recursively
		// and if any child is kept then its parent is also kept. Records that are neither traced nor ancestors
		// of traced records are excluded. This ensures that when the ShowTraceabilityOnly option is enabled
		// the exported Mermaid diagram remains focused on traceability relationships while still preserving
		// the necessary parent chain for context in baseline hierarchies.

		/// <summary>
		/// Filters the baseline hierarchy tree to include only nodes that are part of traceability links.
		/// </summary>
		/// <remarks>
		/// A node is retained if it is directly traced or if it is an ancestor of a traced node.
		/// Children are processed recursively, and if any child is retained then its parent is also retained.
		/// Nodes that are neither traced nor ancestors of traced nodes are excluded.
		/// </remarks>
		/// <param name="node">
		/// The current hierarchy node being evaluated. This node may represent a Baseline, BaselineItemzType,
		/// or BaselineItemz record.
		/// </param>
		/// <param name="tracedIds">
		/// A set of unique identifiers (<see cref="Guid"/>) representing all BaselineItemz records that are part
		/// of traceability links. These identifiers are used to determine which nodes should be retained.
		/// </param>
		/// <returns>
		/// A <see cref="NestedBaselineHierarchyIdRecordDetailsDTO"/> representing the filtered node and its children
		/// if the node is retained, or <c>null</c> if the node is excluded.
		/// </returns>
		private NestedBaselineHierarchyIdRecordDetailsDTO? FilterBaselineForTraceability(
			NestedBaselineHierarchyIdRecordDetailsDTO node,
			HashSet<Guid> tracedIds)
		{
			bool keepThis = tracedIds.Contains(node.RecordId);
			var keptChildren = new List<NestedBaselineHierarchyIdRecordDetailsDTO>();

			if (node.Children != null)
			{
				foreach (var child in node.Children)
				{
					var filteredChild = FilterBaselineForTraceability(child, tracedIds);
					if (filteredChild != null)
					{
						keptChildren.Add(filteredChild);
						keepThis = true;
					}
				}
			}

			if (keepThis)
			{
				return new NestedBaselineHierarchyIdRecordDetailsDTO
				{
					RecordId = node.RecordId,
					BaselineHierarchyId = node.BaselineHierarchyId,
					Level = node.Level,
					RecordType = node.RecordType,
					Name = node.Name,
					isIncluded = node.isIncluded,
					Children = keptChildren
				};
			}

			return null;
		}

		#endregion

	}
}