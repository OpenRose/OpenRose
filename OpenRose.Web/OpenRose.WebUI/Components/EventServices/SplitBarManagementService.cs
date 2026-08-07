// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

namespace OpenRose.WebUI.Components.EventServices
{
	public class SplitBarManagementService
	{
		/// <summary>
		/// Tracks whether the split bar is currently collapsed.
		/// True means the split bar is at position ZERO.
		/// False means the split bar is at its default expanded position.
		/// This state is scoped per user session because the service is registered as Scoped.
		/// </summary>
		public bool IsCollapsed { get; private set; } = false;

		/// <summary>
		/// Fired when any component requests the split bar to toggle
		/// between default and collapsed positions.
		/// </summary>
		public event Action OnToggleSplitBarRequested;

		/// <summary>
		/// Components call this to toggle the split bar.
		/// This method also updates the internal IsCollapsed state
		/// so that UI components can query the current split bar position.
		/// </summary>
		public void ToggleSplitBar()
		{
			// Toggle the state before notifying listeners.
			IsCollapsed = !IsCollapsed;

			OnToggleSplitBarRequested?.Invoke();
		}

		/// <summary>
		/// Helper method for UI components to check if the split bar is collapsed.
		/// </summary>
		public bool GetIsCollapsed()
		{
			return IsCollapsed;
		}

		/// <summary>
		/// Enables components to request the split bar to collapse to position ZERO.
		/// </summary>
		public void CollapseSplitBar()
		{
			// Only collapse if not already collapsed.
			if (!IsCollapsed)
			{
				// Toggle the state before notifying listeners.
				IsCollapsed = true;

				OnToggleSplitBarRequested?.Invoke();
			}
		}
	}
}
