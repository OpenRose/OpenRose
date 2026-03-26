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
