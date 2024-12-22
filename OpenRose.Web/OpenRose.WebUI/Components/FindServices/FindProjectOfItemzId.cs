// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.Services.BaselineHierarchy;
using OpenRose.WebUI.Client.Services.Hierarchy;
using OpenRose.WebUI.Client.SharedModels;

namespace OpenRose.WebUI.Components.FindServices
{
    public class FindProjectOfItemzId
    {
        private readonly IHierarchyService hierarchyService;

        public FindProjectOfItemzId(IHierarchyService hierarchyService)
        {
            this.hierarchyService = hierarchyService;
        }

        public async Task<Guid> getProjectIdOfItemzId (Guid itemzId)
        {

        List<NestedHierarchyIdRecordDetailsDTO> AllParentHierarchy = new List<NestedHierarchyIdRecordDetailsDTO>();

        var returnedParentHierarchyList = await hierarchyService.__Get_All_Parents_Hierarchy_By_GUID__Async(itemzId);

        if (returnedParentHierarchyList != null)
        {
            AllParentHierarchy = returnedParentHierarchyList.ToList();
        }

        var matchingProjectRecord = FindRecordUsingLambda(hierarchy: AllParentHierarchy, recordType: "Project", level: 1);

        if (matchingProjectRecord != null)
        {
            return matchingProjectRecord.RecordId;
        }
        return Guid.Empty;

        }

        #region Finding_Record_In_Parent_Hierarchy_Nodes

        public NestedHierarchyIdRecordDetailsDTO? FindRecordUsingLambda(List<NestedHierarchyIdRecordDetailsDTO> hierarchy, string recordType, int level)
        {
            return hierarchy
                .SelectMany(parent => GetAllRecords(parent))
                .FirstOrDefault(record => record.RecordType == recordType && record.Level == level);
        }

        private IEnumerable<NestedHierarchyIdRecordDetailsDTO> GetAllRecords(NestedHierarchyIdRecordDetailsDTO parent)
        {
            yield return parent;
            if (parent.Children != null)
            {
                foreach (var child in parent.Children.SelectMany(GetAllRecords))
                {
                    yield return child;
                }
            }
        }
        #endregion
    }
}
