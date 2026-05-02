// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

namespace OpenRose.WebUI.Services;

using OpenRose.WebUI.Configuration;

/// <summary>
/// Logs offline content configuration details for debugging and auditing purposes.
/// Consolidates output into single cohesive log messages to reduce clutter.
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
			// Track the configuration sources
			configSourceTracker.TrackConfigurationSource(
				"OfflineContent:StorageFolder",
				offlineContentSettings.StorageFolder);

			configSourceTracker.TrackConfigurationSource(
				"OfflineContent:DefaultJsonFile",
				offlineContentSettings.DefaultJsonFile);

			// Build comprehensive status message
			var statusMessage = BuildOfflineContentStatusMessage(
				offlineContentSettings,
				resolvedFolderPath);

			_logger.LogInformation(statusMessage);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error logging offline content configuration");
		}
	}

	/// <summary>
	/// Builds a comprehensive single-message status of offline content configuration.
	/// </summary>
	private string BuildOfflineContentStatusMessage(
		OfflineContentSettings offlineContentSettings,
		string resolvedFolderPath)
	{
		var message = new System.Text.StringBuilder();

		message.AppendLine("=== Offline Content Configuration ===");
		message.AppendLine($"  StorageFolder Setting: {offlineContentSettings.StorageFolder}");
		message.AppendLine($"  DefaultJsonFile Setting: {offlineContentSettings.DefaultJsonFile}");
		message.AppendLine($"  Resolved Full Path: {resolvedFolderPath}");

		// Check if directory exists and get details
		if (Directory.Exists(resolvedFolderPath))
		{
			var directoryInfo = new DirectoryInfo(resolvedFolderPath);
			var fileCount = directoryInfo.GetFiles().Length;
			var subdirCount = directoryInfo.GetDirectories().Length;
			var jsonFiles = directoryInfo.GetFiles("*.json", SearchOption.TopDirectoryOnly);

			message.AppendLine($"  Directory Status: EXISTS | Files: {fileCount} | Subdirectories: {subdirCount}");

			if (jsonFiles.Any())
			{
				message.AppendLine($"  JSON Files Found: {jsonFiles.Length}");
				foreach (var file in jsonFiles)
				{
					var sizeKB = file.Length / 1024;
					message.AppendLine($"    - {file.Name} ({sizeKB} KB)");
				}
			}
		}
		else
		{
			message.AppendLine($"  Directory Status: DOES NOT EXIST");
			message.AppendLine($"  WARNING: Server-side offline JSON file loading is DISABLED.");
		}

		message.Append("=== End Offline Content Configuration ===");

		return message.ToString();
	}
}