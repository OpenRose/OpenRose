// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.SharedModels;

namespace OpenRose.WebUI.Client.Services.ItemzChangeHistoryByRepositoryService
{
    public interface IItemzChangeHistoryByRepositoryService
	{
		public Task<int> __GET_Number_of_ItemzChangeHistory_By_Repository__Async();

		public Task<int> __GET_Number_of_ItemzChangeHistory_By_Repository_Upto_DateTime__Async(GetNumberOfChangeHistoryByRepositoryDTO body);

	}
}
