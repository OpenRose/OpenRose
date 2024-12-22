// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.Services.BaselineHierarchy;
using OpenRose.WebUI.Client.SharedModels;

namespace OpenRose.WebUI.Components.FindServices
{
    public class FindProjectAndBaselineIdsByBaselineItemzIdService : IFindProjectAndBaselineIdsByBaselineItemzIdService
    {
        private readonly IBaselineHierarchyService baselineHierarchyService;

        public FindProjectAndBaselineIdsByBaselineItemzIdService(IBaselineHierarchyService baselineHierarchyService)
        {
            this.baselineHierarchyService = baselineHierarchyService;
        }

        public async Task<(Guid ProjectId, Guid BaselineId)> GetProjectAndBaselineId(Guid baselineItemzId)
        {
            List<NestedBaselineHierarchyIdRecordDetailsDTO> allParentBaselineHierarchy = new List<NestedBaselineHierarchyIdRecordDetailsDTO>();
            var returnedParentHierarchyList = await baselineHierarchyService.__Get_All_Parents_Baseline_Hierarchy_By_GUID__Async(baselineItemzId);

            if (returnedParentHierarchyList != null)
            {
                allParentBaselineHierarchy = returnedParentHierarchyList.ToList();
            }

            var projectId = FindRecordUsingLambda(hierarchy: allParentBaselineHierarchy, recordType: "Project", level: 1)?.RecordId ?? Guid.Empty;
            var baselineId = FindRecordUsingLambda(hierarchy: allParentBaselineHierarchy, recordType: "Baseline", level: 2)?.RecordId ?? Guid.Empty;

            return (projectId, baselineId);
        }

        #region Finding_Record_In_Parent_Hierarchy_Nodes

        private NestedBaselineHierarchyIdRecordDetailsDTO? FindRecordUsingLambda(List<NestedBaselineHierarchyIdRecordDetailsDTO> hierarchy, string recordType, int level)
        {
            return hierarchy
                .SelectMany(parent => GetAllRecords(parent))
                .FirstOrDefault(record => record.RecordType == recordType && record.Level == level);
        }

        private IEnumerable<NestedBaselineHierarchyIdRecordDetailsDTO> GetAllRecords(NestedBaselineHierarchyIdRecordDetailsDTO parent)
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
