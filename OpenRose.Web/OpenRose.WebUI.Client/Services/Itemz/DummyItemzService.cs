// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.SharedModels;

namespace OpenRose.WebUI.Client.Services.Itemz
{
    public class DummyItemzService : IItemzService
    {
        public Task __Delete_All_Orphan_Itemz__Async()
        {
            throw new NotImplementedException();
        }

        public Task __DELETE_Itemz_By_GUID_ID__Async(Guid itemzId)
        {
            throw new NotImplementedException();
        }

        public Task<(ICollection<GetItemzWithBasePropertiesDTO>, string)> __GET_Orphan_Itemzs_Collection__Async(int? pageNumber, int? pageSize, string orderBy)
        {
            throw new NotImplementedException();
        }

        public Task<int> __GET_Orphan_Itemzs_Count__Async()
        {
            throw new NotImplementedException();
        }

		public Task<GetItemzDTO> __POST_Copy_Itemz_By_GUID_ID__Async(CopyItemzDTO body)
		{
			throw new NotImplementedException();
		}

		public Task<GetItemzDTO> __POST_Create_Itemz_Between_Existing_Itemz__Async(Guid? firstItemzId, Guid? secondItemzId, CreateItemzDTO body)
        {
            throw new NotImplementedException();
        }

        public Task<GetItemzDTO> __POST_Create_Itemz__Async(Guid? parentId, bool? atBottomOfChildNodes, CreateItemzDTO body)
        {
            throw new NotImplementedException();
        }

        public Task __POST_Move_Itemz_Between_Existing_Itemz__Async(Guid? movingItemzId, Guid? firstItemzId, Guid? secondItemzId)
        {
            throw new NotImplementedException();
        }

        public Task __POST_Move_Itemz__Async(Guid movingItemzId, Guid targetId, bool atBottomOfChildNodes)
        {
            throw new NotImplementedException();
        }

        public Task __PUT_Update_Itemz_By_GUID_ID__Async(Guid itemzId, UpdateItemzDTO updateItemzDTO)
        {
            throw new NotImplementedException();
        }

        public Task<GetItemzDTO?> __Single_Itemz_By_GUID_ID__Async(Guid itemzId)
        {
            throw new NotImplementedException();
        }
    }
}
