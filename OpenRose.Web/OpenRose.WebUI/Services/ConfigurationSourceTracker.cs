// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

namespace OpenRose.WebUI.Services;

using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;

/// <summary>
/// Tracks which configuration source was used for a setting.
/// Useful for debugging and auditing configuration precedence.
/// 
/// Follows Microsoft's official configuration provider precedence order:
/// 1. appsettings.json (lowest)
/// 2. appsettings.{Environment}.json
/// 3. User secrets (Development only)
/// 4. Environment variables
/// 5. Command-line arguments (highest)
/// 
/// Reference: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-10.0
/// </summary>
public class ConfigurationSourceTracker
{
	public enum ConfigurationSource
	{
		/// <summary>Default hardcoded value - not configured anywhere</summary>
		Default,

		/// <summary>From appsettings.json</summary>
		AppSettingsJson,

		/// <summary>From appsettings.{Environment}.json (e.g., appsettings.Development.json)</summary>
		AppSettingsEnvironmentJson,

		/// <summary>From User Secrets (Development only)</summary>
		UserSecrets,

		/// <summary>From Environment Variable</summary>
		EnvironmentVariable,

		/// <summary>From Command-line arguments</summary>
		CommandLineArgument,

		/// <summary>Unknown or not determined</summary>
		Unknown
	}

	public string SettingName { get; set; } = string.Empty;
	public string FinalValue { get; set; } = string.Empty;
	public ConfigurationSource Source { get; set; } = ConfigurationSource.Unknown;
	public string? SourceDetail { get; set; } // e.g., "appsettings.Development.json" or "OPENROSE__USERFILESSTORAGE__ROOTPATH"

	public override string ToString()
	{
		return $"Setting: {SettingName} | Value: {FinalValue} | Detail: {SourceDetail ?? "N/A"}";
	}
}

/// <summary>
/// Service to help trace configuration sources during application startup.
/// Respects Microsoft's official configuration provider precedence order.
/// </summary>
public class ConfigurationSourceTrackerService
{
	private readonly ILogger<ConfigurationSourceTrackerService> _logger;
	private readonly IConfigurationRoot _configurationRoot;
	private readonly List<ConfigurationSourceTracker> _trackedConfigurations = new();
	private readonly string _environmentName;

	public ConfigurationSourceTrackerService(
		IConfiguration configuration,
		ILogger<ConfigurationSourceTrackerService> logger)
	{
		_logger = logger;
		_environmentName = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";

		// Cast to IConfigurationRoot to access Providers
		_configurationRoot = configuration as IConfigurationRoot
			?? throw new InvalidOperationException("IConfiguration must be an IConfigurationRoot to track configuration sources");
	}

	/// <summary>
	/// Tracks a configuration value and determines its source.
	/// 
	/// Follows Microsoft's official precedence order:
	/// Checks providers in REVERSE order (last provider wins).
	/// 1. appsettings.json (lowest precedence)
	/// 2. appsettings.{Environment}.json
	/// 3. User secrets
	/// 4. Environment variables
	/// 5. Command-line arguments (highest precedence)
	/// </summary>
	public void TrackConfigurationSource(string configKey, string? finalValue)
	{
		try
		{
			var tracker = new ConfigurationSourceTracker
			{
				SettingName = configKey,
				FinalValue = finalValue ?? "[null/empty]"
			};

			if (string.IsNullOrEmpty(finalValue))
			{
				// No value found anywhere
				tracker.Source = ConfigurationSourceTracker.ConfigurationSource.Default;
			}
			else
			{
				// Check providers in REVERSE order (last provider wins per Microsoft's design)
				var source = DetermineConfigurationFileSource(configKey);
				tracker.Source = source.Source;
				tracker.SourceDetail = source.SourceDetail;
			}

			_trackedConfigurations.Add(tracker);
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Error tracking configuration source for key: {ConfigKey}", configKey);
		}
	}

	/// <summary>
	/// Determines which configuration provider supplied a value.
	/// 
	/// Iterates through providers in REVERSE order because:
	/// - Later providers override earlier ones (per Microsoft's design)
	/// - The LAST provider to contain a key is the one that supplies the final value
	/// 
	/// Precedence order (from lowest to highest):
	/// 1. appsettings.json
	/// 2. appsettings.{Environment}.json
	/// 3. User secrets (Development only)
	/// 4. Environment variables
	/// 5. Command-line arguments
	/// </summary>
	private (ConfigurationSourceTracker.ConfigurationSource Source, string? SourceDetail) DetermineConfigurationFileSource(string configKey)
	{
		try
		{
			// Get all providers from the configuration root
			var providers = _configurationRoot.Providers.ToList();

			// Iterate in REVERSE order - the last provider containing the key is the winner
			for (int i = providers.Count - 1; i >= 0; i--)
			{
				var provider = providers[i];

				// Check if this provider has the key
				if (!provider.TryGet(configKey, out _))
				{
					continue; // Provider doesn't have this key, check next
				}

				// This provider has the key - determine which type it is
				return DetermineProviderType(provider);
			}

			// Key not found in any provider
			return (ConfigurationSourceTracker.ConfigurationSource.Default, null);
		}
		catch (Exception ex)
		{
			_logger.LogDebug(ex, "Error determining configuration file source for key: {ConfigKey}", configKey);
			return (ConfigurationSourceTracker.ConfigurationSource.Unknown, null);
		}
	}

