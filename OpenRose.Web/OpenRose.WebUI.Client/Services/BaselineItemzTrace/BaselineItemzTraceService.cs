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

namespace OpenRose.WebUI.Client.Services.BaselineItemzTrace
{
	public class BaselineItemzTraceService : IBaselineItemzTraceService
	{
		private readonly HttpClient _httpClient;

		public BaselineItemzTraceService(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		#region __GET_Check_Baseline_Itemz_Trace_Exists__Async
		public async Task<BaselineItemzTraceDTO> __GET_Check_Baseline_Itemz_Trace_Exists__Async(Guid? fromTraceBaselineItemzId, Guid? toTraceBaselineItemzId)
		{
			return await __GET_Check_Baseline_Itemz_Trace_Exists__Async(fromTraceBaselineItemzId, toTraceBaselineItemzId, CancellationToken.None);
		}

		public async Task<BaselineItemzTraceDTO> __GET_Check_Baseline_Itemz_Trace_Exists__Async(Guid? fromTraceBaselineItemzId, Guid? toTraceBaselineItemzId, CancellationToken cancellationToken)
		{
			try
			{

				// TODO :: Utilize urlBuilder which is commented below.

				var urlBuilder_ = new System.Text.StringBuilder();
				urlBuilder_.Append("/api/BaselineItemzTrace/CheckExists");
				urlBuilder_.Append('?');

				if (fromTraceBaselineItemzId != Guid.Empty)
				{
					urlBuilder_.Append(System.Uri.EscapeDataString("fromTraceBaselineItemzId")).Append('=').Append(System.Uri.EscapeDataString(fromTraceBaselineItemzId.ToString()!)).Append('&');
				}
				else
				{
					throw new ArgumentNullException(nameof(fromTraceBaselineItemzId) + "is required for which value is not provided");
				}

				if (toTraceBaselineItemzId != Guid.Empty)
				{
					urlBuilder_.Append(System.Uri.EscapeDataString("toTraceBaselineItemzId")).Append('=').Append(System.Uri.EscapeDataString(toTraceBaselineItemzId.ToString()!)).Append('&');
				}
				else
				{
					throw new ArgumentNullException(nameof(toTraceBaselineItemzId) + "is required for which value is not provided");
				}

				urlBuilder_.Length--;


				var response = await _httpClient.GetFromJsonAsync<BaselineItemzTraceDTO>($"/api/BaselineItemzTrace/CheckExists?fromTraceBaselineItemzId={fromTraceBaselineItemzId.ToString()}&toTraceBaselineItemzId={toTraceBaselineItemzId.ToString()}", cancellationToken);

				return response!;
			}
			catch (Exception)
			{
			}
			return default;
		}
		#endregion

		#region __GET_Baseline_Itemz_Traces_By_BaselineItemzID__Async
		public async Task<ICollection<BaselineItemzTraceDTO>> __GET_Baseline_Itemz_Traces_By_BaselineItemzID__Async(Guid baselineItemzId)
		{
			return await __GET_Baseline_Itemz_Traces_By_BaselineItemzID__Async(baselineItemzId, CancellationToken.None);
		}
		public async Task<ICollection<BaselineItemzTraceDTO>> __GET_Baseline_Itemz_Traces_By_BaselineItemzID__Async(Guid baselineItemzId, CancellationToken cancellationToken)
		{
			try
			{

				if (baselineItemzId == Guid.Empty)
				{
					throw new ArgumentNullException(nameof(baselineItemzId) + "is required for which value is not provided");
				}
				// TODO :: Utilize urlBuilder which is commented below.

				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append($"/api/BaselineItemzTrace/{baselineItemzId.ToString()}");
				//urlBuilder_.Append('?');

				//urlBuilder_.Length--;

				var response = await _httpClient.GetFromJsonAsync<ICollection<BaselineItemzTraceDTO>>($"/api/BaselineItemzTrace/{baselineItemzId.ToString()}", cancellationToken);

				return response!;
			}
			catch (Exception)
			{

			}
			return default;
		}
		#endregion

		#region __GET_All_Parent_and_Child_Baseline_Itemz_Traces_By_BaselineItemzID__Async
		public async Task<BaselineItemzParentAndChildTraceDTO> __GET_All_Parent_and_Child_Baseline_Itemz_Traces_By_BaselineItemzID__Async(Guid baselineItemzId)
		{
			return await __GET_All_Parent_and_Child_Baseline_Itemz_Traces_By_BaselineItemzID__Async(baselineItemzId, CancellationToken.None);
		}

		public async Task<BaselineItemzParentAndChildTraceDTO> __GET_All_Parent_and_Child_Baseline_Itemz_Traces_By_BaselineItemzID__Async(Guid baselineItemzId, CancellationToken cancellationToken)
		{
			try
			{

				if (baselineItemzId == Guid.Empty)
				{
					throw new ArgumentNullException(nameof(baselineItemzId) + "is required for which value is not provided");
				}
				// TODO :: Utilize urlBuilder which is commented below.

				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append($"/api/BaselineItemzTrace/AllBaselineItemzTraces/{baselineItemzId.ToString()}");
				//urlBuilder_.Append('?');

				//urlBuilder_.Length--;

				var response = await _httpClient.GetFromJsonAsync<BaselineItemzParentAndChildTraceDTO>($"/api/BaselineItemzTrace/AllBaselineItemzTraces/{baselineItemzId.ToString()}", cancellationToken);

				return response!;
			}
			catch (Exception)
			{

			}
			return default;
		}
		#endregion

		#region __GET_From_BaselineItemz_Trace_Count_By_BaselineItemzID__Async
		public async Task<int> __GET_From_BaselineItemz_Trace_Count_By_BaselineItemzID__Async(Guid baselineItemzId)
		{
			return await __GET_From_BaselineItemz_Trace_Count_By_BaselineItemzID__Async(baselineItemzId, CancellationToken.None);
		}

		public async Task<int> __GET_From_BaselineItemz_Trace_Count_By_BaselineItemzID__Async(Guid baselineItemzId, CancellationToken cancellationToken)
		{
			try
			{

				if (baselineItemzId == Guid.Empty)
				{
					throw new ArgumentNullException(nameof(baselineItemzId) + "is required for which value is not provided");
				}
				// TODO :: Utilize urlBuilder which is commented below.

				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append($"/api/BaselineItemzTrace/GetFromBaselineItemzTraceCount/{baselineItemzId.ToString()}");
				//urlBuilder_.Append('?');

				//urlBuilder_.Length--;

				var response = await _httpClient.GetFromJsonAsync<int>($"/api/BaselineItemzTrace/GetFromBaselineItemzTraceCount/{baselineItemzId.ToString()}", cancellationToken);

				return response!;
			}
			catch (Exception)
			{

			}
			return default;
		}
		#endregion

		#region __GET_To_BaselineItemz_Trace_Count_By_BaselineItemzID__Async
		public async Task<int> __GET_To_BaselineItemz_Trace_Count_By_BaselineItemzID__Async(Guid baselineItemzId)
		{
			return await __GET_To_BaselineItemz_Trace_Count_By_BaselineItemzID__Async(baselineItemzId, CancellationToken.None);
		}
		public async Task<int> __GET_To_BaselineItemz_Trace_Count_By_BaselineItemzID__Async(Guid baselineItemzId, CancellationToken cancellationToken)
		{
			try
			{

				if (baselineItemzId == Guid.Empty)
				{
					throw new ArgumentNullException(nameof(baselineItemzId) + "is required for which value is not provided");
				}
				// TODO :: Utilize urlBuilder which is commented below.

				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append($"/api/BaselineItemzTrace/GetToBaselineItemzTraceCount/{baselineItemzId.ToString()}");
				//urlBuilder_.Append('?');

				//urlBuilder_.Length--;

				var response = await _httpClient.GetFromJsonAsync<int>($"/api/BaselineItemzTrace/GetToBaselineItemzTraceCount/{baselineItemzId.ToString()}", cancellationToken);

				return response!;
			}
			catch (Exception)
			{

			}
			return default;
		}
		#endregion

		#region __GET_All_From_and_To_Traces_Count_By_BaselineItemzID__Async

		public async Task<int> __GET_All_From_and_To_Traces_Count_By_BaselineItemzID__Async(Guid baselineItemzId)
		{
			return await __GET_All_From_and_To_Traces_Count_By_BaselineItemzID__Async(baselineItemzId, CancellationToken.None);
		}

		public async Task<int> __GET_All_From_and_To_Traces_Count_By_BaselineItemzID__Async(Guid baselineItemzId, CancellationToken cancellationToken)
		{
			try
			{

				if (baselineItemzId == Guid.Empty)
				{
					throw new ArgumentNullException(nameof(baselineItemzId) + "is required for which value is not provided");
				}
				// TODO :: Utilize urlBuilder which is commented below.

				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append($"/api/BaselineItemzTrace/GetAllFromAndToTracesCountByBaselineItemzId/{baselineItemzId.ToString()}");
				//urlBuilder_.Append('?');

				//urlBuilder_.Length--;

				var response = await _httpClient.GetFromJsonAsync<int>($"/api/BaselineItemzTrace/GetAllFromAndToTracesCountByBaselineItemzId/{baselineItemzId.ToString()}", cancellationToken);

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