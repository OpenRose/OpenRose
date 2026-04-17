// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System.Threading.Tasks;
using System.Collections.Generic;
using OpenRose.WebUI.Client.SharedModels;

namespace OpenRose.WebUI.Client.Services.Hierarchy
{
    public interface IHierarchyService
	{
		public Task<HierarchyIdRecordDetailsDTO> __Get_Hierarchy_Record_Details_By_GUID__Async(Guid recordId);
		
		public Task<HierarchyIdRecordDetailsDTO> __Get_Next_Sibling_Hierarchy_Record_Details_By_GUID__Async(Guid recordId);

		public Task<ICollection<HierarchyIdRecordDetailsDTO>> __Get_Immediate_Children_Hierarchy_By_GUID__Async(Guid recordId);

		public Task<ICollection<NestedHierarchyIdRecordDetailsDTO>> __Get_All_Parents_Hierarchy_By_GUID__Async(Guid recordId);
		
		public Task<ICollection<NestedHierarchyIdRecordDetailsDTO>> __Get_All_Children_Hierarchy_By_GUID__Async(Guid recordId);

        public Task<int> __Get_All_Children_Hierarchy_Count_By_GUID__Async(Guid recordId);

		/// <summary>
		/// PHASE 1: Updates estimation fields for a hierarchy record
		/// </summary>
		public Task<bool> __Update_Hierarchy_Estimation_Async__(Guid recordId, UpdateHierarchyEstimationDTO updateEstimationDTO);

		///// <summary>
		///// PHASE 1: Gets hierarchy record with all estimation details
		///// </summary>
		//public Task<HierarchyIdRecordDetailsDTO> __Get_Hierarchy_Record_With_Estimations_Async__(Guid recordId);

		public Task<string> __POST_Recalculate_Project_RollUpEstimations__Async(Guid projectHierarchyRecordId);


	}
}
