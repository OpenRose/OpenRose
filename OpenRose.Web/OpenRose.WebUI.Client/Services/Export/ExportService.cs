// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace OpenRose.WebUI.Client.Services.Export
{
	public class ExportService : IExportService
	{
		private readonly HttpClient _httpClient;

		public ExportService(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		/// <inheritdoc/>
		public async Task<(byte[]? fileContent, string? fileName)> DownloadExportHierarchyAsync(Guid exportRecordId, bool exportIncludedBaselineItemzOnly, CancellationToken cancellationToken = default)
		{
			if (exportRecordId == Guid.Empty)
				throw new ArgumentException("ExportRecordId must be a valid GUID.", nameof(exportRecordId));

			var requestUri = $"/api/export/ExportHierarchy?exportRecordId={exportRecordId}";
			if (exportIncludedBaselineItemzOnly)
				requestUri += "&exportIncludedBaselineItemzOnly=true";

			// Set Accept header for JSON file
			var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				// TODO : Think about how we want to either log or handle different status codes here
				return (null, null);
			}

			var fileContent = await response.Content.ReadAsByteArrayAsync(cancellationToken);

			// Try to get filename from Content-Disposition header
			string? fileName = null;
			if (response.Content.Headers.ContentDisposition != null)
			{
				fileName = response.Content.Headers.ContentDisposition.FileNameStar
						?? response.Content.Headers.ContentDisposition.FileName;
				fileName = fileName?.Trim('\"');
			}
			else if (response.Content.Headers.TryGetValues("Content-Disposition", out var values))
			{
				var contentDisposition = values.FirstOrDefault();
				if (!string.IsNullOrEmpty(contentDisposition))
				{
					const string fileNameKey = "filename=";
					int fileNameIndex = contentDisposition.IndexOf(fileNameKey, StringComparison.OrdinalIgnoreCase);
					if (fileNameIndex >= 0)
					{
						fileName = contentDisposition.Substring(fileNameIndex + fileNameKey.Length).Trim('\"', ';');
					}
				}
			}

			return (fileContent, fileName);
		}
	}
}