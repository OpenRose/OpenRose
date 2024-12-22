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

namespace OpenRose.WebUI.Client.Services.BaselineItemzCollection
{
	public class BaselineItemzCollectionService : IBaselineItemzCollectionService
	{
		private readonly HttpClient _httpClient;

		public BaselineItemzCollectionService(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		#region __GET_BaselineItemz_Collection_By_GUID_IDS__Async
		public async Task<ICollection<GetBaselineItemzDTO>> __GET_BaselineItemz_Collection_By_GUID_IDS__Async(IEnumerable<Guid> baselineItemzids)
		{
			return await __GET_BaselineItemz_Collection_By_GUID_IDS__Async(baselineItemzids, CancellationToken.None);
		}
		public async Task<ICollection<GetBaselineItemzDTO>> __GET_BaselineItemz_Collection_By_GUID_IDS__Async(IEnumerable<Guid> baselineItemzids, CancellationToken cancellationToken)
		{
			try
			{
				// TODO :: Utilize urlBuilder which is commented below.

				if (!baselineItemzids?.Any() ?? true)
				{
					throw new ArgumentNullException(nameof(baselineItemzids) + "is required for which value is not provided");
				}
				var urlBuilder_ = new System.Text.StringBuilder();
				urlBuilder_.Append("/api/BaselineItemzCollection/(");
				// urlBuilder_.Append('(');
				for (var i = 0; i < baselineItemzids!.Count(); i++)
				{
					if (i > 0) urlBuilder_.Append(',');
					urlBuilder_.Append((baselineItemzids!.ElementAt(i).ToString()));
				}
				urlBuilder_.Append(')');


				var response = await _httpClient.GetFromJsonAsync<IEnumerable<GetBaselineItemzDTO>>($"{urlBuilder_}", cancellationToken);

				return response!.ToList();
			}
			catch (Exception)
			{
			}
			return default;

		}

		#endregion

	}
}