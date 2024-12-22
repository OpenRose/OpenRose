// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.Services.BaselineHierarchy;
using OpenRose.WebUI.Client.SharedModels;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace OpenRose.WebUI.Client.Services.BaselineHierarchy
{
    public class BaselineHierarchyService : IBaselineHierarchyService
	{
        private readonly HttpClient _httpClient;

        public BaselineHierarchyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

		#region __Get_BaselineHierarchy_Record_Details_By_GUID__Async
		public async Task<BaselineHierarchyIdRecordDetailsDTO> __Get_BaselineHierarchy_Record_Details_By_GUID__Async(Guid recordId)
		{
			return await __Get_BaselineHierarchy_Record_Details_By_GUID__Async(recordId, CancellationToken.None);
		}

		public async Task<BaselineHierarchyIdRecordDetailsDTO> __Get_BaselineHierarchy_Record_Details_By_GUID__Async(Guid recordId, CancellationToken cancellationToken)
		{
			try
			{
				if (recordId == Guid.Empty)
				{
					throw new ArgumentNullException(nameof(recordId) + "is required for which value is not provided");
				}

				//TODO::Utilize urlBuilder which is commented below.

				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append($"/api/BaselineHierarchy/{recordId.ToString()}");
				//urlBuilder_.Append('?');

				//urlBuilder_.Length--;

				var httpResponseMessage = await _httpClient.GetFromJsonAsync<BaselineHierarchyIdRecordDetailsDTO>($"/api/BaselineHierarchy/{recordId.ToString()}", cancellationToken);
				return httpResponseMessage!;
			}
			catch (Exception)
			{
                throw new NotImplementedException();
            }
            return default;
		}
		#endregion

		#region __Get_VerifyParentChild_BreakdownStructure__Async
		public async Task<bool> __Get_VerifyParentChild_BreakdownStructure__Async(Guid? parentId, Guid? childId)
		{
			return await __Get_VerifyParentChild_BreakdownStructure__Async(parentId, childId, CancellationToken.None);
		}
		public async Task<bool> __Get_VerifyParentChild_BreakdownStructure__Async(Guid? parentId, Guid? childId, CancellationToken cancellationToken)
		{
			try
			{
				//TODO::Utilize urlBuilder which is commented below.

				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append($"/api/BaselineHierarchy/VerifyParentChildBreakdownStructure");
				//urlBuilder_.Append('?');

				//if (parentId != Guid.Empty)
				//{
				//	urlBuilder_.Append(System.Uri.EscapeDataString("parentId")).Append('=').Append(System.Uri.EscapeDataString(parentId.ToString()!)).Append('&');
				//}
				//else
				//{
				//	throw new ArgumentNullException(nameof(parentId) + "is required for which value is not provided");
				//}

				//if (childId != Guid.Empty)
				//{
				//	urlBuilder_.Append(System.Uri.EscapeDataString("childId")).Append('=').Append(System.Uri.EscapeDataString(childId.ToString()!)).Append('&');
				//}
				//else
				//{
				//	throw new ArgumentNullException(nameof(childId) + "is required for which value is not provided");
				//}

				//urlBuilder_.Length--;

				var httpResponseMessage = await _httpClient.GetFromJsonAsync<bool>($"/api/BaselineHierarchy/VerifyParentChildBreakdownStructure?parentId={parentId.ToString()}&childId={childId.ToString()}", cancellationToken);
				return httpResponseMessage!;
			}
			catch (Exception)
			{
			}
			return default;

		}
		#endregion

		#region __Get_Immediate_Children_Baseline_Hierarchy_By_GUID__Async
		public async Task<ICollection<BaselineHierarchyIdRecordDetailsDTO>> __Get_Immediate_Children_Baseline_Hierarchy_By_GUID__Async(Guid recordId)
		{
			return await __Get_Immediate_Children_Baseline_Hierarchy_By_GUID__Async(recordId, CancellationToken.None);
		}

		public async Task<ICollection<BaselineHierarchyIdRecordDetailsDTO>> __Get_Immediate_Children_Baseline_Hierarchy_By_GUID__Async(Guid recordId, CancellationToken cancellationToken)
		{
			try
			{
				var response = await _httpClient.GetFromJsonAsync<ICollection<BaselineHierarchyIdRecordDetailsDTO>>($"/api/BaselineHierarchy/GetImmediateChildren/{recordId}", cancellationToken);

				//if (response != null && response.Any())
				//{
				//	return response;
				//}
				//else
				//{
				//	// Handle the case where the response is null or empty 
				//	// You could log this scenario or display an appropriate message to the user
				//	throw new Exception("No data found.");
				//}
				return response;
			}

			catch (HttpRequestException httpEx)
			{ 
				// Handle HTTP-specific exceptions (e.g., 404, 500) 
				// You could log this exception or display an appropriate message to the user
				throw new Exception($"HTTP error occurred: {httpEx.Message}"); 
			} 
			catch (Exception ex)
			{
				// Handle other exceptions
				// You could log this exception or display an appropriate message to the user 
				throw new Exception($"An error occurred: {ex.Message}");
			
			}
			return default;

		}
        #endregion


        #region __Get_All_Children_Baseline_Hierarchy_By_GUID__Async
        public async Task<ICollection<NestedBaselineHierarchyIdRecordDetailsDTO>> __Get_All_Children_Baseline_Hierarchy_By_GUID__Async(Guid recordId)
        {
            return await __Get_All_Children_Baseline_Hierarchy_By_GUID__Async(recordId, CancellationToken.None);
        }

        public async Task<ICollection<NestedBaselineHierarchyIdRecordDetailsDTO>> __Get_All_Children_Baseline_Hierarchy_By_GUID__Async(Guid recordId, CancellationToken cancellationToken)
        {

            try
            {
                var response = await _httpClient.GetFromJsonAsync<ICollection<NestedBaselineHierarchyIdRecordDetailsDTO>>($"/api/BaselineHierarchy/GetAllChildren/{recordId}", cancellationToken);

                return response!;
            }
            catch (Exception)
            {
                throw new NotImplementedException();
            }
            return default;

        }
        #endregion

        #region __Get_All_Children_Baseline_Hierarchy_Count_By_GUID__Async
        public async Task<int> __Get_All_Children_Baseline_Hierarchy_Count_By_GUID__Async(Guid recordId)
        {
            return await __Get_All_Children_Baseline_Hierarchy_Count_By_GUID__Async(recordId, CancellationToken.None);
        }

        public async Task<int> __Get_All_Children_Baseline_Hierarchy_Count_By_GUID__Async(Guid recordId, CancellationToken cancellationToken)
        {

            try
            {
                var response = await _httpClient.GetFromJsonAsync<int>($"/api/BaselineHierarchy/GetAllChildrenCount/{recordId}", cancellationToken);

                return response!;
            }
            catch (Exception)
            {
				throw new NotImplementedException();
            }
            return default;

        }
        #endregion

        #region __Get_All_Parents_Baseline_Hierarchy_By_GUID__
        public async Task<ICollection<NestedBaselineHierarchyIdRecordDetailsDTO>> __Get_All_Parents_Baseline_Hierarchy_By_GUID__Async(Guid recordId)
		{
			return await __Get_All_Parents_Baseline_Hierarchy_By_GUID__Async(recordId, CancellationToken.None);
		}

		public async Task<ICollection<NestedBaselineHierarchyIdRecordDetailsDTO>> __Get_All_Parents_Baseline_Hierarchy_By_GUID__Async(Guid recordId, CancellationToken cancellationToken)
		{

			try
			{
				var response = await _httpClient.GetFromJsonAsync<ICollection<NestedBaselineHierarchyIdRecordDetailsDTO>>($"/api/BaselineHierarchy/GetAllParents/{recordId}", cancellationToken);

				return response!;
			}
			catch (Exception)
			{
                throw new NotImplementedException();
            }
            return default;

		}
		#endregion
	}
}
