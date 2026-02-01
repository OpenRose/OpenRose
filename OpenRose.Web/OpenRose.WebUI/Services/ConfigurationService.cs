// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

namespace OpenRose.WebUI.Services
{
    public class ConfigurationService
    {
        public bool IsOpenRoseAPIConfigured { get; set; }

		// API and WebUI version information
		public string? ApiVersion { get; set; }
		public string? WebUiVersion { get; set; }

		// User-visible message when versions don't match or API cannot be reached
		public string? ApiVersionMismatchMessage { get; set; }
	}
}
