// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.SharedModels;

namespace OpenRose.WebUI.Client.Services.ItemzCollection
{
	public class DummyItemzCollectionService : IItemzCollectionService
	{
		//public Task<ICollection<GetItemzDTO>> __GET_Itemz_Collection_By_GUID_IDS__Async(IEnumerable<Guid> ids)
		//{
		//	throw new NotImplementedException();
		//}

		public Task<ICollection<GetItemzDTO>> __POST_Create_Itemz_Collection__Async(IEnumerable<CreateItemzDTO> body)
		{
			throw new NotImplementedException();
		}

		// New POST-by-ids stub
		public Task<ICollection<GetItemzDTO>> __POST_GET_Itemz_Collection_By_GUID_IDS__Async(IEnumerable<Guid> ids)
		{
			throw new NotImplementedException();
		}
	}
}