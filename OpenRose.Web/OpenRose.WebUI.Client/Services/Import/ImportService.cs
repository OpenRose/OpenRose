// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.


using Microsoft.AspNetCore.Components.Forms;
using OpenRose.WebUI.Client.Services.Export;
using OpenRose.WebUI.Client.SharedModels;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace OpenRose.WebUI.Client.Services.Import
{
	public class ImportService : IImportService
	{
		private readonly HttpClient _httpClient;

		public ImportService(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		public async Task<HttpResponseMessage> ImportHierarchyAsync(
			IBrowserFile file,
			ImportFormClientDTO formDto,
			CancellationToken cancellationToken = default)
		{
			var content = new MultipartFormDataContent();

			// Add file content
			var fileStream = file.OpenReadStream(maxAllowedSize: 10_000_000); // Adjust limit as needed
			var streamContent = new StreamContent(fileStream);
			streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
			content.Add(streamContent, "File", file.Name);

			// Add form fields
			if (formDto.TargetParentId.HasValue)
				content.Add(new StringContent(formDto.TargetParentId.Value.ToString()), nameof(formDto.TargetParentId));

			content.Add(new StringContent(formDto.AtBottomOfChildNodes.ToString()), nameof(formDto.AtBottomOfChildNodes));
			content.Add(new StringContent(formDto.ImportExcludedBaselineItemz.ToString()), nameof(formDto.ImportExcludedBaselineItemz));

			if (formDto.FirstItemzTypeId.HasValue)
				content.Add(new StringContent(formDto.FirstItemzTypeId.Value.ToString()), nameof(formDto.FirstItemzTypeId));

			if (formDto.SecondItemzTypeId.HasValue)
				content.Add(new StringContent(formDto.SecondItemzTypeId.Value.ToString()), nameof(formDto.SecondItemzTypeId));

			if (formDto.FirstItemzId.HasValue)
				content.Add(new StringContent(formDto.FirstItemzId.Value.ToString()), nameof(formDto.FirstItemzId));

			if (formDto.SecondItemzId.HasValue)
				content.Add(new StringContent(formDto.SecondItemzId.Value.ToString()), nameof(formDto.SecondItemzId));

			var response = await _httpClient.PostAsync("/api/Import/ImportHierarchy", content, cancellationToken);

			return response;
		}
	}
}
