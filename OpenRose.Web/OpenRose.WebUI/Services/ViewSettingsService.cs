// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

namespace OpenRose.WebUI.Services
{
	/// <summary>
	/// UPDATED SERVICE FOR JSON FILE DATA SOURCE FEATURE
	/// 
	/// ViewSettingsService manages UI view-related settings such as whether to show
	/// read-only view or editable view in TreeView components.
	/// 
	/// EXISTING CODE:
	/// Previously this service only managed the ReadOnlyView toggle which controls
	/// whether TreeView displays in Read-Only mode (view-only) or Editable mode (CRUD).
	/// 
	/// NEW ADDITIONS:
	/// Added property to track if we are in JSON file data source mode, which determines
	/// what UI elements are visible and which options are enabled/disabled.
	/// </summary>
	public class ViewSettingsService
	{
		// ========================================================================
		// EXISTING CODE - PRESERVE AS-IS
		// ========================================================================

		/// <summary>
		/// EXISTING PROPERTY: Determines if TreeView should display in Read-Only mode.
		/// 
		/// When ReadOnlyView is true:
		/// - Users can view data and traceability but cannot modify records
		/// - Create, Update, Delete buttons are hidden
		/// - Only view/read operations are available
		/// 
		/// When ReadOnlyView is false:
		/// - Users can view data in editable mode with full CRUD operations
		/// - Create, Update, Delete buttons are visible
		/// </summary>
		public bool ReadOnlyView { get; set; } = false;

		// ========================================================================
		// NEW ADDITIONS FOR JSON FILE DATA SOURCE FEATURE
		// ========================================================================

		/// <summary>
		/// NEW PROPERTY: Indicates whether the application is currently operating
		/// in JSON File data source mode (read-only local file) versus API mode.
		/// 
		/// EXPLANATION: This property is set by DataSourceStateService whenever
		/// data source changes. UI components check this property to decide:
		/// - Which navigation menu items to show or hide
		/// - Which CRUD operation buttons to enable or disable
		/// - Which routing paths are allowed
		/// - Whether to show certain UI elements
		/// 
		/// When IsOperatingInJsonFileDataSourceMode is true:
		/// - Navigation menu shows ONLY Settings option
		/// - All CRUD operation buttons (Create, Update, Delete) are hidden
		/// - Copy, Import, Export, Move operations are disabled
		/// - Users can only view data in read-only TreeView
		/// - Breadcrumbs and navigation are simplified
		/// 
		/// When IsOperatingInJsonFileDataSourceMode is false (API mode):
		/// - All navigation options are visible as normal
		/// - CRUD buttons are enabled (subject to user permissions)
		/// - Import/Export/Copy/Move operations are available
		/// - Full application functionality is available
		/// </summary>
		public bool IsOperatingInJsonFileDataSourceMode { get; set; } = false;

		/// <summary>
		/// NEW PROPERTY: Controls whether to display the "Switch Data Source" button/icon
		/// in the application UI (specifically in MainLayout).
		/// 
		/// EXPLANATION: This allows the application to conditionally show or hide
		/// the data source switch control based on configuration or application state.
		/// For now, this defaults to true (always show the switch button).
		/// </summary>
		public bool ShowDataSourceSwitchControl { get; set; } = true;
	}
}