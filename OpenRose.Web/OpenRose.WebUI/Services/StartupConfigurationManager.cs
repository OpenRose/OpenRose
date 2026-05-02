// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

namespace OpenRose.WebUI.Services;

using Microsoft.Extensions.FileProviders;
using OpenRose.WebUI.Configuration;

/// <summary>
/// Manages application startup configuration logging and offline content setup.
/// Encapsulates all configuration-related setup to keep Program.cs clean and manageable.
/// </summary>
public class StartupConfigurationManager
{
	private readonly WebApplication _app;
	private readonly IConfiguration _configuration;
	private readonly ILogger _logger;
	private readonly StartupCapabilitiesService _startupCapabilities;

	public StartupConfigurationManager(
		WebApplication app,
		IConfiguration configuration,
		ILogger logger,
		StartupCapabilitiesService startupCapabilities)
	{
		_app = app;
		_configuration = configuration;
		_logger = logger;
		_startupCapabilities = startupCapabilities;
	}

	/// <summary>
	/// Configures and logs all startup configuration settings.
	/// Call this during application startup, after building the app but before app.Run().
	/// </summary>
	public void ConfigureAndLogStartupSettings()
	{
		try
		{
			var configSourceTracker = _app.Services.GetRequiredService<ConfigurationSourceTrackerService>();

			ConfigureOfflineContent(configSourceTracker);
			ConfigureUserFilesStorage(configSourceTracker);

			// Log all configuration sources ONCE at the end, after all configurations have been tracked
			configSourceTracker.LogAllTrackedConfigurations();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error during startup configuration");
		}
	}

	/// <summary>
	/// Configures and logs offline content settings.
	/// </summary>
	private void ConfigureOfflineContent(ConfigurationSourceTrackerService configSourceTracker)
	{
		try
		{
			if (_startupCapabilities.ServerOfflineAvailable)
			{
				var offlineContentSettings = _configuration.GetSection("OfflineContent").Get<OfflineContentSettings>();
				var offlineContentPathResolver = _app.Services.GetRequiredService<OfflineContentPathResolver>();

				if (offlineContentSettings != null)
				{
					var resolvedPath = offlineContentPathResolver.ResolvedStorageFolderPath ?? "[Not Configured]";

					var offlineContentLogger = new OfflineContentConfigurationLogger(_logger);
					offlineContentLogger.LogOfflineContentConfiguration(
						offlineContentSettings,
						resolvedPath,
						configSourceTracker);
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error during offline content configuration");
		}
	}

	/// <summary>
	/// Configures and logs user files storage settings.
	/// </summary>
	private void ConfigureUserFilesStorage(ConfigurationSourceTrackerService configSourceTracker)
	{
		try
		{
			var openRoseOptions = _configuration.GetSection("OpenRose").Get<OpenRoseOptions>();

			if (openRoseOptions?.UserFilesStorage.Enabled == true)
			{
				ConfigureEnabledUserFilesStorage(openRoseOptions, configSourceTracker);
			}
			else
			{
				var userFilesLogger = new UserFilesStorageConfigurationLogger(_logger);
				userFilesLogger.LogUserFilesStorageDisabled();
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error during user files storage configuration");
		}
	}

	/// <summary>
	/// Handles configuration when user files storage is enabled.
	/// </summary>
	private void ConfigureEnabledUserFilesStorage(
		OpenRoseOptions openRoseOptions,
		ConfigurationSourceTrackerService configSourceTracker)
	{
		var userFilesPath = openRoseOptions.UserFilesStorage.RootPath;
		var originalPath = userFilesPath;

		// If path is relative, make it relative to application base directory
		if (!Path.IsPathRooted(userFilesPath))
		{
			userFilesPath = Path.Combine(AppContext.BaseDirectory, userFilesPath);
		}

		// Convert to absolute path
		userFilesPath = Path.GetFullPath(userFilesPath);

		var userFilesLogger = new UserFilesStorageConfigurationLogger(_logger);

		if (Directory.Exists(userFilesPath))
		{
			// Configure static file serving for user files
			_app.UseStaticFiles(new StaticFileOptions
			{
				FileProvider = new PhysicalFileProvider(userFilesPath),
				RequestPath = "/userfiles"
			});

			// Alternative path
			_app.UseStaticFiles(new StaticFileOptions
			{
				FileProvider = new PhysicalFileProvider(userFilesPath),
				RequestPath = "/media"
			});

			// Log success configuration
			userFilesLogger.LogUserFilesStorageConfigurationSuccess(
				openRoseOptions.UserFilesStorage,
				originalPath,
				userFilesPath,
				_app.Environment.EnvironmentName,
				configSourceTracker);
		}
		else
		{
			// Log warning configuration
			userFilesLogger.LogUserFilesStorageConfigurationWarning(
				originalPath,
				userFilesPath,
				configSourceTracker);
		}
	}
}