// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

namespace OpenRose.WebUI.Components.EventServices
{
	public class SplitBarManagementService
	{
		/// <summary>
		/// Fired when any component requests the split bar to toggle
		/// between default and collapsed positions.
		/// </summary>
		public event Action OnToggleSplitBarRequested;

		/// <summary>
		/// Components call this to toggle the split bar.
		/// </summary>
		public void ToggleSplitBar()
		{
			OnToggleSplitBarRequested?.Invoke();
		}
	}
}

