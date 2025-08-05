// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.


using Microsoft.AspNetCore.Components.Forms;
using OpenRose.WebUI.Client.SharedModels;

namespace OpenRose.WebUI.Client.Services.Import
{
	public class DummyImportService : IImportService
	{
		public Task<HttpResponseMessage> ImportHierarchyAsync(IBrowserFile file, ImportFormClientDTO formDto, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}
	}
}
