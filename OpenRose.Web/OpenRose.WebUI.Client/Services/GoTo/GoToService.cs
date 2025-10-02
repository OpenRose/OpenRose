// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.SharedModels;
using System.Net.Http;
using System.Net.Http.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenRose.WebUI.Client.Services.GoTo
{
	public class GoToService : IGoToService
	{
		private readonly HttpClient _httpClient;

		public GoToService(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}


		#region __Get_GoTo_Details_By_GUID__


		public async Task<GoToResolutionDTO> __Get_GoTo_Details_By_GUID__(Guid recordId)
		{
			return await __Get_GoTo_Details_By_GUID__(recordId, CancellationToken.None);
		}
		public async Task<GoToResolutionDTO> __Get_GoTo_Details_By_GUID__(Guid recordId, CancellationToken cancellationToken)
		{
			try
			{

				//TODO::Utilize urlBuilder which is commented below.

				//var urlBuilder_ = new System.Text.StringBuilder();
				//urlBuilder_.Append($"/api/GoTo/{recordId.ToString()}");
				//urlBuilder_.Append('?');

				//urlBuilder_.Length--;

				var httpResponseMessage = await _httpClient.GetFromJsonAsync<GoToResolutionDTO>($"/api/GoTo/{recordId}", cancellationToken);
				return httpResponseMessage!;
			}
			catch (Exception)
			{
			}
			return default;
		}

		#endregion
	}
}