// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenRose.WebUI.Configuration;

namespace OpenRose.WebUI.Services
{
	/// <summary>
	/// EXPLANATION:
	/// Central service that resolves the final storage folder path for OfflineContent.
	/// 
	/// Responsibilities:
	///   - Interpret OfflineContent.StorageFolder as either absolute or relative.
	///   - For relative paths, resolve against a cross-platform base folder
	///     (e.g., Environment.SpecialFolder.ApplicationData).
	///   - Attempt to create the folder if it does not exist.
	///   - Never throw for folder creation failures; instead log a warning and
	///     expose a "folder available" flag.
	/// 
	/// This keeps all path logic in one place and makes OfflineCatalogRepository simpler.
	/// </summary>
	public class OfflineContentPathResolver
	{
		private readonly OfflineContentSettings _settings;
		private readonly ILogger<OfflineContentPathResolver> _logger;

		public string? ResolvedStorageFolderPath { get; private set; }

		/// <summary>
		/// EXPLANATION:
		/// Indicates whether the storage folder is considered usable.
		/// If false, callers should treat server-side JSON as unavailable,
		/// but the application should continue running.
		/// </summary>
		public bool IsStorageFolderAvailable { get; private set; }

		public OfflineContentPathResolver(
			IOptions<OfflineContentSettings> settingsOptions,
			ILogger<OfflineContentPathResolver> logger)
		{
			_settings = settingsOptions.Value;
			_logger = logger;

			ResolveAndEnsureFolder();

			// NEW: Log final resolved folder path at startup
			if (ResolvedStorageFolderPath is null)
			{
				_logger.LogInformation(
					"OfflineContent: StorageFolder is not configured. Server-side JSON mode will be unavailable.");
			}
			else if (!IsStorageFolderAvailable)
			{
				_logger.LogWarning(
					"OfflineContent: StorageFolder resolved to '{Path}', but the folder is unavailable (creation failed or inaccessible).",
					ResolvedStorageFolderPath);
			}
			else
			{
				_logger.LogInformation(
					"OfflineContent: StorageFolder resolved to '{Path}' and is available.",
					ResolvedStorageFolderPath);
			}
		}


		private void ResolveAndEnsureFolder()
		{
			// If no folder configured at all, we simply mark it unavailable.
			if (string.IsNullOrWhiteSpace(_settings.StorageFolder))
			{
				_logger.LogInformation("OfflineContent.StorageFolder is not configured. Server-side JSON mode will be unavailable.");
				ResolvedStorageFolderPath = null;
				IsStorageFolderAvailable = false;
				return;
			}

			string configuredPath = _settings.StorageFolder;

			// EXPLANATION:
			// If the configured path is absolute, we use it as-is.
			// If it is relative, we resolve it under a cross-platform base folder.
			string finalPath;
			if (Path.IsPathRooted(configuredPath))
			{
				finalPath = configuredPath;
			}
			else
			{
				// EXPLANATION:
				// Use a cross-platform application data folder as base.
				// On Windows:  C:\Users\<User>\AppData\Roaming
				// On Linux:    /home/<user>/.config
				// On macOS:    /Users/<user>/Library/Application Support
				var basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

				if (string.IsNullOrWhiteSpace(basePath))
				{
					// Fallback: if for some reason ApplicationData is not available,
					// use the current directory as a last resort.
					basePath = AppContext.BaseDirectory;
					_logger.LogWarning("Environment.SpecialFolder.ApplicationData returned empty. Falling back to AppContext.BaseDirectory: {BaseDirectory}", basePath);
				}

				finalPath = Path.Combine(basePath, configuredPath);
			}

			ResolvedStorageFolderPath = finalPath;

			try
			{
				if (!Directory.Exists(finalPath))
				{
					// EXPLANATION:
					// Try to create the folder. If this fails, we log a warning
					// and mark the folder as unavailable, but we do NOT throw.
					Directory.CreateDirectory(finalPath);
					_logger.LogInformation("Offline storage folder created or confirmed at: {Path}", finalPath);
				}

				IsStorageFolderAvailable = true;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex,
					"Offline storage folder could not be created or accessed at: {Path}. Server-side JSON files will not be available.",
					finalPath);

				IsStorageFolderAvailable = false;
			}
		}
	}
}
