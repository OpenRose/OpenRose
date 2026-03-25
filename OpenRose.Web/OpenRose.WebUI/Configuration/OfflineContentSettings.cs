// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

namespace OpenRose.WebUI.Configuration
{
	/// <summary>
	/// EXPLANATION:
	/// This class represents the configuration section "OfflineContent" from appsettings.json.
	/// It defines where server-side JSON files are stored and how offline mode behaves at startup.
	/// </summary>
	public class OfflineContentSettings
	{
		public string StorageFolder { get; set; } = string.Empty;

		public string DefaultJsonFile { get; set; } = string.Empty;

		///// <summary>
		///// EXPLANATION:
		///// StartupMode controls how the WebUI decides between API mode and Offline JSON mode.
		///// Allowed values:
		/////   "AUTO"    → Try API first, fallback to offline if API unavailable.
		/////   "LIVE"    → Always use API mode.
		/////   "OFFLINE" → Always use offline JSON mode.
		///// </summary>
		//public string StartupMode { get; set; } = "AUTO";
	}
}
