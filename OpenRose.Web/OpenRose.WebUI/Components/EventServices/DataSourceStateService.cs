// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;

namespace OpenRose.WebUI.Components.EventServices
{
	/// <summary>
	/// NEW SERVICE FOR JSON FILE DATA SOURCE FEATURE
	/// 
	/// DataSourceStateService is a global event service that tracks the current data source
	/// being used by the OpenRose WebUI application. This service manages state transitions
	/// between API-based data source and JSON File-based read-only data source.
	/// 
	/// The service maintains state information that is used across the application to:
	/// - Determine which UI elements should be visible or hidden
	/// - Control the color of the application title bar (Blue for API, Gray/Orange for JSON)
	/// - Manage navigation menu visibility and options
	/// - Enable/disable routing to certain components
	/// - Control which data retrieval services are used in WebUI.Client
	/// 
	/// Usage Pattern:
	/// 1. Inject this service where you need to monitor or change data source state
	/// 2. Subscribe to OnDataSourceChanged event to react to state changes
	/// 3. Call SwitchToJsonFileDataSourceAsync() or SwitchToApiDataSourceAsync() to change sources
	/// 4. Access CurrentDataSourceState property to get current state information
	/// </summary>
	public class DataSourceStateService
	{
		// ========================================================================
		// EXISTING CODE PATTERN REFERENCE:
		// This service follows the same observer pattern as ConfigurationService and FormStateService
		// ========================================================================

		/// <summary>
		/// Enumeration representing the type of data source being used.
		/// </summary>
		public enum DataSourceType
		{
			/// <summary>
			/// Connected to OpenRose API Server - Full CRUD operations supported
			/// </summary>
			ApiServer,

			/// <summary>
			/// Connected to local JSON file - Read-Only operations only
			/// </summary>
			JsonFile
		}

		/// <summary>
		/// Data class that encapsulates all state information about the current data source.
		/// This state is used throughout the application to determine behavior and UI rendering.
		/// </summary>
		public class DataSourceState
		{
			/// <summary>
			/// Type of data source currently active: API or JSON File
			/// </summary>
			public DataSourceType CurrentDataSourceType { get; set; } = DataSourceType.ApiServer;

			/// <summary>
			/// Indicates whether the current data source is read-only.
			/// True when using JSON File, False when using API.
			/// </summary>
			public bool IsReadOnlyMode { get; set; } = false;

			/// <summary>
			/// Full file path (UNC or local) to the currently loaded JSON file.
			/// This is null/empty when data source is API.
			/// Example: "\\\\server\\share\\openrose_export_20260214_120000.json"
			/// or "C:\\Exports\\openrose_export_20260214_120000.json"
			/// </summary>
			public string? JsonFilePathForDataSource { get; set; }

			/// <summary>
			/// File name (without path) of the currently loaded JSON file for display purposes.
			/// Example: "openrose_export_20260214_120000.json"
			/// </summary>
			public string? JsonFileNameForDisplay { get; set; }

			/// <summary>
			/// User-facing error message if JSON file validation or loading failed.
			/// This is displayed in error dialogs to inform the user about what went wrong.
			/// </summary>
			public string? LastErrorMessage { get; set; }

			/// <summary>
			/// Complete exception details for logging/debugging by developers.
			/// This should not be shown to end users, only in logs.
			/// </summary>
			public string? LastErrorDetails { get; set; }
		}

		// ========================================================================
		// PRIVATE FIELDS
		// ========================================================================

		/// <summary>
		/// Holds the current state of the data source configuration.
		/// This is a private field with a public read-only property to enforce consistency.
		/// </summary>
		private DataSourceState _currentDataSourceState = new DataSourceState();

		// ========================================================================
		// PUBLIC PROPERTIES
		// ========================================================================

		/// <summary>
		/// Gets the current data source state. This is read-only to external callers.
		/// To change the state, use SwitchToJsonFileDataSourceAsync() or SwitchToApiDataSourceAsync().
		/// </summary>
		public DataSourceState CurrentDataSourceState
		{
			get { return _currentDataSourceState; }
		}

		/// <summary>
		/// Event that fires whenever the data source state changes.
		/// Components can subscribe to this event to be notified of state transitions.
		/// Example: MainLayout subscribes to this to change title bar color.
		/// </summary>
		public event Action? OnDataSourceChanged;

		// ========================================================================
		// PUBLIC METHODS
		// ========================================================================

