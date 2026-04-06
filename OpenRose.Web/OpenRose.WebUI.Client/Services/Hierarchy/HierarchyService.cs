// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.Services.Hierarchy;
using OpenRose.WebUI.Client.SharedModels;
using OpenRose.WebUI.Client.Utilities;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenRose.WebUI.Client.Services.Hierarchy
{
    public class HierarchyService : IHierarchyService
	{
        private readonly HttpClient _httpClient;

        public HierarchyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

		#region __Get_Hierarchy_Record_Details_By_GUID__Async

		public async Task<HierarchyIdRecordDetailsDTO> __Get_Hierarchy_Record_Details_By_GUID__Async(Guid recordId)
        {
            return await __Get_Hierarchy_Record_Details_By_GUID__Async(recordId, CancellationToken.None);
		}
		public async Task<HierarchyIdRecordDetailsDTO> __Get_Hierarchy_Record_Details_By_GUID__Async(Guid recordId, CancellationToken cancellationToken)
		{
			try
			{
				//TODO::Utilize urlBuilder which is commented below.

				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append($"/api/Hierarchy/{recordId.ToString()}");
				//urlBuilder_.Append('?');

				//urlBuilder_.Length--;

				var httpResponseMessage = await _httpClient.GetFromJsonAsync<HierarchyIdRecordDetailsDTO>($"/api/Hierarchy/{recordId.ToString()}", cancellationToken);
				return httpResponseMessage!;
			}
			catch (Exception)
			{
			}
			return default;
		}

		#endregion

		#region __Get_Next_Sibling_Hierarchy_Record_Details_By_GUID__Async

		public async Task<HierarchyIdRecordDetailsDTO> __Get_Next_Sibling_Hierarchy_Record_Details_By_GUID__Async(Guid recordId)
		{
			return await __Get_Next_Sibling_Hierarchy_Record_Details_By_GUID__Async(recordId, CancellationToken.None);
		}
		public async Task<HierarchyIdRecordDetailsDTO> __Get_Next_Sibling_Hierarchy_Record_Details_By_GUID__Async(Guid recordId, CancellationToken cancellationToken)
		{
			try
			{
				//TODO::Utilize urlBuilder which is commented below.

				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append($"/api/Hierarchy/{recordId.ToString()}");
				//urlBuilder_.Append('?');

				//urlBuilder_.Length--;

				var httpResponseMessage = await _httpClient.GetFromJsonAsync<HierarchyIdRecordDetailsDTO>($"/api/Hierarchy/GetNextSibling/{recordId.ToString()}", cancellationToken);
				return httpResponseMessage!;
			}
			catch (Exception)
			{
                throw new NotImplementedException();
            }
			return default;
		}

		#endregion

		#region __Get_Immediate_Children_Hierarchy_By_GUID__Async
		public async Task<ICollection<HierarchyIdRecordDetailsDTO>> __Get_Immediate_Children_Hierarchy_By_GUID__Async(Guid recordId)
		{
			return await __Get_Immediate_Children_Hierarchy_By_GUID__Async(recordId, CancellationToken.None);
		}

		public async Task<ICollection<HierarchyIdRecordDetailsDTO>> __Get_Immediate_Children_Hierarchy_By_GUID__Async(Guid recordId, CancellationToken cancellationToken)
		{

			try
			{
				var response = await _httpClient.GetFromJsonAsync<ICollection<HierarchyIdRecordDetailsDTO>>($"/api/Hierarchy/GetImmediateChildren/{recordId}", cancellationToken);

				return response!;
			}
			catch (Exception)
			{

			}
			return default;

		}
		#endregion

		#region __Get_All_Parents_Hierarchy_By_GUID__Async
		public async Task<ICollection<NestedHierarchyIdRecordDetailsDTO>>
			__Get_All_Parents_Hierarchy_By_GUID__Async(Guid recordId)
		{
			return await __Get_All_Parents_Hierarchy_By_GUID__Async(recordId, CancellationToken.None);
		}

		public async Task<ICollection<NestedHierarchyIdRecordDetailsDTO>>
			__Get_All_Parents_Hierarchy_By_GUID__Async(Guid recordId, CancellationToken cancellationToken)
		{
			try
			{
				var response = await _httpClient.GetFromJsonAsync<ICollection<NestedHierarchyIdRecordDetailsDTO>>(
					$"/api/Hierarchy/GetAllParents/{recordId}", cancellationToken);

				// If API returns null, treat it as "no parents found"
				return response ?? new List<NestedHierarchyIdRecordDetailsDTO>();
			}
			catch (Exception)
			{
				// Never throw here — return empty list so caller can handle "not found"
				return new List<NestedHierarchyIdRecordDetailsDTO>();
			}
		}

		#endregion

		#region __Get_All_Children_Hierarchy_By_GUID__Async
		public async Task<ICollection<NestedHierarchyIdRecordDetailsDTO>> __Get_All_Children_Hierarchy_By_GUID__Async(Guid recordId)
		{
			return await __Get_All_Children_Hierarchy_By_GUID__Async(recordId, CancellationToken.None);
		}

		public async Task<ICollection<NestedHierarchyIdRecordDetailsDTO>> __Get_All_Children_Hierarchy_By_GUID__Async(Guid recordId, CancellationToken cancellationToken)
		{

			try
			{
				var response = await _httpClient.GetFromJsonAsync<ICollection<NestedHierarchyIdRecordDetailsDTO>>($"/api/Hierarchy/GetAllChildren/{recordId}", cancellationToken);

				return response!;
			}
			catch (Exception)
			{
                throw new NotImplementedException();
            }
            return default;

		}
        #endregion

        #region __Get_All_Children_Hierarchy_Count_By_GUID__Async
        public async Task<int> __Get_All_Children_Hierarchy_Count_By_GUID__Async(Guid recordId)
        {
            return await __Get_All_Children_Hierarchy_Count_By_GUID__Async(recordId, CancellationToken.None);
        }

        public async Task<int> __Get_All_Children_Hierarchy_Count_By_GUID__Async(Guid recordId, CancellationToken cancellationToken)
        {

            try
            {
                var response = await _httpClient.GetFromJsonAsync<int>($"/api/Hierarchy/GetAllChildrenCount/{recordId}", cancellationToken);

                return response!;
            }
            catch (Exception)
            {
				throw new NotImplementedException();
            }
            return default;

        }
		#endregion

		#region __Update_Hierarchy_Estimation_Async__

		public async Task<bool> __Update_Hierarchy_Estimation_Async__(Guid recordId, UpdateHierarchyEstimationDTO updateEstimationDTO)
		{
			return await __Update_Hierarchy_Estimation_Async__(recordId, updateEstimationDTO, CancellationToken.None);
		}

		public async Task<bool> __Update_Hierarchy_Estimation_Async__(Guid recordId, UpdateHierarchyEstimationDTO updateEstimationDTO, CancellationToken cancellationToken)
		{
			try
			{
				if (recordId == Guid.Empty)
				{
					throw new ArgumentNullException(nameof(recordId), "Record ID is required");
				}

				if (updateEstimationDTO == null)
				{
					throw new ArgumentNullException(nameof(updateEstimationDTO), "Update estimation DTO is required");
				}

				var options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true,
				};

				var json_ = JsonSerializer.Serialize<UpdateHierarchyEstimationDTO>(updateEstimationDTO, options);

				var httpResponseMessage = await _httpClient.PutAsync(
					$"/api/Hierarchy/UpdateEstimation",
					new StringContent(json_, Encoding.UTF8, "application/json"),
					cancellationToken);

				if (HttpStatusCodesHelper.ErrorStatusCodes.Contains(httpResponseMessage.StatusCode))
				{
					var _errorContent = await httpResponseMessage.Content.ReadAsStringAsync();
					throw new ApplicationException($"FAILED: {_errorContent}");
				}

				httpResponseMessage.EnsureSuccessStatusCode();
				return true;
			}
			catch (Exception ex)
			{
				// Log or handle exception as needed
				throw;
			}
		}

		#endregion

		#region __Get_Hierarchy_Record_With_Estimations_Async__

		public async Task<HierarchyIdRecordDetailsDTO> __Get_Hierarchy_Record_With_Estimations_Async__(Guid recordId)
		{
			return await __Get_Hierarchy_Record_With_Estimations_Async__(recordId, CancellationToken.None);
		}

		public async Task<HierarchyIdRecordDetailsDTO> __Get_Hierarchy_Record_With_Estimations_Async__(Guid recordId, CancellationToken cancellationToken)
		{
			try
			{
				if (recordId == Guid.Empty)
				{
					throw new ArgumentNullException(nameof(recordId), "Record ID is required");
				}

				var httpResponseMessage = await _httpClient.GetFromJsonAsync<HierarchyIdRecordDetailsDTO>(
					$"/api/Hierarchy/{recordId}",
					cancellationToken);

				return httpResponseMessage!;
			}
			catch (Exception)
			{
				// Log exception if needed
			}

			return default;
		}

		#endregion

	}
}
