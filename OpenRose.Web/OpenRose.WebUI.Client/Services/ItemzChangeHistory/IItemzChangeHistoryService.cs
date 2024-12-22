// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.SharedModels;

namespace OpenRose.WebUI.Client.Services.ItemzChangeHistory
{
    public interface IItemzChangeHistoryService
	{
		public Task<ICollection<GetItemzChangeHistoryDTO>> __GET_ItemzChangeHistory_By_GUID_ItemzID__Async(Guid itemzId);

		public Task<int> __DELETE_ItemzChangeHistory_By_GUID_ID__Async(DeleteChangeHistoryDTO body);

	}
}
