// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0.
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using Humanizer;
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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ItemzApp.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ImportController : ControllerBase
	{
		private readonly IImportService _importService;
		private readonly ILogger<ImportController> _logger;

		public ImportController(IImportService importService, ILogger<ImportController> logger)
		{
			_importService = importService ?? throw new ArgumentNullException(nameof(importService));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		[HttpPost("ImportHierarchy", Name = "__Import_Hierarchy__")]
		[Consumes("multipart/form-data")]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<IActionResult> ImportHierarchy([FromForm] ImportFormDTO form)
		{
			if (form.File == null || form.File.Length == 0)
			{
				return BadRequest("A valid OpenRose export JSON file is required.");
			}

			RepositoryExportDTO? repositoryExportDto;

			try
			{
				using var stream = form.File.OpenReadStream();
				using var reader = new StreamReader(stream, Encoding.UTF8); // EXPLICITE UTF8 Encoding
				var jsonText = await reader.ReadToEndAsync();

				var schemaJson = await System.IO.File.ReadAllTextAsync("Schemas/OpenRose.Export.schema.1.0.json");
				var schema = JSchema.Parse(schemaJson);

				var importJObject = JObject.Parse(jsonText);
				if (!importJObject.IsValid(schema, out IList<string> errors))
				{
					_logger.LogError("Import JSON failed schema validation: {Errors}", string.Join("; ", errors));
					return UnprocessableEntity(new { error = "Import JSON failed schema validation.", details = errors });
				}

				repositoryExportDto = JsonSerializer.Deserialize<RepositoryExportDTO>(jsonText, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});

				if (repositoryExportDto == null)
				{
					return BadRequest("Could not parse the uploaded file as a valid OpenRose export.");
				}

			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to validate or deserialize the uploaded file.");
				return BadRequest("Invalid file format or content. Please upload a valid OpenRose export JSON file.");
			}

			bool hasBaselineItemz =
				repositoryExportDto?.BaselineItemz?.Any() == true;

			bool hasBaselineItemzTypes =
				repositoryExportDto?.BaselineItemzTypes?.Any(t => t.BaselineItemz?.Any() == true) == true;

			bool hasBaselines =
				repositoryExportDto?.Baselines?.Any(b =>
					b.BaselineItemzTypes?.Any(t => t.BaselineItemz?.Any() == true) == true) == true;

			bool needsFiltering = !form.ImportExcludedBaselineItemz &&
								  (hasBaselineItemz || hasBaselineItemzTypes || hasBaselines);

			if (needsFiltering)
			{
				BaselineImportUtility.FilterExcludedBaselineItemzAcrossRepository(
					repositoryExportDto,
					form.ImportExcludedBaselineItemz,
					_logger);
			}


			string detectedType = null;
			if (repositoryExportDto.Projects?.Count > 0) detectedType = "Project";
			else if (repositoryExportDto.ItemzTypes?.Count > 0) detectedType = "ItemzType";
			else if (repositoryExportDto.Itemz?.Count > 0) detectedType = "Itemz";
			else if (repositoryExportDto.Baselines?.Count > 0) detectedType = "Baseline";
			else if (repositoryExportDto.BaselineItemzTypes?.Count > 0) detectedType = "BaselineItemzType";
			else if (repositoryExportDto.BaselineItemz?.Count > 0) detectedType = "BaselineItemz";

			if (string.IsNullOrEmpty(detectedType))
			{
				return BadRequest("Could not detect record type from the uploaded file.");
			}

			var traceValidationErrors = ImportDataTraceValidator.ValidateTraceLinks(repositoryExportDto, detectedType);
			if (traceValidationErrors.Any())
			{
				_logger.LogWarning("Trace validation failed: {Errors}", string.Join("; ", traceValidationErrors));
				return BadRequest(new
				{
					error = "Invalid traceability links detected.",
					details = traceValidationErrors
				});
			}

			var placementDto = new ImportDataPlacementDTO
			{
				TargetParentId = form.TargetParentId,
				AtBottomOfChildNodes = form.AtBottomOfChildNodes,
				FirstItemzTypeId = form.FirstItemzTypeId,
				SecondItemzTypeId = form.SecondItemzTypeId,
				FirstItemzId = form.FirstItemzId,
				SecondItemzId = form.SecondItemzId
			};

			if (!string.Equals(detectedType, "Project", StringComparison.OrdinalIgnoreCase) &&
				!string.Equals(detectedType, "Baseline", StringComparison.OrdinalIgnoreCase))
			{
				var dtoValidatorResult = ImportDataPlacementDTOValidator.ValidatePlacement(detectedType, placementDto);
				if (!dtoValidatorResult.IsValid)
				{
					_logger.LogWarning("Import placement validation failed: {Error}", dtoValidatorResult.ErrorMessage);
					return BadRequest(dtoValidatorResult.ErrorMessage);
				}
			}

			try
			{
				ImportResult result;

				if (string.Equals(detectedType, "Project", StringComparison.OrdinalIgnoreCase))
				{
					result = await _importService.ImportProjectHierarchyAsync(repositoryExportDto, placementDto);
				}
				else if (string.Equals(detectedType, "ItemzType", StringComparison.OrdinalIgnoreCase))
				{
					result = await _importService.ImportItemzTypeHierarchyAsync(repositoryExportDto, placementDto);
				}
				else if (string.Equals(detectedType, "BaselineItemz", StringComparison.OrdinalIgnoreCase))
				{
					result = await _importService.ImportBaselineItemzAsync(repositoryExportDto, placementDto);
				}
				else if (string.Equals(detectedType, "BaselineItemzType", StringComparison.OrdinalIgnoreCase))
				{
					result = await _importService.ImportBaselineItemzTypeAsync(repositoryExportDto, placementDto);
				}
				else if (string.Equals(detectedType, "Baseline", StringComparison.OrdinalIgnoreCase))
				{
					result = await _importService.ImportBaselineAsProjectAsync(repositoryExportDto, placementDto);
				}

				else
				{
					result = await _importService.ImportAsync(repositoryExportDto, detectedType, placementDto);
				}

				if (!result.Success)
				{
					return BadRequest(new
					{
						error = "Import failed.",
						details = result.Errors
					});
				}

				return Ok(new
				{
					message = "Import successful.",
					importedRootId = result.ImportedRootId,
					importSummary = result.ImportSummary,
					importedRecordIdMapping = result.ItemzIdMapping.Select(kvp => new {
						originalId = kvp.Key,
						newId = kvp.Value
					})
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception during import process.");
				return StatusCode(StatusCodes.Status500InternalServerError, "Unexpected error during import.");
			}
		}
	}
}
