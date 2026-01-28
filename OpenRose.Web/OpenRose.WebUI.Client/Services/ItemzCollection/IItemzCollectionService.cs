// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.SharedModels;

namespace OpenRose.WebUI.Client.Services.ItemzCollection
{
	public interface IItemzCollectionService
	{

		//public Task<ICollection<GetItemzDTO>> __GET_Itemz_Collection_By_GUID_IDS__Async(IEnumerable<System.Guid> ids);

		public Task<ICollection<GetItemzDTO>> __POST_Create_Itemz_Collection__Async(IEnumerable<CreateItemzDTO> body);

		// New: POST endpoint that accepts GUIDs in JSON body to retrieve multiple Itemz (avoids long GET urls)
		public Task<ICollection<GetItemzDTO>> __POST_GET_Itemz_Collection_By_GUID_IDS__Async(IEnumerable<System.Guid> ids);

	}
}
