// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0.
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using Microsoft.AspNetCore.Mvc;
using OpenRose.WebUI.Services;

namespace OpenRose.WebUI.Server.Controllers
{
	[ApiController]
	[Route("offline")]
	public class OfflineCatalogController : ControllerBase
	{
		private readonly OfflineCatalogRepository _repository;
		private readonly ILogger<OfflineCatalogController> _logger;

		public OfflineCatalogController(
			OfflineCatalogRepository repository,
			ILogger<OfflineCatalogController> logger)
		{
			_repository = repository;
			_logger = logger;
		}

		// ------------------------------------------------------------
		// GET /offline/files
		// Returns list of available JSON files in the storage folder.
		// ------------------------------------------------------------
		[HttpGet("files")]
		public IActionResult GetAvailableFiles()
		{
			try
			{
				var files = _repository.GetAvailableJsonFiles();
				return Ok(files);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to list offline JSON files.");
				return StatusCode(500, "Failed to list offline JSON files.");
			}
		}

		// ------------------------------------------------------------
		// GET /offline/active
		// Returns the currently active offline JSON file.
		// ------------------------------------------------------------
		[HttpGet("active")]
		public IActionResult GetActiveFile()
		{
			try
			{
				var activeFile = _repository.GetActiveOfflineFile();

				return Ok(new
				{
					activeFile = activeFile
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to get active offline JSON file.");
				return StatusCode(500, "Failed to get active offline JSON file.");
			}
		}

		// ------------------------------------------------------------
		// POST /offline/select
		// Sets the active offline JSON file.
		// Validates that the file exists before saving.
		// ------------------------------------------------------------
		public class SelectFileRequest
		{
			public string? FileName { get; set; }
		}

		[HttpPost("select")]
		public async Task<IActionResult> SelectActiveFile([FromBody] SelectFileRequest request)
		{
			if (request == null || string.IsNullOrWhiteSpace(request.FileName))
			{
				return BadRequest("FileName is required.");
			}

			try
			{
				// Validate file exists
				var fullPath = _repository.GetFullPathForJsonFile(request.FileName);

				if (!System.IO.File.Exists(fullPath))
				{
					return NotFound($"File '{request.FileName}' does not exist on the server.");
				}

				// Save as active file
				await _repository.SetActiveOfflineFileAsync(request.FileName);

				_logger.LogInformation("Active offline JSON file set to: {File}", request.FileName);

				return Ok(new
				{
					success = true,
					activeFile = request.FileName
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to set active offline JSON file.");
				return StatusCode(500, "Failed to set active offline JSON file.");
			}
		}
	}
}
