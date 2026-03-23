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
	/// defined by OfflineContent.StorageFolder (e.g., C:\OpenRoseData\OfflineFiles).
	/// It provides methods for:
	///   - Ensuring the folder exists
	///   - Listing available JSON files
	///   - Saving uploaded JSON files to the server
	///   - Tracking the "active" offline JSON file
	///   - Returning full file paths for loading JSON data
	/// This service is used by:
	///   - Startup resolver (Phase 3)
	///   - Server JSON selection dialog (Phase 4)
	///   - Offline JSON loader (Phase 5)
	/// </summary>
	public class OfflineCatalogRepository
	{
		private readonly OfflineContentSettings _offlineSettings;
		private readonly string _storageFolderFullPath;
		private readonly string _activeFileMetadataPath;

		public OfflineCatalogRepository(IOptions<OfflineContentSettings> offlineSettingsOptions)
		{
			_offlineSettings = offlineSettingsOptions.Value;

			// EXPLANATION:
			// Resolve the absolute folder path where offline JSON files are stored.
			// Example: C:\OpenRoseData\OfflineFiles
			_storageFolderFullPath = _offlineSettings.StorageFolder;

			// EXPLANATION:
			// Metadata file that stores the currently active offline JSON file name.
			// Example: C:\OpenRoseData\OfflineFiles\ActiveOfflineFile.json
			_activeFileMetadataPath = Path.Combine(_storageFolderFullPath, "ActiveOfflineFile.json");

			EnsureStorageFolderExists();
		}

		/// <summary>
		/// EXPLANATION:
		/// Ensures that the offline storage folder exists.
		/// If it does not exist, it is created automatically.
		/// </summary>
		private void EnsureStorageFolderExists()
		{
			// If no folder is configured, do nothing.
			if (string.IsNullOrWhiteSpace(_storageFolderFullPath))
				return;

			if (!Directory.Exists(_storageFolderFullPath))
			{
				Directory.CreateDirectory(_storageFolderFullPath);
			}
		}


		/// <summary>
		/// EXPLANATION:
		/// Returns a list of all JSON files stored in the offline folder.
		/// Only files with .json extension are returned.
		/// </summary>
		public IEnumerable<string> GetAvailableJsonFiles()
		{
			EnsureStorageFolderExists();

			return Directory
				.EnumerateFiles(_storageFolderFullPath, "*.json", SearchOption.TopDirectoryOnly)
				.Select(Path.GetFileName)
				.OrderBy(name => name)
				.ToList();
		}

		/// <summary>
		/// EXPLANATION:
		/// Saves a JSON file (provided as a string) into the offline storage folder.
		/// The file is saved using the provided file name.
		/// </summary>
		public async Task SaveJsonFileAsync(string fileName, string jsonContent)
		{
			EnsureStorageFolderExists();

			string fullPath = Path.Combine(_storageFolderFullPath, fileName);

			await File.WriteAllTextAsync(fullPath, jsonContent);
		}

		/// <summary>
		/// EXPLANATION:
		/// Deletes a JSON file from the offline storage folder.
		/// </summary>
		public void DeleteJsonFile(string fileName)
		{
			string fullPath = Path.Combine(_storageFolderFullPath, fileName);

			if (File.Exists(fullPath))
			{
				File.Delete(fullPath);
			}
		}

		/// <summary>
		/// EXPLANATION:
		/// Returns the full absolute path to a JSON file stored on the server.
		/// </summary>
		public string GetFullPathForJsonFile(string fileName)
		{
			return Path.Combine(_storageFolderFullPath, fileName);
		}

		/// <summary>
		/// EXPLANATION:
		/// Saves the name of the currently active offline JSON file.
		/// This allows the system to remember which file to load at startup.
		/// </summary>
		public async Task SetActiveOfflineFileAsync(string fileName)
		{
			var metadata = new ActiveOfflineFileMetadata
			{
				ActiveFile = fileName
			};

			string json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions
			{
				WriteIndented = true
			});

			await File.WriteAllTextAsync(_activeFileMetadataPath, json);
		}

		/// <summary>
		/// EXPLANATION:
		/// Loads the name of the currently active offline JSON file.
		/// If no metadata file exists, returns the default JSON file from configuration.
		/// </summary>
		public string? GetActiveOfflineFile()
		{
			if (File.Exists(_activeFileMetadataPath))
			{
				try
				{
					string json = File.ReadAllText(_activeFileMetadataPath);
					var metadata = JsonSerializer.Deserialize<ActiveOfflineFileMetadata>(json);

					return metadata?.ActiveFile;
				}
				catch
				{
					// EXPLANATION:
					// If metadata is corrupted, fall back to default file.
					return _offlineSettings.DefaultJsonFile;
				}
			}

			return _offlineSettings.DefaultJsonFile;
		}

		private class ActiveOfflineFileMetadata
		{
			public string? ActiveFile { get; set; }
		}
	}
}
