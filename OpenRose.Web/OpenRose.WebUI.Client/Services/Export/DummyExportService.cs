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
	public class DummyExportService : IExportService
	{

		/// <inheritdoc/>
		public async Task<(byte[]? fileContent, string? fileName)> DownloadExportHierarchyAsync(Guid exportRecordId, CancellationToken cancellationToken = default)
		{
			throw new InvalidOperationException("OpenRose API base URL is not configured. Please provide a valid URL in the configuration file.");
		}
	}
}