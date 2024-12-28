// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.SharedModels;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace OpenRose.WebUI.Client.Services.Itemz
{
	public class ItemzService : IItemzService
	{
		private readonly HttpClient _httpClient;

		public ItemzService(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}


		#region __Single_Itemz_By_GUID_ID__Async
		public async Task<GetItemzDTO?> __Single_Itemz_By_GUID_ID__Async(System.Guid itemzId)
		{
			return await __Single_Itemz_By_GUID_ID__Async(itemzId, CancellationToken.None);
		}


		public async Task<GetItemzDTO?> __Single_Itemz_By_GUID_ID__Async(Guid itemzId, CancellationToken cancellationToken)
		{
			try
			{
				var response = await _httpClient.GetFromJsonAsync<GetItemzDTO>($"/api/Itemzs/{itemzId}", cancellationToken);

				return response!;
			}
			catch (Exception)
			{
			}
			return default;
		}

		#endregion

		#region __POST_Create_Itemz__Async
		public async Task<GetItemzDTO?> __POST_Create_Itemz__Async(Guid? parentId, bool? atBottomOfChildNodes, CreateItemzDTO createItemzDTO)
		{
			return await __POST_Create_Itemz__Async(parentId, atBottomOfChildNodes, createItemzDTO, CancellationToken.None);
		}

		public async Task<GetItemzDTO?> __POST_Create_Itemz__Async(Guid? parentId, bool? atBottomOfChildNodes, CreateItemzDTO createItemzDTO, CancellationToken cancellationToken)
		{
			try
			{
				// TODO :: Because parentId and atBottomOfChildNodes are optional parameters we should introduce condition to check for null values in them
				// and accordingly forward request over to API endpoint.

				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append("/api/Itemzs");
				//urlBuilder_.Append('?');
				//if (parentId != null)
				//{
				//	urlBuilder_.Append(System.Uri.EscapeDataString("parentId")).Append('=').Append(System.Uri.EscapeDataString(parentId.ToString())).Append('&');
				//}

				//if ((bool)atBottomOfChildNodes!)
				//{
				//	urlBuilder_.Append(System.Uri.EscapeDataString("atBottomOfChildNodes")).Append('=').Append(System.Uri.EscapeDataString("true")).Append('&');
				//}
				//else
				//{
				//	urlBuilder_.Append(System.Uri.EscapeDataString("atBottomOfChildNodes")).Append('=').Append(System.Uri.EscapeDataString("false")).Append('&');
				//}

				//urlBuilder_.Length--;

				if (createItemzDTO == null || string.IsNullOrWhiteSpace(createItemzDTO.Name))
				{
					throw new ArgumentNullException("Itemz Name is a required field for which value was not provided");
				}

				var httpResponseMessage = await _httpClient.PostAsJsonAsync($"/api/Itemzs?parentId={parentId}&AtBottomOfChildNodes={atBottomOfChildNodes}", createItemzDTO, cancellationToken);

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
				var response = JsonSerializer.Deserialize<GetItemzDTO>(responseContent, options);
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

		#region __GET_Orphan_Itemzs_Collection__Async

		public async Task<(ICollection<GetItemzWithBasePropertiesDTO>, string )> __GET_Orphan_Itemzs_Collection__Async(int? pageNumber, int? pageSize, string orderBy)
		{
			return await __GET_Orphan_Itemzs_Collection__Async(pageNumber, pageSize, orderBy, CancellationToken.None);
		}
		public async Task<(ICollection<GetItemzWithBasePropertiesDTO>, string)> __GET_Orphan_Itemzs_Collection__Async(int? pageNumber, int? pageSize, string orderBy, CancellationToken cancellationToken)
		{
			try
			{
				var urlBuilder_ = new System.Text.StringBuilder();
				urlBuilder_.Append("/api/Itemzs/GetOrphan");
				urlBuilder_.Append('?');
				if (pageNumber != null)
				{
					urlBuilder_.Append(System.Uri.EscapeDataString("pageNumber")).Append('=').Append(System.Uri.EscapeDataString(pageNumber.ToString()!)).Append('&');
				}
				if (pageSize != null)
				{
					urlBuilder_.Append(System.Uri.EscapeDataString("pageSize")).Append('=').Append(System.Uri.EscapeDataString(pageSize.ToString()!)).Append('&');
				}

				if (orderBy != null)
				{
					urlBuilder_.Append(System.Uri.EscapeDataString("orderBy")).Append('=').Append(System.Uri.EscapeDataString(orderBy)).Append('&');
				}

    //            if (urlBuilder_[urlBuilder_.Length - 1] == '&') 
				//{ 
				//	urlBuilder_.Length--; 
				//}

                urlBuilder_.Length--;

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get, // TODO: VERIFY THAT THIS GET METHOD WORKS AS I CHANGED DELETE TO GET HERE.
                    RequestUri = new Uri(urlBuilder_.ToString(), UriKind.Relative)
                };
                var response = await _httpClient.SendAsync(request);

                var listOfOrphandItemz = await response.Content.ReadFromJsonAsync<ICollection<GetItemzWithBasePropertiesDTO>>(cancellationToken);
                var paginationHeader = response.Headers.GetValues("x-pagination").FirstOrDefault();

				return (listOfOrphandItemz!, paginationHeader!); // return paginationHeader which is JSON

                // var response = await _httpClient.GetFromJsonAsync<ICollection<GetItemzDTO>>("/api/Itemzs/GetOrphan", cancellationToken);
                // var response = await _httpClient.SendAsync(request, cancellationToken);
                //return response!;
			}
			catch (Exception)
			{

			}
			return default;
		}

		#endregion

		#region __GET_Orphan_Itemzs_Count__Async

		public async Task<int> __GET_Orphan_Itemzs_Count__Async()
		{
			return await __GET_Orphan_Itemzs_Count__Async(CancellationToken.None);
		}

		public async Task<int> __GET_Orphan_Itemzs_Count__Async(CancellationToken cancellationToken)
		{
			try
			{

				// TODO :: Utilize urlBuilder_ which is commented below.

				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append("/api/Itemzs/GetOrphanCount");
				//urlBuilder_.Append('?');

				//urlBuilder_.Length--;

				var response = await _httpClient.GetFromJsonAsync<int>("/api/Itemzs/GetOrphanCount", cancellationToken);

				return response!;
			}
			catch (Exception)
			{
			}
			return default;
		}

		#endregion

		#region __POST_Create_Itemz_Between_Existing_Itemz__Async
		public async Task<GetItemzDTO> __POST_Create_Itemz_Between_Existing_Itemz__Async(Guid? firstItemzId, Guid? secondItemzId, CreateItemzDTO createItemzDTO)
		{
			return await __POST_Create_Itemz_Between_Existing_Itemz__Async(firstItemzId, secondItemzId, createItemzDTO, CancellationToken.None);
		}

		public async Task<GetItemzDTO> __POST_Create_Itemz_Between_Existing_Itemz__Async(Guid? firstItemzId, Guid? secondItemzId, CreateItemzDTO createItemzDTO, CancellationToken cancellationToken)
		{

			try
			{
				// TODO :: Utilize urlBuilder which is commented below.

				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append("/api/Itemzs/CreateItemzBetweenExistingItemz");
				//urlBuilder_.Append('?');
				//if (firstItemzId != Guid.Empty)
				//{
				//	urlBuilder_.Append(System.Uri.EscapeDataString("firstItemzId")).Append('=').Append(System.Uri.EscapeDataString(firstItemzId.ToString()!)).Append('&');
				//}
				//if (secondItemzId != Guid.Empty)
				//{
				//	urlBuilder_.Append(System.Uri.EscapeDataString("secondItemzId")).Append('=').Append(System.Uri.EscapeDataString(secondItemzId.ToString()!)).Append('&');
				//}

				//urlBuilder_.Length--;

				if (createItemzDTO == null || string.IsNullOrWhiteSpace(createItemzDTO.Name))
				{
					throw new ArgumentNullException("Itemz Name is a required field for which value was not provided");
				}

				var httpResponseMessage = await _httpClient.PostAsJsonAsync($"/api/Itemzs/CreateItemzBetweenExistingItemz?firstItemzId={firstItemzId.ToString()}&secondItemzId={secondItemzId.ToString()}"
					, createItemzDTO, cancellationToken);

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
				var response = JsonSerializer.Deserialize<GetItemzDTO>(responseContent, options);
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

		#region __POST_Move_Itemz_Between_Existing_Itemz__Async

		public async Task __POST_Move_Itemz_Between_Existing_Itemz__Async(Guid? movingItemzId, Guid? firstItemzId, Guid? secondItemzId)
		{
			await __POST_Move_Itemz_Between_Existing_Itemz__Async(movingItemzId, firstItemzId, secondItemzId, CancellationToken.None);

		}

		public async Task __POST_Move_Itemz_Between_Existing_Itemz__Async(Guid? movingItemzId, Guid? firstItemzId, Guid? secondItemzId, CancellationToken cancellationToken)
		{
			try
			{
				// TODO :: Utilize urlBuilder which is commented below.

				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append("/api/Itemzs/MoveItemzBetweenExistingItemz");
				//urlBuilder_.Append('?');

				//if(movingItemzId != Guid.Empty)
				//{
				//	urlBuilder_.Append(System.Uri.EscapeDataString("movingItemzId")).Append('=').Append(System.Uri.EscapeDataString(movingItemzId.ToString()!)).Append('&');
				//}
				//else
				//{
				//	throw new ArgumentNullException(nameof(movingItemzId) + "is required for which value is not provided");
				//}

				//if (firstItemzId != Guid.Empty)
				//{
				//	urlBuilder_.Append(System.Uri.EscapeDataString("firstItemzId")).Append('=').Append(System.Uri.EscapeDataString(firstItemzId.ToString()!)).Append('&');
				//}
				//else
				//{
				//	throw new ArgumentNullException(nameof(firstItemzId) + "is required for which value is not provided");
				//}

				//if (secondItemzId != Guid.Empty)
				//{
				//	urlBuilder_.Append(System.Uri.EscapeDataString("secondItemzId")).Append('=').Append(System.Uri.EscapeDataString(secondItemzId.ToString()!)).Append('&');
				//}
				//else
				//{
				//	throw new ArgumentNullException(nameof(secondItemzId) + "is required for which value is not provided");
				//}

				//urlBuilder_.Length--;

				if (movingItemzId == Guid.Empty)
				{
					throw new ArgumentNullException(nameof(movingItemzId) + "is required for which value is not provided");
				}

				if (firstItemzId == Guid.Empty)
				{
					throw new ArgumentNullException(nameof(firstItemzId) + "is required for which value is not provided");
				}

				if (secondItemzId == Guid.Empty)
				{
					throw new ArgumentNullException(nameof(secondItemzId) + "is required for which value is not provided");
				}

				var httpResponseMessage = await _httpClient.PostAsJsonAsync($"/api/Itemzs/MoveItemzBetweenExistingItemz?movingItemzId={movingItemzId.ToString()}&firstItemzId={firstItemzId.ToString()}&secondItemzId={secondItemzId.ToString()}", movingItemzId,cancellationToken: cancellationToken);

				if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.Conflict)
				{
					// Read the response content
					var _errorContent = await httpResponseMessage.Content.ReadAsStringAsync();

					// TODO :: Use MudBlazor Snackbar to show the message (assuming MudBlazor Snackbar is set up)
					// TODO :: Do we need to pass server error message all the way to user UI? We need to check what's included in _errorContent though!
					throw new ApplicationException($"FAILED : {_errorContent}");
				}

				httpResponseMessage.EnsureSuccessStatusCode();

				// EXPLANATION :: Because we are not going to return any data from this specific method, we decided to comment out 
				// following code that proceses httpResponseMessage.Content 

				//string responseContent = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

				//// EXPLANATION :: HERE WE ARE SERIALIZING JSON RESPONSE INTO DESIRED CLASS / OBJECT FORMAT FOR RETURNING
				//var options = new JsonSerializerOptions
				//{
				//	PropertyNameCaseInsensitive = true,
				//};
				//var response = JsonSerializer.Deserialize<GetItemzDTO>(responseContent, options);
				//return (response ?? default);	
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

		#region __PUT_Update_Itemz_By_GUID_ID__Async
		public async Task __PUT_Update_Itemz_By_GUID_ID__Async(Guid itemzId, UpdateItemzDTO updateItemzDTO)
		{
			await __PUT_Update_Itemz_By_GUID_ID__Async(itemzId,updateItemzDTO, CancellationToken.None);
		}

		public async Task __PUT_Update_Itemz_By_GUID_ID__Async(Guid itemzId, UpdateItemzDTO updateItemzDTO, CancellationToken cancellationToken)
		{
			try
			{
				// TODO::Utilize urlBuilder which is commented below.

				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append($"/api/Itemzs/{itemzId.ToString()}");
				//urlBuilder_.Append('?');

				//urlBuilder_.Length--;

				if (updateItemzDTO == null || string.IsNullOrWhiteSpace(updateItemzDTO.Name))
				{
					throw new ArgumentNullException("Itemz Name is a required field for which value was not provided");
				}


				var httpResponseMessage = await _httpClient.PutAsJsonAsync($"/api/Itemzs/{itemzId.ToString()}", updateItemzDTO,  cancellationToken);

				if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.Conflict)
				{
					// Read the response content
					var _errorContent = await httpResponseMessage.Content.ReadAsStringAsync();

					// TODO :: Use MudBlazor Snackbar to show the message (assuming MudBlazor Snackbar is set up)
					// TODO :: Do we need to pass server error message all the way to user UI? We need to check what's included in _errorContent though!
					throw new ApplicationException($"FAILED : {_errorContent}");
				}

				httpResponseMessage.EnsureSuccessStatusCode();
				
				
				// EXPLANATION :: Because we are not going to return any data from this specific method, we decided to comment out 
				// following code that proceses httpResponseMessage.Content 

				//string responseContent = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

				//// EXPLANATION :: HERE WE ARE SERIALIZING JSON RESPONSE INTO DESIRED CLASS / OBJECT FORMAT FOR RETURNING
				//var options = new JsonSerializerOptions
				//{
				//	PropertyNameCaseInsensitive = true,
				//};
				//var response = JsonSerializer.Deserialize<GetItemzDTO>(responseContent, options);
				//return (response ?? default);
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

		#region __DELETE_Itemz_By_GUID_ID__Async

		public async Task __DELETE_Itemz_By_GUID_ID__Async(Guid itemzId)
		{
			await __DELETE_Itemz_By_GUID_ID__Async(itemzId, CancellationToken.None);
		}

		public async Task __DELETE_Itemz_By_GUID_ID__Async(Guid itemzId, CancellationToken cancellationToken)
		{
			try
			{
				if (itemzId == Guid.Empty)
				{
					throw new ArgumentNullException(nameof(itemzId) + "is required for which value is not provided");
				}

				// TODO::Utilize urlBuilder which is commented below.

				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append($"/api/Itemzs/{itemzId.ToString()}");
				//urlBuilder_.Append('?');

				//urlBuilder_.Length--;

				var httpResponseMessage = await _httpClient.DeleteAsync($"/api/Itemzs/{itemzId.ToString()}", cancellationToken);
				httpResponseMessage.EnsureSuccessStatusCode();
				string responseContent = httpResponseMessage.Content.ReadAsStringAsync().Result;

			}
			catch (Exception)
			{
			}

		}
		#endregion

		#region __Delete_All_Orphan_Itemz__Async

		public async Task __Delete_All_Orphan_Itemz__Async()
		{
			await __Delete_All_Orphan_Itemz__Async(CancellationToken.None);
		}

		public async Task __Delete_All_Orphan_Itemz__Async(CancellationToken cancellationToken)
		{
			try
			{
				// TODO::Utilize urlBuilder which is commented below.

				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append($"/api/Itemzs/DeleteAllOrphanItemz");
				//urlBuilder_.Append('?');

				//urlBuilder_.Length--;

				var httpResponseMessage = await _httpClient.DeleteAsync($"/api/Itemzs/DeleteAllOrphanItemz", cancellationToken);
				httpResponseMessage.EnsureSuccessStatusCode();
				string responseContent = httpResponseMessage.Content.ReadAsStringAsync().Result;

			}
			catch (Exception)
			{
				throw new NotImplementedException();
			}
		}

		#endregion

		#region __POST_Move_Itemz__Async

		public async Task __POST_Move_Itemz__Async(Guid movingItemzId, Guid targetId, bool atBottomOfChildNodes = true)
		{
			await __POST_Move_Itemz__Async(movingItemzId, targetId, atBottomOfChildNodes, CancellationToken.None);
		}

		public async Task __POST_Move_Itemz__Async(Guid movingItemzId, Guid targetId, bool atBottomOfChildNodes, CancellationToken cancellationToken)
		{
			try
			{
				// TODO::Utilize urlBuilder which is commented below.

				var urlBuilder_ = new System.Text.StringBuilder();
				Guid tempMovingItemzId;
				Guid tempTargetId;
				if (movingItemzId == Guid.Empty)
				{
					throw new ArgumentNullException(nameof(movingItemzId) + "is required for which value is not provided");
				}

				urlBuilder_.Append($"/api/Itemzs/");
				urlBuilder_.Append(System.Uri.EscapeDataString(movingItemzId.ToString()!));
				urlBuilder_.Append('?');

				if (targetId != Guid.Empty)
				{
					urlBuilder_.Append(System.Uri.EscapeDataString("TargetId")).Append('=').Append(System.Uri.EscapeDataString(targetId.ToString()!)).Append('&');
				}
				else
				{
					throw new ArgumentNullException(nameof(targetId) + "is required for which value is not provided");
				}

				if ((bool)atBottomOfChildNodes!)
				{
					urlBuilder_.Append(System.Uri.EscapeDataString("AtBottomOfChildNodes")).Append('=').Append(System.Uri.EscapeDataString("true")).Append('&');
				}
				else
				{
					urlBuilder_.Append(System.Uri.EscapeDataString("AtBottomOfChildNodes")).Append('=').Append(System.Uri.EscapeDataString("false")).Append('&');
				}

				urlBuilder_.Length--;

				//tempMovingItemzId = movingItemzId;
				//tempTargetId = targetId;
				//var httpResponseMessage = await _httpClient.PostAsJsonAsync($"/api/Itemzs/{tempMovingItemzId}?TargetId={tempTargetId}&AtBottomOfChildNodes=true", cancellationToken);

				// EXPLANATION :: WOW THIS WAS TOUGH ONE. _httpClient.PostAsJsonAsync was not happy until I passed in 'targetId' OR SOME SERIALIZABLE OBJECT 
				// as second parameter. It just kept giving error while I was passing in only cancellationToken as parameter.
				// This is because cancellationToken has to be 3rd or further down parameter. It can't be 2nd Parameter!
				// I kept getting error as ... NotSupportedException: Serialization and deserialization of 'System.IntPtr' instances is not supported.

				var httpResponseMessage = await _httpClient.PostAsJsonAsync(requestUri: $"{urlBuilder_}", targetId, cancellationToken: cancellationToken);

				////MoveItemzRequest payload = new MoveItemzRequest();
				////payload.MovingItemzId = movingItemzId;
				////payload.TargetId = targetId;
				////payload.AtBottomOfChildNodes = true;

				////var httpResponseMessage = await _httpClient.PostAsJsonAsync($"/api/Itemzs/{payload.MovingItemzId}?TargetId={payload.TargetId}&AtBottomOfChildNodes={payload.AtBottomOfChildNodes}", payload);


				if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.Conflict)
				{
					// Read the response content
					var _errorContent = await httpResponseMessage.Content.ReadAsStringAsync();

					// TODO :: Use MudBlazor Snackbar to show the message (assuming MudBlazor Snackbar is set up)
					// TODO :: Do we need to pass server error message all the way to user UI? We need to check what's included in _errorContent though!
					throw new ApplicationException($"FAILED : {_errorContent}");
				}

				httpResponseMessage.EnsureSuccessStatusCode();

				// EXPLANATION :: Because we are not going to return any data from this specific method, we decided to comment out 
				// following code that proceses httpResponseMessage.Content 

				//string responseContent = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

				//// EXPLANATION :: HERE WE ARE SERIALIZING JSON RESPONSE INTO DESIRED CLASS / OBJECT FORMAT FOR RETURNING
				//var options = new JsonSerializerOptions
				//{
				//	PropertyNameCaseInsensitive = true,
				//};
				//var response = JsonSerializer.Deserialize<GetItemzDTO>(responseContent, options);
				//return (response ?? default);					
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

	//public class MoveItemzRequest 
	//{ 
	//	public Guid MovingItemzId { get; set; } 
	//	public Guid TargetId { get; set; } 
	//	public bool AtBottomOfChildNodes { get; set; } 
	//}
}