// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.Services.ItemzChangeHistoryByProjectService;
using OpenRose.WebUI.Client.SharedModels;

namespace OpenRose.WebUI.Client.Services.ItemzChangeHistoryByProject
{
    public class DummyItemzChangeHistoryByProjectService : IItemzChangeHistoryByProjectService
    {
        public Task<int> __DELETE_Itemz_Change_History_By_Project_GUID_ID__Async(DeleteChangeHistoryDTO body)
        {
            throw new NotImplementedException();
        }

        public Task<int> __GET_Number_of_ItemzChangeHistory_By_Project_Upto_DateTime__Async(GetNumberOfChangeHistoryDTO body)
        {
            throw new NotImplementedException();
        }

        public Task<int> __GET_Number_of_ItemzChangeHistory_By_Project__Async(Guid projectId)
        {
            throw new NotImplementedException();
        }
    }
}
