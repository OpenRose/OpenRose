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

		/// <summary>
		/// Downloads Mermaid flowchart text for the given record ID.
		/// </summary>
		/// <param name="exportRecordId">The record GUID to export.</param>
		/// <param name="exportIncludedBaselineItemzOnly">
		/// When true, only BaselineItemz marked as included are exported.
		/// </param>
		/// <param name="cancellationToken">Optional cancellation token.</param>
		/// <returns>
		/// A tuple containing:
		/// <list type="bullet">
		///   <item><description><c>string? mermaidText</c> – The Mermaid diagram text if successful, otherwise null.</description></item>
		///   <item><description><c>int statusCode</c> – The HTTP status code returned by the server.</description></item>
		/// </list>
		/// </returns>
		Task<string> __GET_MermaidFlowChart_By_GUID_ID__Async(Guid exportRecordId,
																bool exportIncludedBaselineItemzOnly,
																bool showTraceabilityOnly,
																string baseUrl,
																CancellationToken cancellationToken = default);
	}


}