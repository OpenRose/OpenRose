// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.SharedModels;

namespace OpenRose.WebUI.Client.Services.BaselineItemzCollection
{
	public interface IBaselineItemzCollectionService
	{
		//public Task<ICollection<GetBaselineItemzDTO>> __GET_BaselineItemz_Collection_By_GUID_IDS__Async(IEnumerable<System.Guid> baselineItemzids);

		// New: POST-based retrieval that accepts GUID list in JSON body to avoid very long GET URLs
		public Task<ICollection<GetBaselineItemzDTO>> __POST_GET_BaselineItemz_Collection_By_GUID_IDS__Async(IEnumerable<System.Guid> baselineItemzids);
	}
}