// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.SharedModels;

namespace OpenRose.WebUI.Client.Services.BaselineItemz
{
    public class DummyBaselineItemzService : IBaselineItemzService
    {
        public Task<ICollection<GetBaselineItemzDTO>> __GET_BaselineItemzs_By_Itemz__Async(Guid itemzId)
        {
            throw new NotImplementedException();
        }

        public Task<int> __GET_BaselineItemz_Count_By_ItemzId__Async(Guid itemzId)
        {
            throw new NotImplementedException();
        }

        public Task __PUT_Update_BaselineItemzs_By_GUID_IDs__Async(UpdateBaselineItemzDTO body)
        {
            throw new NotImplementedException();
        }

        public Task<GetBaselineItemzDTO> __Single_BaselineItemz_By_GUID_ID__Async(Guid baselineItemzId)
        {
            throw new NotImplementedException();
        }
    }
}
