// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.SharedModels;

namespace OpenRose.WebUI.Client.Services.Hierarchy
{
    public class DummyHierarchyService : IHierarchyService
    {
        public Task<ICollection<NestedHierarchyIdRecordDetailsDTO>> __Get_All_Children_Hierarchy_By_GUID__Async(Guid recordId)
        {
            throw new NotImplementedException();
        }

        public Task<int> __Get_All_Children_Hierarchy_Count_By_GUID__Async(Guid recordId)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<NestedHierarchyIdRecordDetailsDTO>> __Get_All_Parents_Hierarchy_By_GUID__Async(Guid recordId)
        {
            throw new NotImplementedException();
        }

        public Task<HierarchyIdRecordDetailsDTO> __Get_Hierarchy_Record_Details_By_GUID__Async(Guid recordId)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<HierarchyIdRecordDetailsDTO>> __Get_Immediate_Children_Hierarchy_By_GUID__Async(Guid recordId)
        {
            throw new NotImplementedException();
        }

        public Task<HierarchyIdRecordDetailsDTO> __Get_Next_Sibling_Hierarchy_Record_Details_By_GUID__Async(Guid recordId)
        {
            throw new NotImplementedException();
        }
    }
}
