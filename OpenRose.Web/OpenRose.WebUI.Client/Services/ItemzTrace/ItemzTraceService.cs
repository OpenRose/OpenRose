// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.SharedModels;
using OpenRose.WebUI.Client.Utilities;
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

namespace OpenRose.WebUI.Client.Services.ItemzTrace
{
	public class ItemzTraceService : IItemzTraceService
	{
		private readonly HttpClient _httpClient;

		public ItemzTraceService(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		#region __GET_Check_Itemz_Trace_Exists__Async

		public async Task<ItemzTraceDTO> __GET_Check_Itemz_Trace_Exists__Async(Guid? fromTraceItemzId, Guid? toTraceItemzId)
		{
			return await __GET_Check_Itemz_Trace_Exists__Async(fromTraceItemzId, toTraceItemzId, CancellationToken.None);
		}

		public async Task<ItemzTraceDTO> __GET_Check_Itemz_Trace_Exists__Async(Guid? fromTraceItemzId, Guid? toTraceItemzId, CancellationToken cancellationToken)
		{
			try
			{

				// TODO :: Utilize urlBuilder which is commented below.

				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append("/api/ItemzTrace/CheckExists");
				//urlBuilder_.Append('?');

				//if (fromTraceItemzId != Guid.Empty)
				//{
				//	urlBuilder_.Append(System.Uri.EscapeDataString("fromTraceItemzId")).Append('=').Append(System.Uri.EscapeDataString(fromTraceItemzId.ToString()!)).Append('&');
				//}
				//else
				//{
				//	throw new ArgumentNullException(nameof(fromTraceItemzId) + "is required for which value is not provided");
				//}

				//if (toTraceItemzId != Guid.Empty)
				//{
				//	urlBuilder_.Append(System.Uri.EscapeDataString("toTraceItemzId")).Append('=').Append(System.Uri.EscapeDataString(toTraceItemzId.ToString()!)).Append('&');
				//}
				//else
				//{
				//	throw new ArgumentNullException(nameof(toTraceItemzId) + "is required for which value is not provided");
				//}

				//urlBuilder_.Length--;


				var response = await _httpClient.GetFromJsonAsync<ItemzTraceDTO>($"/api/ItemzTrace/CheckExists?fromTraceItemzId={fromTraceItemzId.ToString()}&toTraceItemzId={toTraceItemzId.ToString()}", cancellationToken);

				return response!;
			}
			catch (Exception)
			{
			}
			return default;
		}

		#endregion

		#region __POST_Establish_Trace_Between_Itemz__Async
		public async Task<ItemzTraceDTO> __POST_Establish_Trace_Between_Itemz__Async(ItemzTraceDTO body)
		{
			return await __POST_Establish_Trace_Between_Itemz__Async(body, CancellationToken.None);
		}

		public async Task<ItemzTraceDTO> __POST_Establish_Trace_Between_Itemz__Async(ItemzTraceDTO itemzTraceDTO, CancellationToken cancellationToken)
		{
			try
			{
				// TODO :: Utilize urlBuilder which is commented below.

				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append("/api/ItemzTrace");
				//urlBuilder_.Append('?');
				//if (itemzTraceDTO == null)
				//{
				//	throw new ArgumentNullException(nameof(itemzTraceDTO) + "is required for which value is not provided");
				//}

				//urlBuilder_.Length--;

				var httpResponseMessage = await _httpClient.PostAsJsonAsync($"/api/ItemzTrace", itemzTraceDTO, cancellationToken);

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

				// EXPLANATION :: HERE WE ARE SERIALIZING JSON RESPONSE INTO DESIRED CLASS / OBJECT FORMAT FOR RETURNING
				var options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true,
				};
				var response = JsonSerializer.Deserialize<ItemzTraceDTO>(responseContent, options);
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

		#region __DELETE_Itemz_Trace__Async
		public async Task __DELETE_Itemz_Trace__Async(ItemzTraceDTO itemzTraceDTO)
		{
			await __DELETE_Itemz_Trace__Async(itemzTraceDTO, CancellationToken.None);
		}

		public async Task __DELETE_Itemz_Trace__Async(ItemzTraceDTO itemzTraceDTO, CancellationToken cancellationToken)
		{
			try
			{
				// TODO::Utilize urlBuilder which is commented below.

				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append($"/api/ItemzTrace");
				//urlBuilder_.Append('?');

				//if (itemzTraceDTO == null)
				//{
				//	throw new ArgumentNullException(nameof(itemzTraceDTO) + "is required for which value is not provided");
				//}

				//urlBuilder_.Length--;

				if (itemzTraceDTO == null || itemzTraceDTO.FromTraceItemzId == Guid.Empty || itemzTraceDTO.ToTraceItemzId == Guid.Empty)
				{
					throw new ArgumentNullException(nameof(itemzTraceDTO) + "is required for which value is not provided");
				}

				// EXPLANATION :: Because .NET does not provide option to send Delete request with json body included 
				// we are manually creating request to send JSON body. We found this information at ... 
				// https://stackoverflow.com/questions/28054515/how-to-send-delete-with-json-to-the-rest-api-using-httpclient

				string jsonString = JsonSerializer.Serialize(itemzTraceDTO);

				var request = new HttpRequestMessage
				{
					Content = new StringContent(jsonString, Encoding.UTF8, "application/json"),
					Method = HttpMethod.Delete,
					RequestUri = new Uri("/api/ItemzTrace", UriKind.Relative)
				};
				var httpResponseMessage = await _httpClient.SendAsync(request);

				//var httpResponseMessage = await _httpClient.DeleteAsync($"/api/ItemzTrace", itemzTraceDTO, cancellationToken);

				if (HttpStatusCodesHelper.ErrorStatusCodes.Contains(httpResponseMessage.StatusCode))
				{
					// Read the response content
					var _errorContent = await httpResponseMessage.Content.ReadAsStringAsync();

					// TODO :: Use MudBlazor Snackbar to show the message (assuming MudBlazor Snackbar is set up)
					// TODO :: Do we need to pass server error message all the way to user UI? We need to check what's included in _errorContent though!
					throw new ApplicationException($"FAILED : {_errorContent}");
				}

				httpResponseMessage.EnsureSuccessStatusCode();

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

		#region __DELETE_AllFromItemz_Trace__Async
		public async Task __DELETE_AllFromItemz_Trace__Async(Guid itemzID)
		{
			await __DELETE_AllFromItemz_Trace__Async(itemzID, CancellationToken.None);
		}

		public async Task __DELETE_AllFromItemz_Trace__Async(Guid itemzID, CancellationToken cancellationToken)
		{
			try
			{
				if (itemzID == Guid.Empty)
				{
					throw new ArgumentNullException(nameof(itemzID) + "is required for which value is not provided");
				}

				// TODO::Utilize urlBuilder which is commented below.
				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append($"/api/ItemzTrac/api/ItemzTrace/DeleteAllFromItemzTraces/{itemzID.ToString()}");
				//urlBuilder_.Append('?');

				//urlBuilder_.Length--;

				var httpResponseMessage = await _httpClient.DeleteAsync($"/api/ItemzTrace/DeleteAllFromItemzTraces/{itemzID.ToString()}", cancellationToken);
				
				if (HttpStatusCodesHelper.ErrorStatusCodes.Contains(httpResponseMessage.StatusCode))
				{
					// Read the response content
					var _errorContent = await httpResponseMessage.Content.ReadAsStringAsync();

					// TODO :: Use MudBlazor Snackbar to show the message (assuming MudBlazor Snackbar is set up)
					// TODO :: Do we need to pass server error message all the way to user UI? We need to check what's included in _errorContent though!
					throw new ApplicationException($"FAILED : {_errorContent}");
				}
				
				httpResponseMessage.EnsureSuccessStatusCode();

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

		#region __DELETE_AllToItemz_Trace__Async
		public async Task __DELETE_AllToItemz_Trace__Async(Guid itemzID)
		{
			await __DELETE_AllToItemz_Trace__Async(itemzID, CancellationToken.None);
		}

		public async Task __DELETE_AllToItemz_Trace__Async(Guid itemzID, CancellationToken cancellationToken)
		{
			try
			{
				if (itemzID == Guid.Empty)
				{
					throw new ArgumentNullException(nameof(itemzID) + "is required for which value is not provided");
				}

				// TODO::Utilize urlBuilder which is commented below.
				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append($"/api/ItemzTrace/DeleteAllToItemzTraces/{itemzID.ToString()}");
				//urlBuilder_.Append('?');

				//urlBuilder_.Length--;

				var httpResponseMessage = await _httpClient.DeleteAsync($"/api/ItemzTrace/DeleteAllToItemzTraces/{itemzID.ToString()}", cancellationToken);

				if (HttpStatusCodesHelper.ErrorStatusCodes.Contains(httpResponseMessage.StatusCode))
				{
					// Read the response content
					var _errorContent = await httpResponseMessage.Content.ReadAsStringAsync();

					// TODO :: Use MudBlazor Snackbar to show the message (assuming MudBlazor Snackbar is set up)
					// TODO :: Do we need to pass server error message all the way to user UI? We need to check what's included in _errorContent though!
					throw new ApplicationException($"FAILED : {_errorContent}");
				}

				httpResponseMessage.EnsureSuccessStatusCode();

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

		#region __GET_Itemz_Traces_By_ItemzID__Async
		public async Task<ICollection<ItemzTraceDTO>> __GET_Itemz_Traces_By_ItemzID__Async(Guid itemzId)
		{
			return await __GET_Itemz_Traces_By_ItemzID__Async(itemzId, CancellationToken.None);
		}
		public async Task<ICollection<ItemzTraceDTO>> __GET_Itemz_Traces_By_ItemzID__Async(Guid itemzId, CancellationToken cancellationToken)
		{
			try
			{
				if (itemzId == Guid.Empty)
				{
					throw new ArgumentNullException(nameof(itemzId) + "is required for which value is not provided");
				}
				
				// TODO :: Utilize urlBuilder which is commented below.
				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append($"/api/ItemzTrace/{itemzId.ToString()}");
				//urlBuilder_.Append('?');

				//urlBuilder_.Length--;

				var response = await _httpClient.GetFromJsonAsync<ICollection<ItemzTraceDTO>>($"/api/ItemzTrace/{itemzId.ToString()}", cancellationToken);

				return response!;
			}
			catch (Exception)
			{

			}
			return default;
		}
		#endregion

		#region __GET_All_Parent_and_Child_Itemz_Traces_By_ItemzID__Async
		public async Task<ItemzParentAndChildTraceDTO> __GET_All_Parent_and_Child_Itemz_Traces_By_ItemzID__Async(Guid itemzId)
		{
			return await __GET_All_Parent_and_Child_Itemz_Traces_By_ItemzID__Async(itemzId, CancellationToken.None);
		}
		public async Task<ItemzParentAndChildTraceDTO> __GET_All_Parent_and_Child_Itemz_Traces_By_ItemzID__Async(Guid itemzId, CancellationToken cancellationToken)
		{
			try
			{
				if (itemzId == Guid.Empty)
				{
					throw new ArgumentNullException(nameof(itemzId) + "is required for which value is not provided");
				}

				// TODO :: Utilize urlBuilder which is commented below.
				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append($"/api/ItemzTrace/AllItemzTraces/{itemzId.ToString()}");
				//urlBuilder_.Append('?');

				//urlBuilder_.Length--;

				var response = await _httpClient.GetFromJsonAsync<ItemzParentAndChildTraceDTO>($"/api/ItemzTrace/AllItemzTraces/{itemzId.ToString()}", cancellationToken);

				return response!;
			}
			catch (Exception)
			{

			}
			return default;
		}
		#endregion

		#region __GET_FromItemz_Count_By_ItemzID__Async
		public async Task<int> __GET_FromItemz_Count_By_ItemzID__Async(Guid itemzId)
		{
			return await __GET_FromItemz_Count_By_ItemzID__Async(itemzId, CancellationToken.None);
		}
		public async Task<int> __GET_FromItemz_Count_By_ItemzID__Async(Guid itemzId, CancellationToken cancellationToken)
		{
			try
			{
				if (itemzId == Guid.Empty)
				{
					throw new ArgumentNullException(nameof(itemzId) + "is required for which value is not provided");
				}

				// TODO :: Utilize urlBuilder which is commented below.
				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append($"/api/ItemzTrace/GetFromItemzTraceCount/{itemzId.ToString()}");
				//urlBuilder_.Append('?');

				//urlBuilder_.Length--;

				var response = await _httpClient.GetFromJsonAsync<int>($"/api/ItemzTrace/GetFromItemzTraceCount/{itemzId.ToString()}", cancellationToken);

				return response!;
			}
			catch (Exception)
			{

			}
			return default;
		}
		#endregion

		#region __GET_ToItemz_Count_By_ItemzID__Async

		public async Task<int> __GET_ToItemz_Count_By_ItemzID__Async(Guid itemzId)
		{
			return await __GET_ToItemz_Count_By_ItemzID__Async(itemzId, CancellationToken.None);
		}
		public async Task<int> __GET_ToItemz_Count_By_ItemzID__Async(Guid itemzId, CancellationToken cancellationToken)
		{
			try
			{
				if (itemzId == Guid.Empty)
				{
					throw new ArgumentNullException(nameof(itemzId) + "is required for which value is not provided");
				}

				// TODO :: Utilize urlBuilder which is commented below.
				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append($"/api/ItemzTrace/GetToItemzTraceCount/{itemzId.ToString()}");
				//urlBuilder_.Append('?');

				//urlBuilder_.Length--;

				var response = await _httpClient.GetFromJsonAsync<int>($"/api/ItemzTrace/GetToItemzTraceCount/{itemzId.ToString()}", cancellationToken);

				return response!;
			}
			catch (Exception)
			{

			}
			return default;
		}

		#endregion

		#region __GET_All_From_and_To_Itemz_Traces_Count_By_ItemzID__Async
		public async Task<int> __GET_All_From_and_To_Itemz_Traces_Count_By_ItemzID__Async(Guid itemzId)
		{
			return await __GET_All_From_and_To_Itemz_Traces_Count_By_ItemzID__Async(itemzId, CancellationToken.None);
		}
		public async Task<int> __GET_All_From_and_To_Itemz_Traces_Count_By_ItemzID__Async(Guid itemzId, CancellationToken cancellationToken)
		{
			try
			{
				if (itemzId == Guid.Empty)
				{
					throw new ArgumentNullException(nameof(itemzId) + "is required for which value is not provided");
				}

				// TODO :: Utilize urlBuilder which is commented below.
				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append($"/api/ItemzTrace/GetAllFromAndToTracesCountByItemzId/{itemzId.ToString()}");
				//urlBuilder_.Append('?');

				//urlBuilder_.Length--;

				var response = await _httpClient.GetFromJsonAsync<int>($"/api/ItemzTrace/GetAllFromAndToTracesCountByItemzId/{itemzId.ToString()}", cancellationToken);

				return response!;
			}
			catch (Exception)
			{

			}
			return default;
		}
		#endregion

		#region __POST_Create_Or_Verify_Itemz_Trace_Collection__Async
		public async Task<ICollection<ItemzTraceDTO>> __POST_Create_Or_Verify_Itemz_Trace_Collection__Async(IEnumerable<ItemzTraceDTO> body)
		{
			return await __POST_Create_Or_Verify_Itemz_Trace_Collection__Async(body, CancellationToken.None);
		}
		public async Task<ICollection<ItemzTraceDTO>> __POST_Create_Or_Verify_Itemz_Trace_Collection__Async(IEnumerable<ItemzTraceDTO> body, CancellationToken cancellationToken)
		{
			try
			{
				// TODO :: Utilize urlBuilder which is commented below.

				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append("/api/ItemzTrace/CreateOrVerifyItemzTraceCollection");
				//urlBuilder_.Append('?');
				//if (body == null)
				//{
				//	throw new ArgumentNullException(nameof(body) + "is required for which value is not provided");
				//}

				//urlBuilder_.Length--;

				var httpResponseMessage = await _httpClient.PostAsJsonAsync($"/api/ItemzTrace/CreateOrVerifyItemzTraceCollection", body, cancellationToken);

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

				// EXPLANATION :: HERE WE ARE SERIALIZING JSON RESPONSE INTO DESIRED CLASS / OBJECT FORMAT FOR RETURNING
				var options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true,
				};
				var response = JsonSerializer.Deserialize< ICollection<ItemzTraceDTO>>(responseContent, options);
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

		#region __DELETE_Itemz_Trace_Collection__Async
		public async Task __DELETE_Itemz_Trace_Collection__Async(IEnumerable<ItemzTraceDTO> body)
		{
			await __DELETE_Itemz_Trace_Collection__Async(body, CancellationToken.None);
		}
		public async Task __DELETE_Itemz_Trace_Collection__Async(IEnumerable<ItemzTraceDTO> body, CancellationToken cancellationToken)
		{
			try
			{
				// TODO::Utilize urlBuilder which is commented below.

				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append($"/api/ItemzTrace/DeleteItemzTraceCollection");
				//urlBuilder_.Append('?');

				//if (body == null)
				//{
				//	throw new ArgumentNullException(nameof(body) + "is required for which value is not provided");
				//}

				//urlBuilder_.Length--;

				// EXPLANATION :: Because .NET does not provide option to send Delete request with json body included 
				// we are manually creating request to send JSON body. We found this information at ... 
				// https://stackoverflow.com/questions/28054515/how-to-send-delete-with-json-to-the-rest-api-using-httpclient

				// EXPLANATION :: HERE WE ARE SERIALIZING DESIRED CLASS / OBJECT FORMAT INTO JSON FORMAT FOR REQUEST CONTENT 
				var options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true,
				};
				var json_ = JsonSerializer.Serialize<IEnumerable<ItemzTraceDTO>>(body, options);

				var request = new HttpRequestMessage
				{
					Content = new StringContent($"{json_}", Encoding.UTF8, "application/json"),
					Method = HttpMethod.Delete,
					RequestUri = new Uri("/api/ItemzTrace/DeleteItemzTraceCollection")
				};
				var httpResponseMessage = await _httpClient.SendAsync(request);

				//var httpResponseMessage = await _httpClient.DeleteAsync($"/api/ItemzTrace/DeleteItemzTraceCollection", body, cancellationToken);

				if (HttpStatusCodesHelper.ErrorStatusCodes.Contains(httpResponseMessage.StatusCode))
				{
					// Read the response content
					var _errorContent = await httpResponseMessage.Content.ReadAsStringAsync();

					// TODO :: Use MudBlazor Snackbar to show the message (assuming MudBlazor Snackbar is set up)
					// TODO :: Do we need to pass server error message all the way to user UI? We need to check what's included in _errorContent though!
					throw new ApplicationException($"FAILED : {_errorContent}");
				}

				httpResponseMessage.EnsureSuccessStatusCode();

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