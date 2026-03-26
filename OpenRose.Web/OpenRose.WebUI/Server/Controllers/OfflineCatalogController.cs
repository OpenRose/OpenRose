using Microsoft.AspNetCore.Mvc;
using OpenRose.WebUI.Services;

namespace OpenRose.WebUI.Server.Controllers
{
	[ApiController]
	[Route("offline")]
	public class OfflineCatalogController : ControllerBase
	{
		private readonly OfflineCatalogRepository _repository;
		private readonly OfflineContentPathResolver _resolver;
		private readonly ILogger<OfflineCatalogController> _logger;

		public OfflineCatalogController(
			OfflineCatalogRepository repository,
			OfflineContentPathResolver resolver,
			ILogger<OfflineCatalogController> logger)
		{
			_repository = repository;
			_resolver = resolver;
			_logger = logger;
		}

		// ------------------------------------------------------------
		// GET /offline/files
		// Returns list of available JSON files AND folder health info.
		// ------------------------------------------------------------
		[HttpGet("files")]
		public IActionResult GetAvailableFiles()
		{
			try
			{
				var files = _repository.GetAvailableJsonFiles();

				return Ok(new
				{
					folderAvailable = _resolver.IsStorageFolderAvailable,
					resolvedFolder = _resolver.ResolvedStorageFolderPath, // for diagnostics
					files = files
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to list offline JSON files.");
				return StatusCode(500, "Failed to list offline JSON files.");
			}
		}

		// ------------------------------------------------------------
		// GET /offline/active
		// Returns the currently active offline JSON file AND health info.
		// ------------------------------------------------------------
		[HttpGet("active")]
		public IActionResult GetActiveFile()
		{
			try
			{
				var activeFile = _repository.GetActiveOfflineFile();

				bool activeFileExists = false;

				if (_resolver.IsStorageFolderAvailable &&
					activeFile is not null &&
					_repository.GetFullPathForJsonFile(activeFile) is string fullPath &&
					System.IO.File.Exists(fullPath))
				{
					activeFileExists = true;
				}

				return Ok(new
				{
					folderAvailable = _resolver.IsStorageFolderAvailable,
					activeFile = activeFile,
					activeFileExists = activeFileExists
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
		// Returns file CONTENT instead of file PATH.
		// ------------------------------------------------------------
		public class SelectFileRequest
		{
			public string? FileName { get; set; }
		}

		[HttpPost("select")]
		public async Task<IActionResult> SelectActiveFile([FromBody] SelectFileRequest request)
		{
			if (request == null || string.IsNullOrWhiteSpace(request.FileName))
				return BadRequest("FileName is required.");

			try
			{
				if (!_resolver.IsStorageFolderAvailable)
				{
					return BadRequest("Offline storage folder is unavailable.");
				}

				var fullPath = _repository.GetFullPathForJsonFile(request.FileName);

				if (fullPath is null || !System.IO.File.Exists(fullPath))
				{
					return NotFound($"File '{request.FileName}' does not exist on the server.");
				}

				// Save as active file
				await _repository.SetActiveOfflineFileAsync(request.FileName);

				_logger.LogInformation("Active offline JSON file set to: {File}", request.FileName);

				// NEW: Return file CONTENT instead of file PATH
				string jsonContent = await System.IO.File.ReadAllTextAsync(fullPath);

				return Ok(new
				{
					success = true,
					fileName = request.FileName,
					jsonContent = jsonContent
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
