// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.


using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace OpenRose.WebUI.Services
{
	public class RefreshRecoveryService
	{
		private readonly ProtectedSessionStorage _session;

		private const string KEY_JSON_FILE = "RR_JSON_FILE";
		private const string KEY_ROOT_ID = "RR_ROOT_ID";
		private const string KEY_SELECTED_ID = "RR_SELECTED_ID";
		private const string KEY_EXPANDED = "RR_EXPANDED_IDS";
		private const string KEY_SCROLL = "RR_SCROLL_POS";

		public RefreshRecoveryService(ProtectedSessionStorage session)
		{
			_session = session;
		}

		// -----------------------------
		// SAVE STATE
		// -----------------------------
		public async Task SaveJsonFileNameAsync(string fileName)
		{
			await _session.SetAsync(KEY_JSON_FILE, fileName);
		}

		public async Task SaveRootIdAsync(Guid rootId)
		{
			await _session.SetAsync(KEY_ROOT_ID, rootId);
		}

		public async Task SaveSelectedIdAsync(Guid selectedId)
		{
			await _session.SetAsync(KEY_SELECTED_ID, selectedId);
		}

		public async Task SaveExpandedNodesAsync(IEnumerable<Guid> expandedIds)
		{
			await _session.SetAsync(KEY_EXPANDED, expandedIds.ToList());
		}

		public async Task SaveScrollPositionAsync(double scrollY)
		{
			await _session.SetAsync(KEY_SCROLL, scrollY);
		}

		// -----------------------------
		// LOAD STATE
		// -----------------------------
		public async Task<string?> LoadJsonFileNameAsync()
		{
			var result = await _session.GetAsync<string>(KEY_JSON_FILE);
			return result.Success ? result.Value : null;
		}

		public async Task<Guid?> LoadRootIdAsync()
		{
			var result = await _session.GetAsync<Guid>(KEY_ROOT_ID);
			return result.Success ? result.Value : null;
		}

		public async Task<Guid?> LoadSelectedIdAsync()
		{
			var result = await _session.GetAsync<Guid>(KEY_SELECTED_ID);
			return result.Success ? result.Value : null;
		}

		public async Task<List<Guid>?> LoadExpandedNodesAsync()
		{
			var result = await _session.GetAsync<List<Guid>>(KEY_EXPANDED);
			return result.Success ? result.Value : null;
		}

		public async Task<double?> LoadScrollPositionAsync()
		{
			var result = await _session.GetAsync<double>(KEY_SCROLL);
			return result.Success ? result.Value : null;
		}

		// -----------------------------
		// CLEAR STATE
		// -----------------------------
		public async Task ClearAllAsync()
		{
			await _session.DeleteAsync(KEY_JSON_FILE);
			await _session.DeleteAsync(KEY_ROOT_ID);
			await _session.DeleteAsync(KEY_SELECTED_ID);
			await _session.DeleteAsync(KEY_EXPANDED);
			await _session.DeleteAsync(KEY_SCROLL);
		}
	}
}
