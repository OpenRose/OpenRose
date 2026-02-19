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

		public bool IsDirty(Guid recordId) => _dirtyRecords.ContainsKey(recordId);

		public bool AnyDirtyRecords() => _dirtyRecords.Count > 0;

		public void SetDirty(Guid recordId, bool isDirty)
		{
			if (isDirty)
			{
				_dirtyRecords[recordId] = true;
			}
			else
			{
				_dirtyRecords.Remove(recordId);
			}
		}
		public IEnumerable<Guid> GetDirtyRecords() => _dirtyRecords.Keys;
		
		public void ClearAll() => _dirtyRecords.Clear();

		// 🔑 New helper: standard unsaved changes dialog
		public async Task<bool> ConfirmDiscardChangesAsync(Guid recordId)
		{
			if (!IsDirty(recordId))
				return true; // nothing dirty, safe to proceed

			var parameters = new DialogParameters();
			var options = new DialogOptions { CloseOnEscapeKey = true, BackdropClick = true };

			var dialog = await _dialogService.ShowAsync<UnsavedChangesDialog>("Unsaved Changes", parameters, options);
			var result = await dialog.Result;

			// Leave Page = Ok(true), Stay on Page = Cancel
			return result.Canceled == false && result.Data is bool b && b;
		}

		public async Task<bool> ConfirmDiscardAllChangesAsync()
		{
			if (!AnyDirtyRecords())
				return true;

			var parameters = new DialogParameters();
			var options = new DialogOptions { CloseOnEscapeKey = true, BackdropClick = true };

			var dialog = await _dialogService.ShowAsync<UnsavedChangesDialog>(
				"Unsaved Changes",
				parameters,
				options);

			var result = await dialog.Result;

			// Ok(true) = discard, Cancel = stay
			if (result.Canceled || result.Data is not bool b || b == false)
				return false;

			ClearAll();
			return true;
		}
	}
}
