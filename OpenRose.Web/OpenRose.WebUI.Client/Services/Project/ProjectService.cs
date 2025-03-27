// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.SharedModels;
using OpenRose.WebUI.Client.Utilities;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenRose.WebUI.Client.Services.Project
{
    public class ProjectService : IProjectService
    {
        private readonly HttpClient _httpClient;

        public ProjectService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        #region __GET_Projects__Async

        public async Task<ICollection<GetProjectDTO>?> __GET_Projects__Async()
        {
            return await __GET_Projects__Async(CancellationToken.None);
        }
        public async Task<ICollection<GetProjectDTO>?> __GET_Projects__Async(CancellationToken cancellationToken)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ICollection<GetProjectDTO>>("/api/Projects", cancellationToken);

                return response!;
            }
            catch (Exception)
            {

            }
            return default;
        }

        #endregion

        #region __Single_Project_By_GUID_ID__Async
        public async Task<GetProjectDTO?> __Single_Project_By_GUID_ID__Async(Guid projectId)
        {
            return await __Single_Project_By_GUID_ID__Async(projectId, CancellationToken.None);
        }


        public async Task<GetProjectDTO?> __Single_Project_By_GUID_ID__Async(Guid projectId, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<GetProjectDTO>($"/api/Projects/{projectId}", cancellationToken);

                return response!;
            }
            catch (Exception)
            {
            }
            return default;
        }

        #endregion

        #region __GET_Itemz_Count_By_Project__Async
        public async Task<int?> __GET_Itemz_Count_By_Project__Async(Guid projectId)
        {
            return await __GET_Itemz_Count_By_Project__Async(projectId, CancellationToken.None);
        }


        public async Task<int?> __GET_Itemz_Count_By_Project__Async(Guid projectId, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<int>($"api/Projects/GetItemzCount/{projectId}", cancellationToken);

                return response!;
            }
            catch (Exception)
            {
            }
            return default;
        }

        #endregion

        #region __GET_ItemzTypes_By_Project__Async
        public async Task<ICollection<GetItemzTypeDTO>?>  __GET_ItemzTypes_By_Project__Async(Guid projectId)
        {
            return await __GET_ItemzTypes_By_Project__Async(projectId, CancellationToken.None);
        }


        public async Task<ICollection<GetItemzTypeDTO>?> __GET_ItemzTypes_By_Project__Async(Guid projectId, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ICollection<GetItemzTypeDTO>>($"/api/Projects/GetItemzTypes/{projectId}", cancellationToken);

                return response!;
            }
            catch (Exception)
            {
            }
            return default;
        }

        #endregion

        #region __GET_Last_Project_HierarchyID__Async
        public async Task<string?> __GET_Last_Project_HierarchyID__Async()
        {
            return await __GET_Last_Project_HierarchyID__Async(CancellationToken.None);
        }

        public async Task<string?> __GET_Last_Project_HierarchyID__Async(CancellationToken cancellationToken)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<string>($"/api/Projects/GetLastProjectHierarchyID", cancellationToken);

                return response!;
            }
            catch (Exception)
            {
            }
            return default;
        }

        #endregion

        #region __PUT_Update_Project_By_GUID_ID (NSWAG - IDAsync)
        public async Task<GetProjectDTO?> __PUT_Update_Project_By_GUID_ID__Async(Guid projectId, GetProjectDTO updateProjectDTO)
        {
            try
            {
                return await __PUT_Update_Project_By_GUID_ID__Async(projectId, updateProjectDTO, CancellationToken.None);
            }
            catch (Exception ex) 
            {
				throw new Exception(ex.Message);
			}
        }

        public async Task<GetProjectDTO?> __PUT_Update_Project_By_GUID_ID__Async(Guid projectId, GetProjectDTO updateProjectDTO, CancellationToken cancellationToken)
        {
            try
            {

				if (updateProjectDTO == null || string.IsNullOrWhiteSpace(updateProjectDTO.Name))
				{
					throw new ArgumentNullException("Project Name is a required field for which value was not provided");
				}

				var httpResponseMessage = await _httpClient.PutAsJsonAsync($"/api/Projects/{projectId}", updateProjectDTO, cancellationToken);

				if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.Conflict)
				{
					// Read the response content
					var _errorContent = await httpResponseMessage.Content.ReadAsStringAsync();

					// TODO :: Use MudBlazor Snackbar to show the message (assuming MudBlazor Snackbar is set up)
					// TODO :: Do we need to pass server error message all the way to user UI? We need to check what's included in _errorContent though!
					throw new ApplicationException($"FAILED : {_errorContent}");
				}

				httpResponseMessage.EnsureSuccessStatusCode();

				string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(responseContent))
                {
                    return default;
                }

				// EXPLANATION :: HERE WE ARE SERIALIZING JSON RESPONSE INTO DESIRED CLASS / OBJECT FORMAT FOR RETURNING
				var options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true,
				};
				var response = JsonSerializer.Deserialize<GetProjectDTO>(responseContent, options);
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

        #region __POST_Create_Project__Async
        public async Task<GetProjectDTO?> __POST_Create_Project__Async(GetProjectDTO updateProjectDTO)
        {
            return await __POST_Create_Project__Async(updateProjectDTO, CancellationToken.None);
        }

        public async Task<GetProjectDTO?> __POST_Create_Project__Async(GetProjectDTO updateProjectDTO, CancellationToken cancellationToken)
        {
            try
            {
				if (updateProjectDTO == null || string.IsNullOrWhiteSpace(updateProjectDTO.Name))
				{
					throw new ArgumentNullException("Project Name is a required field for which value was not provided");
				}

				var httpResponseMessage = await _httpClient.PostAsJsonAsync($"/api/Projects", updateProjectDTO, cancellationToken);

				if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.Conflict)
				{
					// Read the response content
					var _errorContent = await httpResponseMessage.Content.ReadAsStringAsync();

					// TODO :: Use MudBlazor Snackbar to show the message (assuming MudBlazor Snackbar is set up)
                    // TODO :: Do we need to pass server error message all the way to user UI? We need to check what's included in _errorContent though!
					throw new ApplicationException($"FAILED : {_errorContent}");
				}

				httpResponseMessage.EnsureSuccessStatusCode();

                string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();

				if (string.IsNullOrWhiteSpace(responseContent))
				{
					return default;
				}

				// EXPLANATION :: HERE WE ARE SERIALIZING JSON RESPONSE INTO DESIRED CLASS / OBJECT FORMAT FOR RETURNING
				var options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true,
				};
				var response = JsonSerializer.Deserialize<GetProjectDTO>(responseContent, options);
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

		#region __DELETE_Project_By_GUID_ID__Async

		public async Task __DELETE_Project_By_GUID_ID__Async(Guid projectId)
        {
            await __DELETE_Project_By_GUID_ID__Async(projectId, CancellationToken.None);
        }

        public async Task __DELETE_Project_By_GUID_ID__Async(Guid projectId, CancellationToken cancellationToken)
        {
            try
            {
                if(projectId == Guid.Empty)
                {
                    throw new ArgumentNullException("Project ID is a required field for which value was not provided");
                }

                var httpResponseMessage = await _httpClient.DeleteAsync($"/api/Projects/{projectId}", cancellationToken);

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

		#region __POST_Copy_Project_By_GUID_ID__Async
		public async Task<GetProjectDTO> __POST_Copy_Project_By_GUID_ID__Async(CopyProjectDTO copyProjectDTO)
		{
			return await __POST_Copy_Project_By_GUID_ID__Async(copyProjectDTO, CancellationToken.None);
		}

		public async Task<GetProjectDTO> __POST_Copy_Project_By_GUID_ID__Async(CopyProjectDTO copyProjectDTO, CancellationToken cancellationToken)
		{
			try
			{

				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append("/api/Projects/CopyProject");
				//urlBuilder_.Append('?');

				//urlBuilder_.Length--;


				if (copyProjectDTO == null || copyProjectDTO.ProjectId == Guid.Empty)
				{
					throw new ArgumentNullException("Project Id is required field but no value was provided for the same");
				}

				var httpResponseMessage = await _httpClient.PostAsJsonAsync($"/api/Projects/CopyProject", copyProjectDTO, cancellationToken);

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


				if (string.IsNullOrWhiteSpace(responseContent))
				{
					return default;
				}

				// EXPLANATION :: HERE WE ARE SERIALIZING JSON RESPONSE INTO DESIRED CLASS / OBJECT FORMAT FOR RETURNING
				var options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true,
				};
				var response = JsonSerializer.Deserialize<GetProjectDTO>(responseContent, options);
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



		///// <summary>
		///// Gets collection of Projects
		///// </summary>
		///// <returns>Returns collection of Projects based on sorting order</returns>
		///// <exception cref="ApiException">A server side error occurred.</exception>
		//public virtual System.Threading.Tasks.Task<System.Collections.Generic.ICollection<GetProjectDTO>> __GET_Projects__Async()
		//{
		//    return __GET_Projects__Async(System.Threading.CancellationToken.None);
		//}

		///// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		///// <summary>
		///// Gets collection of Projects
		///// </summary>
		///// <returns>Returns collection of Projects based on sorting order</returns>
		///// <exception cref="ApiException">A server side error occurred.</exception>
		//public virtual async System.Threading.Tasks.Task<System.Collections.Generic.ICollection<GetProjectDTO>> __GET_Projects__Async(System.Threading.CancellationToken cancellationToken)
		//{
		//    var client_ = _httpClient;
		//    var disposeClient_ = false;
		//    try
		//    {
		//        using (var request_ = new System.Net.Http.HttpRequestMessage())
		//        {
		//            request_.Method = new System.Net.Http.HttpMethod("GET");
		//            request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));

		//            var urlBuilder_ = new System.Text.StringBuilder();
		//            if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
		//            // Operation Path: "api/Projects"
		//            urlBuilder_.Append("api/Projects");

		//            PrepareRequest(client_, request_, urlBuilder_);

		//            var url_ = urlBuilder_.ToString();
		//            request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

		//            PrepareRequest(client_, request_, url_);

		//            var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
		//            var disposeResponse_ = true;
		//            try
		//            {
		//                var headers_ = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.IEnumerable<string>>();
		//                foreach (var item_ in response_.Headers)
		//                    headers_[item_.Key] = item_.Value;
		//                if (response_.Content != null && response_.Content.Headers != null)
		//                {
		//                    foreach (var item_ in response_.Content.Headers)
		//                        headers_[item_.Key] = item_.Value;
		//                }

		//                ProcessResponse(client_, response_);

		//                var status_ = (int)response_.StatusCode;
		//                if (status_ == 400)
		//                {
		//                    var objectResponse_ = await ReadObjectResponseAsync<ProblemDetails>(response_, headers_, cancellationToken).ConfigureAwait(false);
		//                    if (objectResponse_.Object == null)
		//                    {
		//                        throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
		//                    }
		//                    throw new ApiException<ProblemDetails>("Bad Request", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
		//                }
		//                else
		//                if (status_ == 406)
		//                {
		//                    var objectResponse_ = await ReadObjectResponseAsync<ProblemDetails>(response_, headers_, cancellationToken).ConfigureAwait(false);
		//                    if (objectResponse_.Object == null)
		//                    {
		//                        throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
		//                    }
		//                    throw new ApiException<ProblemDetails>("Not Acceptable", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
		//                }
		//                else
		//                if (status_ == 500)
		//                {
		//                    string responseText_ = (response_.Content == null) ? string.Empty : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
		//                    throw new ApiException("Internal Server Error", status_, responseText_, headers_, null);
		//                }
		//                else
		//                if (status_ == 200)
		//                {
		//                    var objectResponse_ = await ReadObjectResponseAsync<System.Collections.Generic.ICollection<GetProjectDTO>>(response_, headers_, cancellationToken).ConfigureAwait(false);
		//                    if (objectResponse_.Object == null)
		//                    {
		//                        throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
		//                    }
		//                    return objectResponse_.Object;
		//                }
		//                else
		//                if (status_ == 404)
		//                {
		//                    var objectResponse_ = await ReadObjectResponseAsync<ProblemDetails>(response_, headers_, cancellationToken).ConfigureAwait(false);
		//                    if (objectResponse_.Object == null)
		//                    {
		//                        throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
		//                    }
		//                    throw new ApiException<ProblemDetails>("No Projects were found", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
		//                }
		//                else
		//                {
		//                    var objectResponse_ = await ReadObjectResponseAsync<ProblemDetails>(response_, headers_, cancellationToken).ConfigureAwait(false);
		//                    if (objectResponse_.Object == null)
		//                    {
		//                        throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
		//                    }
		//                    throw new ApiException<ProblemDetails>("Error", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
		//                }
		//            }
		//            finally
		//            {
		//                if (disposeResponse_)
		//                    response_.Dispose();
		//            }
		//        }
		//    }
		//    finally
		//    {
		//        if (disposeClient_)
		//            client_.Dispose();
		//    }
		//}
	}
}
