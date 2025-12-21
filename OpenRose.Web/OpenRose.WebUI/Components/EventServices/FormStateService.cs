// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using MudBlazor;

namespace OpenRose.WebUI.Components.EventServices
{
	public class FormStateService
	{
		private readonly IDialogService _dialogService;

		public FormStateService(IDialogService dialogService)
		{
			_dialogService = dialogService;
		}

		// Existing dirty state tracking...
		private readonly Dictionary<Guid, bool> _dirtyRecords = new();

		public bool IsDirty(Guid recordId) =>
			_dirtyRecords.TryGetValue(recordId, out var isDirty) && isDirty;

		public void SetDirty(Guid recordId, bool isDirty) =>
			_dirtyRecords[recordId] = isDirty;

		// 🔑 New helper: standard unsaved changes dialog
		public async Task<bool> ConfirmDiscardChangesAsync(Guid recordId)
		{
			if (!IsDirty(recordId))
				return true; // nothing dirty, safe to proceed

			var result = await _dialogService.ShowMessageBox(
				"Unsaved Changes",
				"You have unsaved changes. Do you want to discard them?",
				yesText: "Discard",
				cancelText: "Cancel"
			);

			return result == true;
		}
	}


}
