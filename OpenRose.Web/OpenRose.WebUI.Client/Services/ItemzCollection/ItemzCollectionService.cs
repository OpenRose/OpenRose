// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.Services.ItemzCollection;
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

namespace OpenRose.WebUI.Client.Services.ItemzCollection
{
	public class ItemzCollectionService : IItemzCollectionService
	{
		private readonly HttpClient _httpClient;

		public ItemzCollectionService(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}


		//#region __GET_Itemz_Collection_By_GUID_IDS__Async
		//public async Task<ICollection<GetItemzDTO>> __GET_Itemz_Collection_By_GUID_IDS__Async(IEnumerable<Guid> ids)
		//{
		//	return await __GET_Itemz_Collection_By_GUID_IDS__Async(ids, CancellationToken.None);
		//}
		//public async Task<ICollection<GetItemzDTO>> __GET_Itemz_Collection_By_GUID_IDS__Async(IEnumerable<Guid> ids, CancellationToken cancellationToken)
		//{
		//	try
		//	{
		//		// TODO :: Utilize urlBuilder which is commented below.

		//		if (!ids?.Any() ?? true)
		//		{
		//			throw new ArgumentNullException(nameof(ids) + "is required for which value is not provided");
		//		}
		//		var urlBuilder_ = new System.Text.StringBuilder();
		//		urlBuilder_.Append("/api/itemzcollection/(");
		//		//urlBuilder_.Append('(');
		//		for (var i = 0; i < ids!.Count(); i++)
		//		{
		//			if (i > 0) urlBuilder_.Append(',');
		//			urlBuilder_.Append((ids!.ElementAt(i).ToString()));
		//		}
		//		urlBuilder_.Append(')');


		//		var response = await _httpClient.GetFromJsonAsync<IEnumerable<GetItemzDTO>>($"{urlBuilder_}", cancellationToken);

		//		return response!.ToList();
		//	}
		//	catch (Exception)
		//	{
		//	}
		//	return default;

		//}

		//#endregion

		#region __POST_GET_Itemz_Collection_By_GUID_IDS__Async
		public async Task<ICollection<GetItemzDTO>> __POST_GET_Itemz_Collection_By_GUID_IDS__Async(IEnumerable<Guid> ids)
		{
			return await __POST_GET_Itemz_Collection_By_GUID_IDS__Async(ids, CancellationToken.None);
		}

		public async Task<ICollection<GetItemzDTO>> __POST_GET_Itemz_Collection_By_GUID_IDS__Async(IEnumerable<Guid> ids, CancellationToken cancellationToken)
		{
			try
			{
				if (!ids?.Any() ?? true)
				{
					throw new ArgumentNullException(nameof(ids) + " is required for which value is not provided");
				}

				var httpResponseMessage = await _httpClient.PostAsJsonAsync($"/api/itemzcollection/by-ids", ids, cancellationToken);

				if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.Conflict)
				{
					// Read the response content
					var _errorContent = await httpResponseMessage.Content.ReadAsStringAsync();
					throw new ApplicationException($"FAILED : {_errorContent}");
				}

				httpResponseMessage.EnsureSuccessStatusCode();

				string responseContent = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

				if (string.IsNullOrWhiteSpace(responseContent))
				{
					return default;
				}

				// EXPLANATION :: HERE WE ARE SERIALIZING JSON RESPONSE INTO DESIRED CLASS / OBJECT FORMAT FOR RETURNING
				var options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true,
				};
				var response = JsonSerializer.Deserialize<ICollection<GetItemzDTO>>(responseContent, options);
				return (response ?? default);
			}
			catch (HttpRequestException httpEx)
			{
				// Handle HTTP-specific exceptions (e.g., 404, 500) 
				// You could log this exception or display an appropriate message to the user
				throw new Exception($"HTTP error occurred: {httpEx.Message}");
			}
			catch (ArgumentNullException argEx)
			{
				throw new Exception($"Argument Null Exception: {argEx.Message}");
			}
			catch (Exception)
			{
				throw;
			}
		}
		#endregion

		#region __POST_Create_Itemz_Collection__Async

		public async Task<ICollection<GetItemzDTO>> __POST_Create_Itemz_Collection__Async(IEnumerable<CreateItemzDTO> body)
		{
			return await __POST_Create_Itemz_Collection__Async(body, CancellationToken.None);
		}
		public async Task<ICollection<GetItemzDTO>> __POST_Create_Itemz_Collection__Async(IEnumerable<CreateItemzDTO> body, CancellationToken cancellationToken)
		{
			try
			{

				if (!body?.Any() ?? true)
				{
					throw new ArgumentNullException(nameof(body) + "is required for which value is not provided");
				}
				// TODO :: Utilize urlBuilder which is commented below.

				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append("/api/itemzcollection");
				//urlBuilder_.Append('?');

				//urlBuilder_.Length--;

				var httpResponseMessage = await _httpClient.PostAsJsonAsync($"/api/itemzcollection", body, cancellationToken);

				if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.Conflict)
				{
					// Read the response content
					var _errorContent = await httpResponseMessage.Content.ReadAsStringAsync();

					// TODO :: Use MudBlazor Snackbar to show the message (assuming MudBlazor Snackbar is set up)
					// TODO :: Do we need to pass server error message all the way to user UI? We need to check what's included in _errorContent though!
					throw new ApplicationException($"FAILED : {_errorContent}");
				}

				httpResponseMessage.EnsureSuccessStatusCode();

				//string responseContent = httpResponseMessage.Content.ReadAsStringAsync().Result;
				string responseContent = httpResponseMessage.Content.ReadAsStringAsync(cancellationToken).Result;


				if (string.IsNullOrWhiteSpace(responseContent))
				{
					return default;
				}

				// EXPLANATION :: HERE WE ARE SERIALIZING JSON RESPONSE INTO DESIRED CLASS / OBJECT FORMAT FOR RETURNING
				var options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true,
				};
				var response = JsonSerializer.Deserialize<ICollection<GetItemzDTO>>(responseContent, options);
				return (response ?? default);
			}
			catch (HttpRequestException httpEx)
			{
				// Handle HTTP-specific exceptions (e.g., 404, 500) 
				// You could log this exception or display an appropriate message to the user
				throw new Exception($"HTTP error occurred: {httpEx.Message}");
			}
			catch (ArgumentNullException argEx)
			{
				throw new Exception($"Argument Null Exception: {argEx.Message}");
			}
			catch (Exception ex)
			{
				throw;
			}
		}
		#endregion
	}
}