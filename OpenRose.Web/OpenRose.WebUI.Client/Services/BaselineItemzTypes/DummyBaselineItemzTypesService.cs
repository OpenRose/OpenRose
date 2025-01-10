// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.SharedModels;

namespace OpenRose.WebUI.Client.Services.BaselineItemzTypes
{
    public class DummyBaselineItemzTypesService : IBaselineItemzTypesService
    {
        public Task<ICollection<GetBaselineItemzDTO>> __GET_BaselineItemzs_By_BaselineItemzType__Async(Guid baselineItemzTypeId, int? pageNumber, int? pageSize, string orderBy)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<GetBaselineItemzTypeDTO>> __GET_BaselineItemzTypes_Collection__Async()
        {
            throw new NotImplementedException();
        }

        public Task<int> __GET_BaselineItemz_Count_By_BaselineItemzType__Async(Guid baselineItemzTypeId)
        {
            throw new NotImplementedException();
        }

        public Task<GetBaselineItemzTypeDTO> __Single_BaselineItemzType_By_GUID_ID__Async(Guid baselineItemzTypeId)
        {
            throw new NotImplementedException();
        }
    }
}
