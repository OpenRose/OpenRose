// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System.Threading.Tasks;
using System.Collections.Generic;
using OpenRose.WebUI.Client.SharedModels;

namespace OpenRose.WebUI.Client.Services.ItemzTypeItemzsService
{
    public interface IItemzTypeItemzsService
	{
		public Task<ICollection<GetItemzDTO>> __GET_Itemzs_By_ItemzType__Async(Guid itemzTypeId, int? pageNumber, int? pageSize, string orderBy);

		public Task<ICollection<GetItemzDTO>> __POST_Create_Itemz_Collection_By_ItemzType__Async(Guid itemzTypeId, IEnumerable<CreateItemzDTO> body);

		public Task<int> __GET_Itemz_Count_By_ItemzType__Async(Guid itemzTypeId);

		public Task<GetItemzDTO> __GET_Check_ItemzType_Itemz_Association_Exists__Async(Guid? itemzTypeId, Guid? itemzId);

		public Task<GetItemzDTO> __POST_Create_Single_Itemz_By_ItemzType__Async(Guid itemzTypeId, bool? atBottomOfChildNodes, CreateItemzDTO body);

		public Task<GetItemzDTO> __POST_Associate_Itemz_To_ItemzType__Async(bool? atBottomOfChildNodes, ItemzTypeItemzDTO body);

		public Task __DELETE_ItemzType_and_Itemz_Association__Async(ItemzTypeItemzDTO body);

	}
}
