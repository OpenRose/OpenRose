// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.Services.ItemzChangeHistoryByProjectService;
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

namespace OpenRose.WebUI.Client.Services.ItemzChangeHistoryByProjectService
{
	public class ItemzChangeHistoryByProjectService : IItemzChangeHistoryByProjectService
	{
		private readonly HttpClient _httpClient;

		public ItemzChangeHistoryByProjectService(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		#region __DELETE_Itemz_Change_History_By_Project_GUID_ID__Async
		public async Task<int> __DELETE_Itemz_Change_History_By_Project_GUID_ID__Async(DeleteChangeHistoryDTO body)
		{
			return await __DELETE_Itemz_Change_History_By_Project_GUID_ID__Async(body, CancellationToken.None);
		}

		public async Task<int> __DELETE_Itemz_Change_History_By_Project_GUID_ID__Async(DeleteChangeHistoryDTO body, CancellationToken cancellationToken)
		{
			try
			{

				if (body == null || body.Id == Guid.Empty)
				{
					throw new ArgumentNullException(nameof(body) + "is required for which value is not provided");
				}

				// TODO::Utilize urlBuilder which is commented below.

				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append($"/api/ItemzChangeHistoryByProject");
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
					RequestUri = new Uri($"/api/ItemzChangeHistoryByProject")
				};
				var httpResponseMessage = await _httpClient.SendAsync(request);

				//var httpResponseMessage = await _httpClient.DeleteAsync($"/api/ItemzChangeHistoryByProject", body, cancellationToken);

				if (HttpStatusCodesHelper.ErrorStatusCodes.Contains(httpResponseMessage.StatusCode))
				{
					// Read the response content
					var _errorContent = await httpResponseMessage.Content.ReadAsStringAsync();

					// TODO :: Use MudBlazor Snackbar to show the message (assuming MudBlazor Snackbar is set up)
					// TODO :: Do we need to pass server error message all the way to user UI? We need to check what's included in _errorContent though!
					throw new ApplicationException($"FAILED : {_errorContent}");
				}

				httpResponseMessage.EnsureSuccessStatusCode();

				string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();

				if (int.TryParse(responseContent, out int responseInt))
				{
					return responseInt;
				}
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

			return default;
		}
		#endregion

		#region __GET_Number_of_ItemzChangeHistory_By_Project_Upto_DateTime__Async
		public async Task<int> __GET_Number_of_ItemzChangeHistory_By_Project_Upto_DateTime__Async(GetNumberOfChangeHistoryDTO body)
		{
			return await __GET_Number_of_ItemzChangeHistory_By_Project_Upto_DateTime__Async(body, CancellationToken.None);
		}
		public async Task<int> __GET_Number_of_ItemzChangeHistory_By_Project_Upto_DateTime__Async(GetNumberOfChangeHistoryDTO body, CancellationToken cancellationToken)
		{
			try
			{
				if (body == null)
				{
					throw new ArgumentNullException(nameof(body) + "is required for which value is not provided");
				}

				// TODO::Utilize urlBuilder which is commented below.

				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append($"/api/ItemzChangeHistoryByProject");
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
				var json_ = JsonSerializer.Serialize<GetNumberOfChangeHistoryDTO>(body, options);

				var request = new HttpRequestMessage
				{
					Content = new StringContent($"{json_}", Encoding.UTF8, "application/json"),
					Method = HttpMethod.Get, // TODO: VERIFY THAT THIS GET METHOD WORKS AS I CHANGED DELETE TO GET HERE.
					RequestUri = new Uri($"/api/ItemzChangeHistoryByProject")
				};
				var httpResponseMessage = await _httpClient.SendAsync(request);

				//var httpResponseMessage = await _httpClient.DeleteAsync($"/api/ItemzChangeHistoryByProject", body, cancellationToken);

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

		#region __GET_Number_of_ItemzChangeHistory_By_Project__Async
		public async Task<int> __GET_Number_of_ItemzChangeHistory_By_Project__Async(Guid projectId)
		{
			return await __GET_Number_of_ItemzChangeHistory_By_Project__Async(projectId, CancellationToken.None);
		}

		public async Task<int> __GET_Number_of_ItemzChangeHistory_By_Project__Async(Guid projectId, CancellationToken cancellationToken)
		{
			try
			{
				if (projectId == Guid.Empty)
				{
					throw new ArgumentNullException(nameof(projectId) + "is required for which value is not provided");
				}

				// TODO::Utilize urlBuilder which is commented below.
				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append($"/api/ItemzChangeHistoryByProject/{projectId.ToString()}");
				//urlBuilder_.Append('?');

				//urlBuilder_.Length--;

				var response = await _httpClient.GetFromJsonAsync<int>($"/api/ItemzChangeHistoryByProject/{projectId.ToString()}", cancellationToken);

				return response!;
			}
			catch (Exception)
			{

			}
			return default;
		}

		#endregion

	}
}