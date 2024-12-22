// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.SharedModels;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace OpenRose.WebUI.Client.Services.ItemzChangeHistory
{
	public class ItemzChangeHistoryService : IItemzChangeHistoryService
	{
		private readonly HttpClient _httpClient;

		public ItemzChangeHistoryService(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		#region __GET_ItemzChangeHistory_By_GUID_ItemzID__Async
		public async Task<ICollection<GetItemzChangeHistoryDTO>> __GET_ItemzChangeHistory_By_GUID_ItemzID__Async(Guid itemzId)
		{
			return await __GET_ItemzChangeHistory_By_GUID_ItemzID__Async(itemzId, CancellationToken.None);
		}

		public async Task<ICollection<GetItemzChangeHistoryDTO>> __GET_ItemzChangeHistory_By_GUID_ItemzID__Async(Guid itemzId, CancellationToken cancellationToken)
		{

			try
			{
				if (itemzId == Guid.Empty)
				{
					throw new ArgumentNullException(nameof(itemzId) + "is required for which value is not provided");
				}

				// TODO::Utilize urlBuilder which is commented below.
				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append($"/api/ItemzChangeHistory/{itemzId.ToString()}");
				//urlBuilder_.Append('?');

				//urlBuilder_.Length--;

				var response = await _httpClient.GetFromJsonAsync<ICollection<GetItemzChangeHistoryDTO>>($"/api/ItemzChangeHistory/{itemzId.ToString()}", cancellationToken);

				return response!;
			}
			catch (Exception)
			{

			}
			return default;
		}
		#endregion

		#region __DELETE_ItemzChangeHistory_By_GUID_ID__Async

		public async Task<int> __DELETE_ItemzChangeHistory_By_GUID_ID__Async(DeleteChangeHistoryDTO body)
		{
			return await __DELETE_ItemzChangeHistory_By_GUID_ID__Async(body, CancellationToken.None);
		}

		public async Task<int> __DELETE_ItemzChangeHistory_By_GUID_ID__Async(DeleteChangeHistoryDTO body, CancellationToken cancellationToken)
		{
			try
			{

				if (body == null)
				{
					throw new ArgumentNullException(nameof(body) + "is required for which value is not provided");
				}

				// TODO::Utilize urlBuilder which is commented below.

				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append($"/api/ItemzChangeHistory");
				//urlBuilder_.Append('?');

				//urlBuilder_.Length--;

				// EXPLANATION :: Because .NET does not provide option to send Delete request with json body included 
				// we are manually creating request to send JSON body. We found this information at ... 
				// https://stackoverflow.com/questions/28054515/how-to-send-delete-with-json-to-the-rest-api-using-httpclient

				// EXPLANATION :: HERE WE ARE SERIALIZING DESIRED CLASS / OBJECT FORMAT INTO JSON FORMAT FOR REQUEST CONTENT 
				var options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true,
				};
				var json_ = JsonSerializer.Serialize<DeleteChangeHistoryDTO>(body, options);

				var request = new HttpRequestMessage
				{
					Content = new StringContent($"{json_}", Encoding.UTF8, "application/json"),
					Method = HttpMethod.Delete,
					RequestUri = new Uri($"/api/ItemzChangeHistory")
				};
				var httpResponseMessage = await _httpClient.SendAsync(request);

				//var httpResponseMessage = await _httpClient.DeleteAsync($"/api/ItemzTrace/DeleteItemzTraceCollection", body, cancellationToken);

				httpResponseMessage.EnsureSuccessStatusCode();
				string responseContent = httpResponseMessage.Content.ReadAsStringAsync().Result;

				if (int.TryParse(responseContent, out int responseInt))
				{
					return responseInt;
				}
			}
			catch (Exception)
			{
			}
			return default;
		}
		#endregion
	}
}