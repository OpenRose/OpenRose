// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using Microsoft.Extensions.Options;
using OpenRose.WebUI.Configuration;
using System.Text.Json;

namespace OpenRose.WebUI.Services
{
	/// <summary>
	/// EXPLANATION:
	/// This service manages server-side offline JSON files stored in the folder
	/// resolved by OfflineContentPathResolver.
	///
	/// IMPORTANT:
	/// - The folder may not exist or may not be writable (e.g., Azure Web App, Linux permissions).
	/// - This repository NEVER throws for missing folders.
	/// - Instead, it gracefully returns empty results and allows the UI to show warnings.
	///
	/// This keeps the application cross-platform and resilient.
	/// </summary>
	public class OfflineCatalogRepository
	{
		private readonly OfflineContentSettings _offlineSettings;
		private readonly OfflineContentPathResolver _pathResolver;

		private readonly string? _storageFolderFullPath;

		public bool IsStorageAvailable => _pathResolver.IsStorageFolderAvailable;

		public OfflineCatalogRepository(
			IOptions<OfflineContentSettings> offlineSettingsOptions,
			OfflineContentPathResolver pathResolver)
		{
			_offlineSettings = offlineSettingsOptions.Value;
			_pathResolver = pathResolver;

			// EXPLANATION:
			// The resolver gives us the final absolute path, or null if unavailable.
			_storageFolderFullPath = _pathResolver.ResolvedStorageFolderPath;

		}

		/// <summary>
		/// EXPLANATION:
		/// Returns a list of all JSON files stored in the offline folder.
		/// If the folder is unavailable, returns an empty list instead of throwing.
		/// </summary>
		public IEnumerable<string> GetAvailableJsonFiles()
		{
			if (!IsStorageAvailable || _storageFolderFullPath is null)
				return Enumerable.Empty<string>();

			try
			{
				return Directory
					.EnumerateFiles(_storageFolderFullPath, "*.json", SearchOption.TopDirectoryOnly)
					.Select(Path.GetFileName)
					.OrderBy(name => name)
					.ToList();
			}
			catch
			{
				// EXPLANATION:
				// If the folder becomes unavailable at runtime (permissions, deletion),
				// we degrade gracefully.
				return Enumerable.Empty<string>();
			}
		}

		/// <summary>
		/// EXPLANATION:
		/// Saves a JSON file into the offline storage folder.
		/// If the folder is unavailable, this becomes a no-op.
		/// </summary>
		public async Task SaveJsonFileAsync(string fileName, string jsonContent)
		{
			if (!IsStorageAvailable || _storageFolderFullPath is null)
				return;

			string fullPath = Path.Combine(_storageFolderFullPath, fileName);

			await File.WriteAllTextAsync(fullPath, jsonContent);
		}

		/// <summary>
		/// EXPLANATION:
		/// Deletes a JSON file from the offline storage folder.
		/// If the folder is unavailable, nothing happens.
		/// </summary>
		public void DeleteJsonFile(string fileName)
		{
			if (!IsStorageAvailable || _storageFolderFullPath is null)
				return;

			string fullPath = Path.Combine(_storageFolderFullPath, fileName);

			if (File.Exists(fullPath))
			{
				File.Delete(fullPath);
			}
		}

		/// <summary>
		/// EXPLANATION:
		/// Returns the full absolute path to a JSON file stored on the server.
		/// If the folder is unavailable, returns null.
		/// </summary>
		public string? GetFullPathForJsonFile(string fileName)
		{
			if (!IsStorageAvailable || _storageFolderFullPath is null)
				return null;

			return Path.Combine(_storageFolderFullPath, fileName);
		}

	}
}
