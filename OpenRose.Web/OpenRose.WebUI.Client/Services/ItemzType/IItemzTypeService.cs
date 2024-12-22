// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System.Threading.Tasks;
using System.Collections.Generic;
using OpenRose.WebUI.Client.SharedModels;

namespace OpenRose.WebUI.Client.Services.ItemzType
{
    public interface IItemzTypeService
    {
		public Task<GetItemzTypeDTO?> __Single_ItemzType_By_GUID_ID__Async(Guid itemzTypeId);

		public Task<GetItemzTypeDTO?> __POST_Create_ItemzType__Async(CreateItemzTypeDTO createItemzTypeDTO);

		public Task<GetItemzTypeDTO?> __PUT_Update_ItemzType_By_GUID_ID__Async(Guid itemzTypeId, UpdateItemzTypeDTO updateItemzTypeDTO);

		public Task __DELETE_ItemzType_By_GUID_ID__Async(Guid itemzTypeId);

		public Task __POST_Move_ItemzType__Async(Guid movingItemzTypeId, Guid? targetProjectId, bool? atBottomOfChildNodes);

		public Task __POST_Move_ItemzType_Between_ItemzTypes__Async(Guid? movingItemzTypeId, Guid? firstItemzTypeId, Guid? secondItemzTypeId);

		///// <summary>
		///// Gets collection of Projects
		///// </summary>
		///// <returns>Returns collection of Projects based on sorting order</returns>
		///// <exception cref="ApiException">A server side error occurred.</exception>
		//System.Threading.Tasks.Task<System.Collections.Generic.ICollection<GetProjectDTO>> __GET_Projects__Async();

		///// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		///// <summary>
		///// Gets collection of Projects
		///// </summary>
		///// <returns>Returns collection of Projects based on sorting order</returns>
		///// <exception cref="ApiException">A server side error occurred.</exception>
		//System.Threading.Tasks.Task<System.Collections.Generic.ICollection<GetProjectDTO>> __GET_Projects__Async(System.Threading.CancellationToken cancellationToken);

	}
}
