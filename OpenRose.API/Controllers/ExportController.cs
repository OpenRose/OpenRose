// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ItemzApp.API.Services;
using ItemzApp.API.Models;

namespace ItemzApp.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")] // e.g. http://HOST:PORT/api/export
	public class ExportController : ControllerBase
	{
		private readonly IExportRepository _exportRepository;
		private readonly IMapper _mapper;
		private readonly ILogger<ExportController> _logger;

		public ExportController(IExportRepository exportRepository,
								IMapper mapper,
								ILogger<ExportController> logger)
		{
			_exportRepository = exportRepository ?? throw new ArgumentNullException(nameof(exportRepository));
			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// Exports data based on the selected entity type (Project, ItemzType, Itemz).
		/// </summary>
		/// <param name="entityId">Unique identifier for the selected entity.</param>
		/// <returns>Returns exported JSON file.</returns>
		[HttpGet("{entityType}/{entityId}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> ExportData(Guid entityId)
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


			_logger.LogInformation($"Export requested for ID {entityId}");

			try
			{
				string filePath = await _exportRepository.GenerateExportFileAsync(entityId);

				if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
				{
					_logger.LogWarning($"Export failed: File not found for ID {entityId}");
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
	}
}
