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

namespace OpenRose.WebUI.Client.Services.BaselineItemzTypes
{
	public class BaselineItemzTypesService : IBaselineItemzTypesService
	{
		private readonly HttpClient _httpClient;

		public BaselineItemzTypesService(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		#region __Single_BaselineItemzType_By_GUID_ID__Async
		public async Task<GetBaselineItemzTypeDTO> __Single_BaselineItemzType_By_GUID_ID__Async(Guid baselineItemzTypeId)
		{
			return await __Single_BaselineItemzType_By_GUID_ID__Async(baselineItemzTypeId, CancellationToken.None);
		}

		public async Task<GetBaselineItemzTypeDTO> __Single_BaselineItemzType_By_GUID_ID__Async(Guid baselineItemzTypeId, CancellationToken cancellationToken)
		{
			try
			{
				if (baselineItemzTypeId == Guid.Empty)
				{
					throw new ArgumentNullException(nameof(baselineItemzTypeId) + "is required for which value is not provided");
				}

				// TODO::Utilize urlBuilder which is commented below.
				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append($"/api/BaselineItemzTypes/{baselineItemzTypeId.ToString()}");
				//urlBuilder_.Append('?');

				//urlBuilder_.Length--;

				var response = await _httpClient.GetFromJsonAsync<GetBaselineItemzTypeDTO>($"/api/BaselineItemzTypes/{baselineItemzTypeId.ToString()}", cancellationToken);

				return response!;
			}
			catch (Exception)
			{

			}
			return default;
		}
		#endregion

		#region __GET_BaselineItemzTypes_Collection__Async
		public async Task<ICollection<GetBaselineItemzTypeDTO>> __GET_BaselineItemzTypes_Collection__Async()
		{
			return await __GET_BaselineItemzTypes_Collection__Async(CancellationToken.None);
		}

		public async Task<ICollection<GetBaselineItemzTypeDTO>> __GET_BaselineItemzTypes_Collection__Async(CancellationToken cancellationToken)
		{
			try
			{
				// TODO::Utilize urlBuilder which is commented below.
				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append($"/api/BaselineItemzTypes");
				//urlBuilder_.Append('?');

				//urlBuilder_.Length--;

				var response = await _httpClient.GetFromJsonAsync<ICollection<GetBaselineItemzTypeDTO>>($"/api/BaselineItemzTypes", cancellationToken);

				return response!;
			}
			catch (Exception)
			{

			}
			return default;
		}
		#endregion

		#region __GET_BaselineItemz_Count_By_BaselineItemzType__Async
		public async Task<int> __GET_BaselineItemz_Count_By_BaselineItemzType__Async(Guid baselineItemzTypeId)
		{
			return await __GET_BaselineItemz_Count_By_BaselineItemzType__Async(baselineItemzTypeId, CancellationToken.None);
		}

		public async Task<int> __GET_BaselineItemz_Count_By_BaselineItemzType__Async(Guid baselineItemzTypeId, CancellationToken cancellationToken)
		{
			try
			{
				if (baselineItemzTypeId == Guid.Empty)
				{
					throw new ArgumentNullException(nameof(baselineItemzTypeId) + "is required for which value is not provided");
				}

				// TODO :: Utilize urlBuilder which is commented below.
				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append($"/api/BaselineItemzTypes/GetBaselineItemzCount/{baselineItemzTypeId.ToString()}");
				//urlBuilder_.Append('?');

				//urlBuilder_.Length--;

				var response = await _httpClient.GetFromJsonAsync<int>($"/api/BaselineItemzTypes/GetBaselineItemzCount/{baselineItemzTypeId.ToString()}", cancellationToken);

				return response!;
			}
			catch (Exception)
			{

			}
			return default;
		}
		#endregion

		#region __GET_BaselineItemzs_By_BaselineItemzType__Async
		public async Task<ICollection<GetBaselineItemzDTO>> __GET_BaselineItemzs_By_BaselineItemzType__Async(Guid baselineItemzTypeId, int? pageNumber, int? pageSize, string orderBy)
		{
			return await __GET_BaselineItemzs_By_BaselineItemzType__Async(baselineItemzTypeId, pageNumber, pageSize, orderBy, CancellationToken.None);
		}

		public async Task<ICollection<GetBaselineItemzDTO>> __GET_BaselineItemzs_By_BaselineItemzType__Async(Guid baselineItemzTypeId, int? pageNumber, int? pageSize, string orderBy, CancellationToken cancellationToken)
		{
			try
			{

				// TODO :: Utilize urlBuilder which is commented below.

				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append($"/api/BaselineItemzTypes/GetBaselineItemzByBaselineItemzType/{baselineItemzTypeId.ToString()}");
				//urlBuilder_.Append('?');
				//if (pageNumber != null)
				//{
				//	urlBuilder_.Append(System.Uri.EscapeDataString("pageNumber")).Append('=').Append(System.Uri.EscapeDataString(pageNumber.ToString()!)).Append('&');
				//}
				//if (pageSize != null)
				//{
				//	urlBuilder_.Append(System.Uri.EscapeDataString("pageSize")).Append('=').Append(System.Uri.EscapeDataString(pageSize.ToString()!)).Append('&');
				//}

				//if (orderBy != null)
				//{
				//	urlBuilder_.Append(System.Uri.EscapeDataString("orderBy")).Append('=').Append(System.Uri.EscapeDataString(orderBy)).Append('&');
				//}

				//urlBuilder_.Length--;

				var response = await _httpClient.GetFromJsonAsync<ICollection<GetBaselineItemzDTO>>($"/api/BaselineItemzTypes/GetBaselineItemzByBaselineItemzType/{baselineItemzTypeId.ToString()}", cancellationToken);

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