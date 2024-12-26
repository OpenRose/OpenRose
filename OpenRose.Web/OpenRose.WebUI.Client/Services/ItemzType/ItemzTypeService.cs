// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.SharedModels;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenRose.WebUI.Client.Services.ItemzType
{
    public class ItemzTypeService : IItemzTypeService
    {
        private readonly HttpClient _httpClient;

        public ItemzTypeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        #region __Single_ItemzType_By_GUID_ID__Async
        public async Task<GetItemzTypeDTO?> __Single_ItemzType_By_GUID_ID__Async(Guid itemzTypeId)
        {
            return await __Single_ItemzType_By_GUID_ID__Async(itemzTypeId, CancellationToken.None);
        }


        public async Task<GetItemzTypeDTO?> __Single_ItemzType_By_GUID_ID__Async(Guid itemzTypeId, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<GetItemzTypeDTO>($"/api/ItemzTypes/{itemzTypeId}", cancellationToken);

                return response!;
            }
            catch (Exception)
            {
            }
            return default;
        }

        #endregion

        #region __PUT_Update_ItemzType_By_GUID_ID (NSWAG - IDAsync)
        public async Task<GetItemzTypeDTO?> __PUT_Update_ItemzType_By_GUID_ID__Async(Guid itemzTypeId, UpdateItemzTypeDTO updateItemzTypeDTO)
        {
            try
            {
                return await __PUT_Update_ItemzType_By_GUID_ID__Async(itemzTypeId, updateItemzTypeDTO, CancellationToken.None);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<GetItemzTypeDTO?> __PUT_Update_ItemzType_By_GUID_ID__Async(Guid itemzTypeId, UpdateItemzTypeDTO updateItemzTypeDTO, CancellationToken cancellationToken)
        {
            try
            {
                var httpResponseMessage = await _httpClient.PutAsJsonAsync($"/api/ItemzTypes/{itemzTypeId}", updateItemzTypeDTO, cancellationToken);
                httpResponseMessage.EnsureSuccessStatusCode();

                string responseContent = httpResponseMessage.Content.ReadAsStringAsync().Result;
                // TODO :: Send back updated content for GetProjectDTO
                return default;
            }
            catch (Exception)
            {
                throw;
            }
            // return default;
        }

        #endregion

        #region __POST_Create_ItemzType__Async
        public async Task<GetItemzTypeDTO?> __POST_Create_ItemzType__Async(CreateItemzTypeDTO createItemzTypeDTO)
        {
            return await __POST_Create_ItemzType__Async(createItemzTypeDTO, CancellationToken.None);
        }

        public async Task<GetItemzTypeDTO?> __POST_Create_ItemzType__Async(CreateItemzTypeDTO createItemzTypeDTO, CancellationToken cancellationToken)
        {
            try
            {
				if (createItemzTypeDTO == null || string.IsNullOrWhiteSpace(createItemzTypeDTO.Name))
				{
					throw new ArgumentNullException("Project Name is a required field for which value was not provided");
				}


				var httpResponseMessage = await _httpClient.PostAsJsonAsync($"/api/ItemzTypes", createItemzTypeDTO, cancellationToken);
				
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
				var response = JsonSerializer.Deserialize<GetItemzTypeDTO>(responseContent, options);
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

        #region __DELETE_ItemzType_By_GUID_ID__Async
        public async Task __DELETE_ItemzType_By_GUID_ID__Async(Guid itemzTypeId)
        {
            await __DELETE_ItemzType_By_GUID_ID__Async(itemzTypeId, CancellationToken.None);
        }

        public async Task __DELETE_ItemzType_By_GUID_ID__Async(Guid itemzTypeId, CancellationToken cancellationToken)
        {
            try
            {
                var httpResponseMessage = await _httpClient.DeleteAsync($"/api/ItemzTypes/{itemzTypeId}", cancellationToken);
                httpResponseMessage.EnsureSuccessStatusCode();
                string responseContent = httpResponseMessage.Content.ReadAsStringAsync().Result;
                // TODO :: Send back updated content for GetProjectDTO
            }
            catch (Exception)
            {
            }
        }

        #endregion

        #region __POST_Move_ItemzType__Async
        public async Task __POST_Move_ItemzType__Async(Guid movingItemzTypeId, Guid? targetProjectId, bool? atBottomOfChildNodes)
        {
            await __POST_Move_ItemzType__Async(movingItemzTypeId, targetProjectId, atBottomOfChildNodes, CancellationToken.None);
        }

        public async Task __POST_Move_ItemzType__Async(Guid movingItemzTypeId, Guid? targetProjectId, bool? atBottomOfChildNodes, CancellationToken cancellationToken)
        {
			HttpResponseMessage httpResponseMessage = null;
			try
            {
                httpResponseMessage = await _httpClient.PostAsJsonAsync($"/api/ItemzTypes/{movingItemzTypeId}?TargetProjectId={targetProjectId}&AtBottomOfChildNodes={atBottomOfChildNodes}", movingItemzTypeId, cancellationToken: cancellationToken);
				httpResponseMessage.EnsureSuccessStatusCode();
				string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
			}
			catch (HttpRequestException httpRequestException)
			{
				string errorMessage = $"Error: {httpRequestException.StatusCode}";

				// Read the response content if available
				if (httpResponseMessage != null)
				{
					var responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
					errorMessage += $": {responseContent}";
				}

//				throw new ApplicationException(errorMessage, httpRequestException);
				throw new ApplicationException(errorMessage);
			}
			catch (Exception ex)
			{
				throw new ApplicationException("An unexpected error occurred.", ex);
			}
		}

		#endregion

		#region __POST_Move_ItemzType_Between_ItemzTypes__Async
		public async Task __POST_Move_ItemzType_Between_ItemzTypes__Async(Guid? movingItemzTypeId, Guid? firstItemzTypeId, Guid? secondItemzTypeId)
		{
			await __POST_Move_ItemzType_Between_ItemzTypes__Async(movingItemzTypeId, firstItemzTypeId, secondItemzTypeId, CancellationToken.None);
        }

        public async Task __POST_Move_ItemzType_Between_ItemzTypes__Async(Guid? movingItemzTypeId, Guid? firstItemzTypeId, Guid? secondItemzTypeId, CancellationToken cancellationToken)
        {
			HttpResponseMessage httpResponseMessage = null;
			try
			{
				httpResponseMessage = await _httpClient.PostAsJsonAsync($"/api/ItemzTypes/MoveItemzTypeBetweenItemzTypes?movingItemzTypeId={movingItemzTypeId}&firstItemzTypeId={firstItemzTypeId}&secondItemzTypeId={secondItemzTypeId}", movingItemzTypeId, cancellationToken: cancellationToken);
				httpResponseMessage.EnsureSuccessStatusCode();
				string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
			}
			catch (HttpRequestException httpRequestException)
			{
				string errorMessage = $"Error: {httpRequestException.StatusCode}";

				// Read the response content if available
				if (httpResponseMessage != null)
				{
					var responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
					errorMessage += $": {responseContent}";
				}

				//				throw new ApplicationException(errorMessage, httpRequestException);
				throw new ApplicationException(errorMessage);
			}
			catch (Exception ex)
			{
				throw new ApplicationException("An unexpected error occurred.", ex);
			}
		}

		#endregion

	}
}
