// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

namespace OpenRose.WebUI.Services;

using OpenRose.WebUI.Configuration;

/// <summary>
/// Logs offline content configuration details for debugging and auditing purposes.
/// </summary>
public class OfflineContentConfigurationLogger
{
	private readonly ILogger _logger;

	public OfflineContentConfigurationLogger(ILogger logger)
	{
		_logger = logger;
	}

	/// <summary>
	/// Logs detailed information about the offline content storage configuration.
	/// This should be called during application startup.
	/// </summary>
	public void LogOfflineContentConfiguration(
		OfflineContentSettings offlineContentSettings,
		string resolvedFolderPath,
		ConfigurationSourceTrackerService configSourceTracker)
	{
		try
		{
			_logger.LogInformation("=== Offline Content Configuration ===");

			// Track the configuration sources
			configSourceTracker.TrackConfigurationSource(
				"OfflineContent:StorageFolder",
				offlineContentSettings.StorageFolder);

			configSourceTracker.TrackConfigurationSource(
				"OfflineContent:DefaultJsonFile",
				offlineContentSettings.DefaultJsonFile);

			// Log the settings as configured
			_logger.LogInformation(
				"Offline Content - StorageFolder Setting: {StorageFolder}",
				offlineContentSettings.StorageFolder);

			_logger.LogInformation(
				"Offline Content - DefaultJsonFile Setting: {DefaultJsonFile}",
				offlineContentSettings.DefaultJsonFile);

			// Log the resolved path
			_logger.LogInformation(
				"Offline Content - Resolved Full Path: {ResolvedPath}",
				resolvedFolderPath);

			// Check if directory exists
			if (Directory.Exists(resolvedFolderPath))
			{
				var directoryInfo = new DirectoryInfo(resolvedFolderPath);
				var fileCount = directoryInfo.GetFiles().Length;
				var subdirCount = directoryInfo.GetDirectories().Length;

				_logger.LogInformation(
					"Offline Content - Directory Status: EXISTS | Files: {FileCount} | Subdirectories: {SubdirCount}",
					fileCount,
					subdirCount);

				// Log files in the directory
				var files = directoryInfo.GetFiles("*.json", SearchOption.TopDirectoryOnly);
				if (files.Any())
				{
					_logger.LogInformation(
						"Offline Content - JSON Files Found: {FileCount}",
						files.Length);

					foreach (var file in files)
					{
						_logger.LogDebug(
							"Offline Content - JSON File: {FileName} | Size: {FileSizeKB} KB | Modified: {LastModified}",
							file.Name,
							file.Length / 1024,
							file.LastWriteTime);
					}
				}
			}
			else
			{
				_logger.LogWarning(
					"Offline Content - Directory Status: DOES NOT EXIST. " +
					"Server-side offline JSON file loading is DISABLED. " +
					"Create the directory or configure a different path.");
			}

			_logger.LogInformation("=== End Offline Content Configuration ===");

			//// Log configuration sources
			//configSourceTracker.LogAllTrackedConfigurations();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error logging offline content configuration");
		}
	}
}