		/// <summary>
		/// Initializes the data source to API mode (default state on application startup).
		/// This should be called from Program.cs during startup to set initial state.
		/// </summary>
		public void InitializeToApiDataSource()
		{
			_currentDataSourceState = new DataSourceState
			{
				CurrentDataSourceType = DataSourceType.ApiServer,
				IsReadOnlyMode = false,
				JsonFilePathForDataSource = null,
				JsonFileNameForDisplay = null,
				LastErrorMessage = null,
				LastErrorDetails = null
			};
			NotifyStateChanged();
		}

		/// <summary>
		/// Switches the data source to JSON File mode with the provided file path.
		/// This method validates that the provided file path is not empty and updates
		/// the internal state accordingly.
		/// 
		/// EXPLANATION: This method is used when user clicks "Open OpenRose File" button
		/// and provides a valid JSON file path. Before calling this method, the caller
		/// should have already validated that the JSON file exists and is readable.
		/// </summary>
		/// <param name="jsonFilePathForDataSourceProvided">
		/// Full path to the JSON file (UNC or local). 
		/// Example: "C:\\Exports\\openrose_export_20260214.json" or "\\\\server\\share\\file.json"
		/// </param>
		public void SwitchToJsonFileDataSource(string jsonFilePathForDataSourceProvided)
		{
			if (string.IsNullOrWhiteSpace(jsonFilePathForDataSourceProvided))
			{
				throw new ArgumentException("JSON file path cannot be null or empty.", nameof(jsonFilePathForDataSourceProvided));
			}

			// Extract file name from full path for display purposes
			string jsonFileNameForDisplayExtracted = System.IO.Path.GetFileName(jsonFilePathForDataSourceProvided);

			_currentDataSourceState = new DataSourceState
			{
				CurrentDataSourceType = DataSourceType.JsonFile,
				IsReadOnlyMode = true,
				JsonFilePathForDataSource = jsonFilePathForDataSourceProvided,
				JsonFileNameForDisplay = jsonFileNameForDisplayExtracted,
				LastErrorMessage = null,
				LastErrorDetails = null
			};

			NotifyStateChanged();
		}

		/// <summary>
		/// Switches the data source back to API Server mode.
		/// This clears all JSON file related state information.
		/// 
		/// EXPLANATION: This method is used when user wants to go back from JSON file
		/// read-only mode to API server connected mode. Since JSON files are read-only,
		/// there are no unsaved changes to worry about.
		/// </summary>
		public void SwitchToApiDataSource()
		{
			_currentDataSourceState = new DataSourceState
			{
				CurrentDataSourceType = DataSourceType.ApiServer,
				IsReadOnlyMode = false,
				JsonFilePathForDataSource = null,
				JsonFileNameForDisplay = null,
				LastErrorMessage = null,
				LastErrorDetails = null
			};

			NotifyStateChanged();
		}

		/// <summary>
		/// Records an error that occurred during JSON file validation or loading.
		/// This method stores both user-friendly error message and detailed error information
		/// for logging purposes.
		/// 
		/// EXPLANATION: When JSON file validation fails, we call this method to record
		/// the error, then we stay in API mode. The error message is shown to user
		/// and detailed error is logged for developer troubleshooting.
		/// </summary>
		/// <param name="userFriendlyErrorMessageToShow">
		/// User-facing error message that will be displayed in error dialog.
		/// Example: "Failed to load JSON file: Invalid data structure"
		/// </param>
		/// <param name="detailedErrorInformationForLogging">
		/// Complete exception details for logging and debugging.
		/// Example: Full exception stack trace and inner exception messages.
		/// </param>
		public void RecordJsonFileLoadingError(string userFriendlyErrorMessageToShow, string detailedErrorInformationForLogging)
		{
			_currentDataSourceState.LastErrorMessage = userFriendlyErrorMessageToShow;
			_currentDataSourceState.LastErrorDetails = detailedErrorInformationForLogging;
			NotifyStateChanged();
		}

		/// <summary>
		/// Clears any recorded error messages and details.
		/// Called when successfully switching to a new data source.
		/// </summary>
		public void ClearErrorInformation()
		{
			_currentDataSourceState.LastErrorMessage = null;
			_currentDataSourceState.LastErrorDetails = null;
		}

		// ========================================================================
		// PRIVATE METHODS
		// ========================================================================

		/// <summary>
		/// Internal method that triggers the OnDataSourceChanged event to notify
		/// all subscribed components that the data source state has changed.
		/// 
		/// EXPLANATION: Similar to ConfigurationService pattern, we call this
		/// whenever state changes to trigger component re-renders in Blazor.
		/// </summary>
		private void NotifyStateChanged()
		{
			OnDataSourceChanged?.Invoke();
		}
	}
}