	/// <summary>
	/// Determines the type of configuration provider and returns the appropriate source.
	/// </summary>
	private (ConfigurationSourceTracker.ConfigurationSource Source, string? SourceDetail) DetermineProviderType(IConfigurationProvider provider)
	{
		// Check for Command-line arguments (highest precedence)
		if (provider is CommandLineConfigurationProvider)
		{
			return (ConfigurationSourceTracker.ConfigurationSource.CommandLineArgument, "Command-line argument");
		}

		// Check for Environment Variables
		if (provider is EnvironmentVariablesConfigurationProvider)
		{
			return (ConfigurationSourceTracker.ConfigurationSource.EnvironmentVariable,
				$"Environment variable");
		}

		// Check for User Secrets (Development only)
		if (provider.GetType().Name == "UserSecretsConfigurationProvider")
		{
			return (ConfigurationSourceTracker.ConfigurationSource.UserSecrets, "User secrets");
		}

		// Check for JSON configuration files
		if (provider is JsonConfigurationProvider jsonProvider)
		{
			var path = jsonProvider.Source.Path ?? "unknown";
			var fileName = Path.GetFileName(path);

			// Determine if it's environment-specific or base appsettings.json
			if (fileName.Contains($"appsettings.{_environmentName}.json"))
			{
				return (ConfigurationSourceTracker.ConfigurationSource.AppSettingsEnvironmentJson,
					$"appsettings.{_environmentName}.json");
			}
			else if (fileName.Contains("appsettings.json"))
			{
				return (ConfigurationSourceTracker.ConfigurationSource.AppSettingsJson,
					"appsettings.json");
			}
		}

		// Unknown provider type
		return (ConfigurationSourceTracker.ConfigurationSource.Unknown, provider.GetType().Name);
	}

	/// <summary>
	/// Logs all tracked configurations for audit purposes.
	/// Consolidates all settings into a single log message to reduce clutter.
	/// </summary>
	public void LogAllTrackedConfigurations()
	{
		if (!_trackedConfigurations.Any())
		{
			return;
		}

		var message = new System.Text.StringBuilder();
		message.AppendLine("=== Configuration Sources Audit Log ===");

		foreach (var config in _trackedConfigurations)
		{
			message.AppendLine($"  {config.ToString()}");
		}

		message.Append("=== End Configuration Sources Audit Log ===");

		_logger.LogInformation(message.ToString());
	}

	/// <summary>
	/// Gets all tracked configurations.
	/// </summary>
	public IReadOnlyList<ConfigurationSourceTracker> GetTrackedConfigurations() => _trackedConfigurations.AsReadOnly();

	/// <summary>
	/// Debug helper: Lists all configuration providers at startup.
	/// Only call this during debugging - not called by default.
	/// Useful for understanding your configuration setup.
	/// </summary>
	public void LogAllProvidersForDebugging()
	{
		try
		{
			_logger.LogDebug("=== Registered Configuration Providers (in order) ===");

			var providers = _configurationRoot.Providers.ToList();
			for (int i = 0; i < providers.Count; i++)
			{
				var providerType = providers[i].GetType().Name;
				var providerInfo = GetProviderInfo(providers[i]);

				_logger.LogDebug(
					"  [{Index}] {ProviderType} - {ProviderInfo}",
					i,
					providerType,
					providerInfo);
			}

			_logger.LogDebug("  NOTE: Providers listed LAST have HIGHEST precedence (they override earlier ones)");
			_logger.LogDebug("=== End Configuration Providers ===");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error logging configuration providers");
		}
	}

	/// <summary>
	/// Gets human-readable information about a provider.
	/// </summary>
	private string GetProviderInfo(IConfigurationProvider provider)
	{
		if (provider is JsonConfigurationProvider jsonProvider)
		{
			var path = jsonProvider.Source.Path ?? "unknown";
			var fileName = Path.GetFileName(path);
			return $"File: {fileName}";
		}

		if (provider is EnvironmentVariablesConfigurationProvider)
		{
			return "Environment variables (OpenRose__* or other env vars)";
		}

		if (provider is CommandLineConfigurationProvider)
		{
			return "Command-line arguments";
		}

		if (provider.GetType().Name == "UserSecretsConfigurationProvider")
		{
			return "User Secrets";
		}

		return provider.GetType().Name;
	}
}