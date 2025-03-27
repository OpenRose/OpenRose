// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.SharedModels;

namespace OpenRose.WebUI.Client.Services.Itemz
{
    public interface IItemzService
    {
		public Task<GetItemzDTO?> __Single_Itemz_By_GUID_ID__Async(Guid itemzId);

		public Task<GetItemzDTO> __POST_Create_Itemz__Async(Guid? parentId, bool? atBottomOfChildNodes, CreateItemzDTO body);

		public Task<(ICollection<GetItemzWithBasePropertiesDTO>, string)> __GET_Orphan_Itemzs_Collection__Async(int? pageNumber, int? pageSize, string orderBy);

		public Task<int> __GET_Orphan_Itemzs_Count__Async();

		public Task<GetItemzDTO> __POST_Create_Itemz_Between_Existing_Itemz__Async(Guid? firstItemzId, Guid? secondItemzId, CreateItemzDTO body);

		public Task __POST_Move_Itemz_Between_Existing_Itemz__Async(Guid? movingItemzId, Guid? firstItemzId, Guid? secondItemzId);

		public Task __PUT_Update_Itemz_By_GUID_ID__Async(Guid itemzId, UpdateItemzDTO updateItemzDTO);

		public Task __DELETE_Itemz_By_GUID_ID__Async(Guid itemzId);

		public Task __POST_Move_Itemz__Async(Guid movingItemzId, Guid targetId, bool atBottomOfChildNodes);

		public Task __Delete_All_Orphan_Itemz__Async();

		public Task<GetItemzDTO> __POST_Copy_Itemz_By_GUID_ID__Async(CopyItemzDTO body);
	}
}
