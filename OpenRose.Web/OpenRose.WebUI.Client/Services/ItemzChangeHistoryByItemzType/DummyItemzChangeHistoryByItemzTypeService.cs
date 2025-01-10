// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.Services.ItemzChangeHistoryByItemzTypeService;
using OpenRose.WebUI.Client.SharedModels;

namespace OpenRose.WebUI.Client.Services.ItemzChangeHistoryByItemzType
{
    public class DummyItemzChangeHistoryByItemzTypeService : IItemzChangeHistoryByItemzTypeService
    {
        public Task<int> __DELETE_Itemz_Change_History_By_ItemzType_GUID_ID__Async(DeleteChangeHistoryDTO body)
        {
            throw new NotImplementedException();
        }

        public Task<int> __GET_Number_of_ItemzChangeHistory_By_ItemzType_Upto_DateTime__Async(GetNumberOfChangeHistoryDTO body)
        {
            throw new NotImplementedException();
        }

        public Task<int> __GET_Number_of_ItemzChangeHistory_By_ItemzType__Async(Guid itemzTypeId)
        {
            throw new NotImplementedException();
        }
    }
}
