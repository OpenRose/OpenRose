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
		private readonly IExportNodeMapper _exportNodeMapper;
		private readonly IProjectRepository _projectRepository;
		private readonly IHierarchyRepository _hierarchyRepository;
		private readonly IExportRepository _exportRepository;
		private readonly IMapper _mapper;
		private readonly ILogger<ExportController> _logger;

		public ExportController(IExportNodeMapper exportNodeMapper, 
								IProjectRepository projectRepository, 
								IExportRepository exportRepository,
								IHierarchyRepository hierarchyRepository,
								IMapper mapper,
								ILogger<ExportController> logger)
		{
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
				// 1. Get Repository root (for RepositoryId)
				var repositoryRecordDto = await _hierarchyRepository.GetRepositoryHierarchyRecord();
				if (repositoryRecordDto == null)
				{
					_logger.LogDebug("{FormattedControllerAndActionNames} No Repository (root) Hierarchy record found for export.",
						ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext));
					return NotFound("Repository (root) not found.");
				}

				// 2. Get the parent record itself
				var parentHierarchyRecord = await _hierarchyRepository.GetHierarchyRecordDetailsByID(exportRecordId);
				if (parentHierarchyRecord == null)
				{
					_logger.LogDebug("{FormattedControllerAndActionNames} Hierarchy record with ID '{ExportRecordId}' not found.",
						ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), exportRecordId);
					return NotFound($"Hierarchy record with ID '{exportRecordId}' not found.");
				}

				// 3. Get all children (descendants)
				var hierarchyTree = await _hierarchyRepository.GetAllChildrenOfItemzHierarchy(exportRecordId);

				// 4. Compose root node
				var rootNode = new NestedHierarchyIdRecordDetailsDTO
				{
					RecordId = parentHierarchyRecord.RecordId,
					HierarchyId = parentHierarchyRecord.HierarchyId,
					Level = parentHierarchyRecord.Level,
					RecordType = parentHierarchyRecord.RecordType,
					Name = parentHierarchyRecord.Name,
					Children = (List<NestedHierarchyIdRecordDetailsDTO>)hierarchyTree.AllRecords
				};

				// 5. Use the record type from parentHierarchyRecord for export
				var exportDto = new RepositoryExportDTO
				{
					RepositoryId = repositoryRecordDto.RecordId
				};

				switch (parentHierarchyRecord.RecordType?.ToLowerInvariant())
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
						return BadRequest($"Unsupported RecordType: {parentHierarchyRecord.RecordType}");
				}

				// 6. Serialize and return file
				var json = System.Text.Json.JsonSerializer.Serialize(
					exportDto,
					new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
				);
				var content = System.Text.Encoding.UTF8.GetBytes(json);
				var fileName = $"RepositoryExport_{parentHierarchyRecord.RecordType}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";

				_logger.LogDebug("{FormattedControllerAndActionNames} Returning {ExportRecordType} export as file: {FileName}",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), parentHierarchyRecord.RecordType, fileName);

				return File(content, "application/json", fileName);

			}
			catch (Exception ex)
			{
				_logger.LogError("{FormattedControllerAndActionNames} Exception during hierarchy export: {Error}",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), ex.Message);
				return StatusCode(StatusCodes.Status500InternalServerError, "Error exporting hierarchy.");
			}
		}

	}
}
