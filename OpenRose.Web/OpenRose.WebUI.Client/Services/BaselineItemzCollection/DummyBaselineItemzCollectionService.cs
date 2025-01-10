// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.SharedModels;

namespace OpenRose.WebUI.Client.Services.BaselineItemzCollection
{
    public class DummyBaselineItemzCollectionService : IBaselineItemzCollectionService
    {
        public Task<ICollection<GetBaselineItemzDTO>> __GET_BaselineItemz_Collection_By_GUID_IDS__Async(IEnumerable<Guid> baselineItemzids)
        {
            throw new NotImplementedException();
        }
    }
}
