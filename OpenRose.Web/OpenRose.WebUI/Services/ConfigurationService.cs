// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;

namespace OpenRose.WebUI.Services
{
	public class ConfigurationService
	{
		public bool IsOpenRoseAPIConfigured { get; private set; }

		// API and WebUI version information
		public string? ApiVersion { get; private set; }
		public string? WebUiVersion { get; set; }

		// User-visible message when versions don't match or API cannot be reached
		public string? ApiVersionMismatchMessage { get; private set; }

		// 🔔 Event to notify Blazor components when state changes
		public event Action? OnChange;

		/// <summary>
		/// Update connection state and notify listeners.
		/// </summary>
		public void SetConnectionState(bool isConfigured, string? apiVersion, string? message)
		{
			IsOpenRoseAPIConfigured = isConfigured;
			ApiVersion = apiVersion;
			ApiVersionMismatchMessage = message;
			NotifyStateChanged();
		}

		/// <summary>
		/// Trigger UI refresh in subscribed components.
		/// </summary>
		public void NotifyStateChanged()
		{
			//Console.WriteLine("ConfigurationService state changed");
			OnChange?.Invoke();
		}
	}
}
