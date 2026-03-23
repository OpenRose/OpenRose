// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0.
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using Microsoft.Extensions.Configuration;

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

		public StartupCapabilitiesService(IConfiguration configuration)
		{
			// API is considered available if the section exists AND BaseUrl is non-empty.
			var apiSection = configuration.GetSection("APISettings");
			ApiAvailable = apiSection.Exists() &&
						   !string.IsNullOrWhiteSpace(apiSection["BaseUrl"]);

			// Offline is considered available if the section exists,
			// regardless of folder/file validity.
			var serverOfflineSection = configuration.GetSection("OfflineContent");
			ServerOfflineAvailable = serverOfflineSection.Exists() &&
				   !string.IsNullOrEmpty(serverOfflineSection["StorageFolder"]);
		}
	}
}
