// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

namespace OpenRose.WebUI.Services;

using OpenRose.WebUI.Configuration;

/// <summary>
/// Logs user files storage configuration details for debugging and auditing purposes.
/// Consolidates output into single cohesive log messages to reduce clutter.
/// </summary>
public class UserFilesStorageConfigurationLogger
{
	private readonly ILogger _logger;

	public UserFilesStorageConfigurationLogger(ILogger logger)
	{
		_logger = logger;
	}

	/// <summary>
	/// Logs detailed information about the user files storage configuration when directory exists.
	/// This should be called during application startup.
	/// </summary>
	public void LogUserFilesStorageConfigurationSuccess(
		UserFilesStorageOptions userFilesStorage,
		string originalPath,
		string resolvedPath,
		string environmentName,
		ConfigurationSourceTrackerService configSourceTracker)
	{
		try
		{
			// Track the configuration sources
			configSourceTracker.TrackConfigurationSource(
				"OpenRose:UserFilesStorage:Enabled",
				userFilesStorage.Enabled.ToString());

			configSourceTracker.TrackConfigurationSource(
				"OpenRose:UserFilesStorage:RootPath",
				originalPath);

			// Build and log consolidated user files status
			var statusMessage = BuildUserFilesStorageStatusMessage(
				userFilesStorage,
				originalPath,
				resolvedPath,
				environmentName);

			_logger.LogInformation(statusMessage);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error logging user files storage configuration");
		}
	}

	/// <summary>
	/// Logs detailed information about the user files storage configuration when directory does NOT exist.
	/// </summary>
	public void LogUserFilesStorageConfigurationWarning(
		string originalPath,
		string resolvedPath,
		ConfigurationSourceTrackerService configSourceTracker)
	{
		try
		{
			// Track the configuration sources
			configSourceTracker.TrackConfigurationSource(
				"OpenRose:UserFilesStorage:Enabled",
				"True");

			configSourceTracker.TrackConfigurationSource(
				"OpenRose:UserFilesStorage:RootPath",
				originalPath);

			var warningMessage = BuildUserFilesStorageWarningMessage(
				originalPath,
				resolvedPath);

			_logger.LogWarning(warningMessage);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error logging user files storage warning");
		}
	}

	/// <summary>
	/// Logs when user files storage is disabled.
	/// </summary>
	public void LogUserFilesStorageDisabled()
	{
		try
		{
			_logger.LogInformation("User files storage is DISABLED. (OpenRose:UserFilesStorage:Enabled = false)");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error logging user files storage disabled");
		}
	}

	/// <summary>
	/// Builds a comprehensive single-message status of user files storage configuration.
	/// </summary>
	private string BuildUserFilesStorageStatusMessage(
		UserFilesStorageOptions userFilesStorage,
		string originalPath,
		string resolvedPath,
		string environmentName)
	{
		var message = new System.Text.StringBuilder();

		message.AppendLine("=== User Files Storage Configuration ===");
		message.AppendLine($"  Enabled Setting: {userFilesStorage.Enabled}");
		message.AppendLine($"  RootPath Setting: {originalPath}");
		message.AppendLine($"  Resolved Full Path: {resolvedPath}");

		// Check directory details
		if (Directory.Exists(resolvedPath))
		{
			var directoryInfo = new System.IO.DirectoryInfo(resolvedPath);
			var fileCount = directoryInfo.GetFiles().Length;
			var subdirCount = directoryInfo.GetDirectories().Length;

			message.AppendLine($"  Directory Status: EXISTS | Files: {fileCount} | Subdirectories: {subdirCount}");
			message.AppendLine($"  Accessible via: /userfiles, /media");
			message.AppendLine($"  Environment: {environmentName}");
		}

		message.Append("=== End User Files Storage Configuration ===");

		return message.ToString();
	}

	/// <summary>
	/// Builds a warning message when user files storage directory does not exist.
	/// </summary>
	private string BuildUserFilesStorageWarningMessage(
		string originalPath,
		string resolvedPath)
	{
		var message = new System.Text.StringBuilder();

		message.AppendLine("=== User Files Storage Configuration ===");
		message.AppendLine($"  RootPath Setting: {originalPath}");
		message.AppendLine($"  Resolved Full Path: {resolvedPath}");
		message.AppendLine($"  Directory Status: DOES NOT EXIST");
		message.AppendLine();
		message.AppendLine($"  ERROR: User files storage path does not exist.");
		message.AppendLine($"  ACTION REQUIRED: Create this directory and place your image files there,");
		message.AppendLine($"  then restart OpenRose.WebUI for the changes to take effect.");
		message.AppendLine($"  User files serving is DISABLED until the directory is created.");
		message.Append("=== End User Files Storage Configuration ===");

		return message.ToString();
	}
}