// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using Microsoft.Extensions.Options;
using OpenRose.WebUI.Client.Services.JsonFile;
using OpenRose.WebUI.Client.SharedModels.ClientSideUIOnlyModel;
using OpenRose.WebUI.Components.EventServices;
using OpenRose.WebUI.Configuration;

namespace OpenRose.WebUI.Services
{
	/// <summary>
	/// EXPLANATION:
	/// This service determines whether the WebUI should start in:
	///   - API Server Mode
	///   - Offline JSON Mode
	///   - Error Mode (if neither is viable)
	///
	/// It uses:
	///   - APISettings (to check if API is configured)
	///   - OfflineContentSettings (to locate offline JSON files)
	///   - OfflineCatalogRepository (to load active offline file)
	///   - JsonFileSchemaValidationService (to validate JSON)
	///   - JsonFileDataSourceService (to load JSON into memory)
	///   - DataSourceStateService (to update global state)
	///
	/// This resolver is executed during application startup.
	/// </summary>
	public class OfflineStartupResolver
	{
		private readonly APISettings _apiSettings;
		private readonly OfflineContentSettings _offlineSettings;
		private readonly OfflineCatalogRepository _offlineCatalogRepository;
		private readonly JsonFileSchemaValidationService _jsonSchemaValidator;
		private readonly JsonFileDataSourceService _jsonDataSourceService;
		private readonly DataSourceStateService _dataSourceStateService;
		private readonly StartupCapabilitiesService _capabilities;
		private readonly ILogger<OfflineStartupResolver> _logger;

		public enum StartupResult
		{
			ApiMode,
			OfflineMode,
			Error
		}

		public OfflineStartupResolver(
			IOptions<APISettings> apiSettingsOptions,
			IOptions<OfflineContentSettings> offlineSettingsOptions,
			OfflineCatalogRepository offlineCatalogRepository,
			JsonFileSchemaValidationService jsonSchemaValidator,
			JsonFileDataSourceService jsonDataSourceService,
			DataSourceStateService dataSourceStateService,
			StartupCapabilitiesService capabilities,
			ILogger<OfflineStartupResolver> logger)
		{
			_apiSettings = apiSettingsOptions.Value;
			_offlineSettings = offlineSettingsOptions.Value;
			_offlineCatalogRepository = offlineCatalogRepository;
			_jsonSchemaValidator = jsonSchemaValidator;
			_jsonDataSourceService = jsonDataSourceService;
			_dataSourceStateService = dataSourceStateService;
			_capabilities = capabilities;
			_logger = logger;
		}

		/// <summary>
		/// EXPLANATION:
		/// Main entry point for startup resolution.
		/// Determines whether to start in API mode or Offline JSON mode.
		/// </summary>
		public async Task<StartupResult> ResolveStartupModeAsync()
		{
			try
			{
				// ============================================================
				// STEP 0: Determine availability from configuration
				// ============================================================
				bool apiAvailable = _capabilities.ApiAvailable;
				bool offlineAvailable = _capabilities.OfflineAvailable;

				// ============================================================
				// STEP 1: Read StartupMode setting
				// ============================================================
				string mode = _offlineSettings.StartupMode?.Trim().ToUpperInvariant() ?? "AUTO";

				// ============================================================
				// STEP 2: LIVE mode → API only
				// ============================================================
				if (mode == "LIVE")
				{
					_logger.LogInformation("StartupMode = LIVE → API required.");

					if (apiAvailable)
						return StartupResult.ApiMode;

					_logger.LogWarning("LIVE mode requested but API is not available.");
					return StartupResult.Error;
				}

				// ============================================================
				// STEP 3: OFFLINE mode → Offline only
				// ============================================================
				if (mode == "OFFLINE")
				{
					_logger.LogInformation("StartupMode = OFFLINE → Offline JSON required.");

					if (offlineAvailable)
						return await TryStartOfflineModeAsync();

					_logger.LogWarning("OFFLINE mode requested but OfflineContent is not available.");
					return StartupResult.Error;
				}

				// ============================================================
				// STEP 4: AUTO mode → API first, then Offline
				// ============================================================
				if (mode == "AUTO")
				{
					_logger.LogInformation("StartupMode = AUTO → Checking API first.");

					if (apiAvailable)
						return StartupResult.ApiMode;

					if (offlineAvailable)
						return await TryStartOfflineModeAsync();

					_logger.LogWarning("AUTO mode: Neither API nor OfflineContent is available.");
					return StartupResult.Error;
				}

				// ============================================================
				// STEP 5: Unknown mode → treat as error
				// ============================================================
				_logger.LogWarning("Unknown StartupMode value: {Mode}", mode);
				return StartupResult.Error;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unexpected error during startup mode resolution.");
				return StartupResult.Error;
			}
		}


		/// <summary>
		/// EXPLANATION:
		/// Attempts to load the active offline JSON file.
		/// Validates the file and loads it into memory.
		/// Updates DataSourceStateService accordingly.
		/// </summary>
		private async Task<StartupResult> TryStartOfflineModeAsync()
		{
			try
			{
				string? activeFileName = _offlineCatalogRepository.GetActiveOfflineFile();

				if (string.IsNullOrWhiteSpace(activeFileName))
				{
					_logger.LogWarning("No active offline JSON file found.");
					return StartupResult.Error;
				}

				string fullPath = _offlineCatalogRepository.GetFullPathForJsonFile(activeFileName);

				if (!File.Exists(fullPath))
				{
					_logger.LogWarning("Active offline JSON file does not exist: {File}", fullPath);
					return StartupResult.Error;
				}

				// ============================================================
				// STEP 1: Read JSON file
				// ============================================================
				string jsonContent = await File.ReadAllTextAsync(fullPath);

				// ============================================================
				// STEP 2: Validate JSON schema
				// ============================================================
				_jsonSchemaValidator.ValidateJsonFileContentAgainstSchema(jsonContent);

				// ============================================================
				// STEP 3: Load JSON into memory
				// ============================================================
				await _jsonDataSourceService.LoadJsonFileDataFromStringAsync(jsonContent);

				// STEP 4: Set metadata for server-side JSON
				_jsonDataSourceService.SetJsonViewerMetadata(new JsonViewerMetadata
				{
					FileName = Path.GetFileName(fullPath),
					FullPath = fullPath,
					FileSizeBytes = new FileInfo(fullPath).Length,
					LoadedAt = DateTime.Now,
					IsServerSide = true
				});

				// ============================================================
				// STEP 5: Update global state
				// ============================================================
				_dataSourceStateService.SwitchToServerSideJsonFile(fullPath);

				_logger.LogInformation("Successfully started in Offline JSON mode using file: {File}", fullPath);

				return StartupResult.OfflineMode;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to start in Offline JSON mode.");
				return StartupResult.Error;
			}
		}
	}
}
