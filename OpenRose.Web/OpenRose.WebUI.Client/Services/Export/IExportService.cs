// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenRose.WebUI.Client.Services.Export
{
	public interface IExportService
	{
		/// <summary>
		/// Downloads the exported hierarchy as a JSON file for a given record ID.
		/// </summary>
		/// <param name="exportRecordId">The GUID of the record to export.</param>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>
		/// A tuple containing the file bytes and the suggested filename.
		/// </returns>
		Task<(byte[]? fileContent, string? fileName)> DownloadExportHierarchyAsync(Guid exportRecordId, bool exportIncludedBaselineItemzOnly, CancellationToken cancellationToken = default);
	}
}