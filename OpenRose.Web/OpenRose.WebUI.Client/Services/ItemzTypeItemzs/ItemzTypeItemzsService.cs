// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.Services.ItemzTypeItemzsService;
using OpenRose.WebUI.Client.SharedModels;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace OpenRose.WebUI.Client.Services.ItemzTypeItemzsService
{
    public class ItemzTypeItemzsService : IItemzTypeItemzsService
	{
        private readonly HttpClient _httpClient;

        public ItemzTypeItemzsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


		#region __GET_Itemzs_By_ItemzType__Async

		public async Task<ICollection<GetItemzDTO>> __GET_Itemzs_By_ItemzType__Async(Guid itemzTypeId, int? pageNumber, int? pageSize, string orderBy)
        {
           return await __GET_Itemzs_By_ItemzType__Async(itemzTypeId,pageNumber,pageSize,orderBy, CancellationToken.None);

		}

		public async Task<ICollection<GetItemzDTO>> __GET_Itemzs_By_ItemzType__Async(Guid itemzTypeId, int? pageNumber, int? pageSize, string orderBy, CancellationToken cancellationToken)
		{
			try
            {
                // TODO :: Utilize urlBuilder which is commented below.

                //var urlBuilder_ = new System.Text.StringBuilder();

                //if (itemzTypeId == Guid.Empty)
                //{
                //    throw new ArgumentNullException(nameof(itemzTypeId) + "is required for which value is not provided");
                //}

                //urlBuilder_.Append($"/api/ItemzTypeItemzs/{itemzTypeId.ToString()}");
                //urlBuilder_.Append('?');

                //if (pageNumber != null)
                //{
                //    urlBuilder_.Append(System.Uri.EscapeDataString("pageNumber")).Append('=').Append(System.Uri.EscapeDataString(pageNumber.ToString()!)).Append('&');
                //}

                //if (pageSize != null)
                //{
                //    urlBuilder_.Append(System.Uri.EscapeDataString("pageSize")).Append('=').Append(System.Uri.EscapeDataString(pageSize.ToString()!)).Append('&');
                //}
                //if (orderBy != null)
                //{
                //    urlBuilder_.Append(System.Uri.EscapeDataString("orderBy")).Append('=').Append(System.Uri.EscapeDataString(orderBy)).Append('&');
                //}

                //urlBuilder_.Length--;

                var response = await _httpClient.GetFromJsonAsync<ICollection<GetItemzDTO>>($"/api/ItemzTypeItemzs/{itemzTypeId.ToString()}?pageNumber={pageNumber}&pageSize={pageSize}&orderBy={orderBy}", cancellationToken);

				return response!;
			}
			catch (Exception)
			{

			}
			return default;


		}

		#endregion


        #region __POST_Create_Itemz_Collection_By_ItemzType__Async

		public async Task<ICollection<GetItemzDTO>> __POST_Create_Itemz_Collection_By_ItemzType__Async(Guid itemzTypeId, IEnumerable<CreateItemzDTO> body)
        {
            return await __POST_Create_Itemz_Collection_By_ItemzType__Async(itemzTypeId, body, CancellationToken.None);

		}

		public async Task<ICollection<GetItemzDTO>> __POST_Create_Itemz_Collection_By_ItemzType__Async(Guid itemzTypeId, IEnumerable<CreateItemzDTO> body, CancellationToken cancellationToken)
		{

			try
			{
				// TODO :: Utilize urlBuilder which is commented below.

				//var urlBuilder_ = new System.Text.StringBuilder();

				//if (itemzTypeId == Guid.Empty)
				//{
				//	throw new ArgumentNullException(nameof(itemzTypeId) + "is required for which value is not provided");
				//}

				//urlBuilder_.Append($"/api/ItemzTypeItemzs/{itemzTypeId.ToString()}");
				//urlBuilder_.Append('?');

				//urlBuilder_.Length--;

				var httpResponseMessage = await _httpClient.PostAsJsonAsync($"/api/ItemzTypeItemzs/{itemzTypeId.ToString()}" 
					, body, cancellationToken);

				if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.Conflict)
				{
					// Read the response content
					var _errorContent = await httpResponseMessage.Content.ReadAsStringAsync();

					// TODO :: Use MudBlazor Snackbar to show the message (assuming MudBlazor Snackbar is set up)
					// TODO :: Do we need to pass server error message all the way to user UI? We need to check what's included in _errorContent though!
					throw new ApplicationException($"FAILED : {_errorContent}");
				}

				httpResponseMessage.EnsureSuccessStatusCode();

				string responseContent = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

				// EXPLANATION :: HERE WE ARE SERIALIZING JSON RESPONSE INTO DESIRED CLASS / OBJECT FORMAT FOR RETURNING
				var options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true,
				};
				var response = JsonSerializer.Deserialize<ICollection<GetItemzDTO>>(responseContent, options);

				return (response!);

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

		#region __GET_Itemz_Count_By_ItemzType__Async

		public async Task<int> __GET_Itemz_Count_By_ItemzType__Async(Guid itemzTypeId)
		{
			return await __GET_Itemz_Count_By_ItemzType__Async(itemzTypeId, CancellationToken.None);
		}

		public async Task<int> __GET_Itemz_Count_By_ItemzType__Async(Guid itemzTypeId, CancellationToken cancellationToken)
		{
			try
			{
				// TODO :: Utilize urlBuilder which is commented below.

				//var urlBuilder_ = new System.Text.StringBuilder();

				//if (itemzTypeId == Guid.Empty)
				//{
				//	throw new ArgumentNullException(nameof(itemzTypeId) + "is required for which value is not provided");
				//}

				//urlBuilder_.Append($"/api/ItemzTypeItemzs/GetItemzCount/{itemzTypeId.ToString()}");
				//urlBuilder_.Append('?');

				//urlBuilder_.Length--;

				var response = await _httpClient.GetFromJsonAsync<int>($"/api/ItemzTypeItemzs/GetItemzCount/{itemzTypeId.ToString()}", cancellationToken);

				return response!;
			}
			catch (Exception)
			{

			}
			return default;

		}

		#endregion

		#region __GET_Check_ItemzType_Itemz_Association_Exists__Async
		public async Task<GetItemzDTO> __GET_Check_ItemzType_Itemz_Association_Exists__Async(Guid? itemzTypeId, Guid? itemzId)
		{
			return await __GET_Check_ItemzType_Itemz_Association_Exists__Async (itemzTypeId, itemzId, CancellationToken.None);
		}

		public async Task<GetItemzDTO> __GET_Check_ItemzType_Itemz_Association_Exists__Async(Guid? itemzTypeId, Guid? itemzId, CancellationToken cancellationToken)
		{

			try
			{
				// TODO :: Utilize urlBuilder which is commented below.

				//var urlBuilder_ = new System.Text.StringBuilder();

				//urlBuilder_.Append($"/api/ItemzTypeItemzs/CheckExists");
				//urlBuilder_.Append('?');

				//if (itemzTypeId != Guid.Empty)
				//{
				//	urlBuilder_.Append(System.Uri.EscapeDataString("itemzTypeId")).Append('=').Append(System.Uri.EscapeDataString(itemzTypeId.ToString()!)).Append('&');
				//}
				//else
				//{
				//	throw new ArgumentNullException(nameof(itemzTypeId) + "is required for which value is not provided");
				//}

				//if (itemzId != Guid.Empty)
				//{
				//	urlBuilder_.Append(System.Uri.EscapeDataString("itemzId")).Append('=').Append(System.Uri.EscapeDataString(itemzId.ToString()!)).Append('&');
				//}
				//else
				//{
				//	throw new ArgumentNullException(nameof(itemzId) + "is required for which value is not provided");
				//}

				//urlBuilder_.Length--;

				var response = await _httpClient.GetFromJsonAsync<GetItemzDTO>($"/api/ItemzTypeItemzs/CheckExists?itemzTypeId={itemzTypeId.ToString()}&itemzId={itemzId.ToString()}", cancellationToken);

				return response!;
			}
			catch (Exception)
			{

			}
			return default;

		}
		#endregion

		#region __POST_Create_Single_Itemz_By_ItemzType__Async

		public async Task<GetItemzDTO> __POST_Create_Single_Itemz_By_ItemzType__Async(Guid itemzTypeId, bool? atBottomOfChildNodes, CreateItemzDTO body)
		{
			return await __POST_Create_Single_Itemz_By_ItemzType__Async(itemzTypeId, atBottomOfChildNodes, body, CancellationToken.None);
		}

		public async Task<GetItemzDTO> __POST_Create_Single_Itemz_By_ItemzType__Async(Guid itemzTypeId, bool? atBottomOfChildNodes, CreateItemzDTO body, CancellationToken cancellationToken)
		{
			try
			{
				// TODO :: Utilize urlBuilder which is commented below.

				//var urlBuilder_ = new System.Text.StringBuilder();

				//if (itemzTypeId == Guid.Empty)
				//{
				//	throw new ArgumentNullException(nameof(itemzTypeId) + "is required for which value is not provided");
				//}

				//urlBuilder_.Append($"/api/ItemzTypeItemzs/CreateSingleItemz/{itemzTypeId.ToString()}");
				//urlBuilder_.Append('?');

				//if ((bool)atBottomOfChildNodes!)
				//{
				//	urlBuilder_.Append(System.Uri.EscapeDataString("atBottomOfChildNodes")).Append('=').Append(System.Uri.EscapeDataString("true")).Append('&');
				//}
				//else
				//{
				//	urlBuilder_.Append(System.Uri.EscapeDataString("atBottomOfChildNodes")).Append('=').Append(System.Uri.EscapeDataString( "false")).Append('&');
				//}

				//urlBuilder_.Length--;

				var httpResponseMessage = await _httpClient.PostAsJsonAsync($"/api/ItemzTypeItemzs/CreateSingleItemz/{itemzTypeId.ToString()}&atBottomOfChildNodes={atBottomOfChildNodes}"
					, body, cancellationToken);
				httpResponseMessage.EnsureSuccessStatusCode();
				string responseContent = httpResponseMessage.Content.ReadAsStringAsync().Result;

				// EXPLANATION :: HERE WE ARE SERIALIZING JSON RESPONSE INTO DESIRED CLASS / OBJECT FORMAT FOR RETURNING
				var options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true,
				};
				var response = JsonSerializer.Deserialize<GetItemzDTO>(responseContent, options);

				return response!;

			}
			catch (Exception)
			{
			}
			return default;

		}

		#endregion

		#region __POST_Associate_Itemz_To_ItemzType__Async
		public async Task<GetItemzDTO> __POST_Associate_Itemz_To_ItemzType__Async(bool? atBottomOfChildNodes, ItemzTypeItemzDTO body)
		{
			return await __POST_Associate_Itemz_To_ItemzType__Async(atBottomOfChildNodes, body, CancellationToken.None);
		}


		public async Task<GetItemzDTO> __POST_Associate_Itemz_To_ItemzType__Async(bool? atBottomOfChildNodes, ItemzTypeItemzDTO body, CancellationToken cancellationToken)
		{
			try
			{
				// TODO :: Utilize urlBuilder which is commented below.

				var urlBuilder_ = new System.Text.StringBuilder();

				urlBuilder_.Append($"/api/ItemzTypeItemzs");
				urlBuilder_.Append('?');

				if ((bool)atBottomOfChildNodes!)
				{
					urlBuilder_.Append(System.Uri.EscapeDataString("atBottomOfChildNodes")).Append('=').Append(System.Uri.EscapeDataString("true")).Append('&');
				}
				else
				{
					urlBuilder_.Append(System.Uri.EscapeDataString("atBottomOfChildNodes")).Append('=').Append(System.Uri.EscapeDataString("false")).Append('&');
				}

				urlBuilder_.Length--;

				var httpResponseMessage = await _httpClient.PostAsJsonAsync($"/api/ItemzTypeItemzs&atBottomOfChildNodes={atBottomOfChildNodes}"
					, body, cancellationToken);
				httpResponseMessage.EnsureSuccessStatusCode();
				string responseContent = httpResponseMessage.Content.ReadAsStringAsync().Result;

				// EXPLANATION :: HERE WE ARE SERIALIZING JSON RESPONSE INTO DESIRED CLASS / OBJECT FORMAT FOR RETURNING
				var options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true,
				};
				var response = JsonSerializer.Deserialize<GetItemzDTO>(responseContent, options);

				return response!;

			}
			catch (Exception)
			{
			}
			return default;
		}

		#endregion

		#region __DELETE_ItemzType_and_Itemz_Association__Async
		public async Task __DELETE_ItemzType_and_Itemz_Association__Async(ItemzTypeItemzDTO body)
		{
			await __DELETE_ItemzType_and_Itemz_Association__Async(body, CancellationToken.None);
		}


		public async Task __DELETE_ItemzType_and_Itemz_Association__Async(ItemzTypeItemzDTO body, CancellationToken cancellationToken)
		{
			try
			{
				// TODO::Utilize urlBuilder which is commented below.

				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append($"/api/ItemzTypeItemzs");
				//urlBuilder_.Append('?');

				//urlBuilder_.Length--;


				// EXPLANATION :: Because .NET does not provide option to send Delete request with json body included 
				// we are manually creating request to send JSON body. We found this information at ... 
				// https://stackoverflow.com/questions/28054515/how-to-send-delete-with-json-to-the-rest-api-using-httpclient

				var request = new HttpRequestMessage
				{
					Content = new StringContent($"{body}", Encoding.UTF8, "application/json"),
					Method = HttpMethod.Delete,
					RequestUri = new Uri("/api/ItemzTypeItemzs")
				};
				var httpResponseMessage = await _httpClient.SendAsync(request);

//				var httpResponseMessage = await _httpClient.DeleteFromJsonAsync<ItemzTypeItemzDTO>($"/api/ItemzTypeItemzs", body, cancellationToken);

				httpResponseMessage.EnsureSuccessStatusCode();
				string responseContent = httpResponseMessage.Content.ReadAsStringAsync().Result;

			}
			catch (Exception)
			{
			}
		}

		#endregion

	}
}
