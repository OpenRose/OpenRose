// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0.
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OpenRose.WebUI.Services
{
	/// <summary>
	/// EXPLANATION:
	/// Centralized service that determines which startup capabilities
	/// are available based solely on configuration sections.
	///
	/// IMPORTANT:
	/// - Does NOT check filesystem.
	/// - Does NOT check API connectivity.
	/// - Does NOT validate JSON files.
	///
	/// It simply answers:
	///   - Is API mode even possible?
	///   - Is Offline mode even possible?
	///
	/// This allows UI components and the startup resolver to make
	/// consistent decisions without duplicating logic.
	/// </summary>
	public class StartupCapabilitiesService
	{
		public bool ApiAvailable { get; }
		public bool ServerOfflineAvailable { get; }

		public StartupCapabilitiesService(IConfiguration configuration,
										  ILogger<StartupCapabilitiesService>? logger = null)
		{
			// If logger was not provided (because Program.cs manually constructs this service),
			// create a temporary logger using the same logging infrastructure.
			logger ??= LoggerFactory.Create(builder =>
			{
				builder.AddConsole();
				builder.AddDebug();
			}).CreateLogger<StartupCapabilitiesService>();


			// API is considered available if the section exists AND BaseUrl is non-empty.
			var apiSection = configuration.GetSection("APISettings");
			var rawBaseUrl = apiSection["BaseUrl"];

			var normalizedBaseUrl = NormalizeConfigValue(rawBaseUrl);

			// API is available only if normalized BaseUrl is non-empty
			ApiAvailable = !string.IsNullOrWhiteSpace(normalizedBaseUrl);

			// LOGGING: Provide a single, clear diagnostic entry for API configuration
			var apiStatusMessage = BuildApiConfigurationStatusMessage(
				rawBaseUrl ?? "(null)",
				normalizedBaseUrl,
				ApiAvailable);

			logger.LogInformation(apiStatusMessage);


			#region DELETE ME NEXT TIME
			//// Offline is considered available if the section exists,
			//// regardless of folder/file validity.
			//var serverOfflineSection = configuration.GetSection("OfflineContent");
			//ServerOfflineAvailable =
			//	serverOfflineSection.Exists() &&
			//	!string.IsNullOrWhiteSpace(serverOfflineSection["StorageFolder"]);
			#endregion

			// Offline is considered available if the section exists,
			// regardless of folder/file validity.
			var serverOfflineSection = configuration.GetSection("OfflineContent");
			var rawStorageFolder = serverOfflineSection["StorageFolder"];

			var normalizedStorageFolder = NormalizeConfigValue(rawStorageFolder);

			// Offline is available only if normalized StorageFolder is non-empty
			ServerOfflineAvailable = !string.IsNullOrWhiteSpace(normalizedStorageFolder);

		}

		private string BuildApiConfigurationStatusMessage(string rawBaseUrl,
														  string normalizedBaseUrl,
														  bool apiAvailable)
		{
			var message = new System.Text.StringBuilder();

			message.AppendLine("=== API Configuration ===");
			message.AppendLine($"  Raw BaseUrl: {rawBaseUrl ?? "(null)"}");
			message.AppendLine($"  Normalized BaseUrl: {normalizedBaseUrl}");
			message.AppendLine($"  ApiAvailable: {apiAvailable}");
			message.Append("=== End API Configuration ===");

			return message.ToString();
		}

		/// <summary>
		/// EXPLANATION:
		/// Normalizes configuration values that may come from environment variables,
		/// appsettings.json, or other sources where empty values may be represented as:
		///   - ""
		///   - "''"
		///   - "\"\""
		///   - whitespace
		///   - null
		///
		/// This ensures consistent behavior across all configuration inputs.
		/// </summary>
		private static string NormalizeConfigValue(string? value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return string.Empty;

			var trimmed = value.Trim();

			// Treat quoted empty strings as empty
			if (trimmed == "\"\"" || trimmed == "''")
				return string.Empty;

			return trimmed;
		}
	}
}
