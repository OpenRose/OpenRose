// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.Services.ItemzTypeItemzsService;
using OpenRose.WebUI.Client.SharedModels;

namespace OpenRose.WebUI.Client.Services.ItemzTypeItemzsService
{
    public class DummyItemzTypeItemzsService : IItemzTypeItemzsService
    {
        public Task __DELETE_ItemzType_and_Itemz_Association__Async(ItemzTypeItemzDTO body)
        {
            throw new NotImplementedException();
        }

        public Task<GetItemzDTO> __GET_Check_ItemzType_Itemz_Association_Exists__Async(Guid? itemzTypeId, Guid? itemzId)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<GetItemzDTO>> __GET_Itemzs_By_ItemzType__Async(Guid itemzTypeId, int? pageNumber, int? pageSize, string orderBy)
        {
            throw new NotImplementedException();
        }

        public Task<int> __GET_Itemz_Count_By_ItemzType__Async(Guid itemzTypeId)
        {
            throw new NotImplementedException();
        }

        public Task<GetItemzDTO> __POST_Associate_Itemz_To_ItemzType__Async(bool? atBottomOfChildNodes, ItemzTypeItemzDTO body)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<GetItemzDTO>> __POST_Create_Itemz_Collection_By_ItemzType__Async(Guid itemzTypeId, IEnumerable<CreateItemzDTO> body)
        {
            throw new NotImplementedException();
        }

        public Task<GetItemzDTO> __POST_Create_Single_Itemz_By_ItemzType__Async(Guid itemzTypeId, bool? atBottomOfChildNodes, CreateItemzDTO body)
        {
            throw new NotImplementedException();
        }
    }
}
