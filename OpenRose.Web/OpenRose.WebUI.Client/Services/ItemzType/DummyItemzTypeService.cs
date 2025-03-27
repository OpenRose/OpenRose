// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.SharedModels;

namespace OpenRose.WebUI.Client.Services.ItemzType
{
    public class DummyItemzTypeService : IItemzTypeService
    {
        public Task __DELETE_ItemzType_By_GUID_ID__Async(Guid itemzTypeId)
        {
            throw new NotImplementedException();
        }

		public Task<GetItemzTypeDTO> __POST_Copy_ItemzType_By_GUID_ID__Async(CopyItemzTypeDTO body)
		{
			throw new NotImplementedException();
		}

		public Task<GetItemzTypeDTO?> __POST_Create_ItemzType__Async(CreateItemzTypeDTO createItemzTypeDTO)
        {
            throw new NotImplementedException();
        }

        public Task __POST_Move_ItemzType_Between_ItemzTypes__Async(Guid? movingItemzTypeId, Guid? firstItemzTypeId, Guid? secondItemzTypeId)
        {
            throw new NotImplementedException();
        }

        public Task __POST_Move_ItemzType__Async(Guid movingItemzTypeId, Guid? targetProjectId, bool? atBottomOfChildNodes)
        {
            throw new NotImplementedException();
        }

        public Task<GetItemzTypeDTO?> __PUT_Update_ItemzType_By_GUID_ID__Async(Guid itemzTypeId, UpdateItemzTypeDTO updateItemzTypeDTO)
        {
            throw new NotImplementedException();
        }

        public Task<GetItemzTypeDTO?> __Single_ItemzType_By_GUID_ID__Async(Guid itemzTypeId)
        {
            throw new NotImplementedException();
        }
    }
}
