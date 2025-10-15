// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.SharedModels;

namespace OpenRose.WebUI.Client.Services.GoTo
{
	public class DummyGoToService : IGoToService
	{
		public Task<GoToResolutionDTO> __Get_GoTo_Details_By_GUID__(Guid recordId)
		{
			throw new NotImplementedException();
		}
	}
}
