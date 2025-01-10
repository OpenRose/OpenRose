// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.SharedModels;

namespace OpenRose.WebUI.Client.Services.Baselines
{
    public class DummyBaselinesService : IBaselinesService
    {
        public Task __DELETE_Baseline_By_GUID_ID__Async(Guid baselineId)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<GetBaselineItemzTypeDTO>> __GET_BaselineItemzTypes_By_Baseline__Async(Guid baselineId)
        {
            throw new NotImplementedException();
        }

        public Task<int> __GET_BaselineItemz_Count_By_Baseline__Async(Guid baselineId)
        {
            throw new NotImplementedException();
        }

        public Task<int> __GET_BaselineItemz_Trace_Count_By_Baseline__Async(Guid baselineId)
        {
            throw new NotImplementedException();
        }

        public Task<int> __GET_Baselines_By_Project_Id__Async(Guid projectId)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<GetBaselineDTO>> __GET_Baselines_Collection__Async()
        {
            throw new NotImplementedException();
        }

        public Task<int> __GET_Baseline_Count_By_Project__Async(Guid projectId)
        {
            throw new NotImplementedException();
        }

        public Task<int> __GET_Excluded_BaselineItemz_Count_By_Baseline__Async(Guid baselineId)
        {
            throw new NotImplementedException();
        }

        public Task<int> __GET_Included_BaselineItemz_Count_By_Baseline__Async(Guid baselineId)
        {
            throw new NotImplementedException();
        }

        public Task<int> __GET_Orphaned_BaselineItemz_Count__Async()
        {
            throw new NotImplementedException();
        }

        public Task<int> __GET_Total_BaselineItemz_Count__Async()
        {
            throw new NotImplementedException();
        }

        public Task<GetBaselineDTO> __POST_Clone_Baseline__Async(CloneBaselineDTO cloneBaselineDTO)
        {
            throw new NotImplementedException();
        }

        public Task<GetBaselineDTO> __POST_Create_Baseline__Async(CreateBaselineDTO createBaselineDTO)
        {
            throw new NotImplementedException();
        }

        public Task __PUT_Update_Baseline_By_GUID_ID__Async(Guid baselineId, UpdateBaselineDTO updateBaselineDTO)
        {
            throw new NotImplementedException();
        }

        public Task<GetBaselineDTO> __Single_Baseline_By_GUID_ID__Async(Guid baselineId)
        {
            throw new NotImplementedException();
        }
    }
}
