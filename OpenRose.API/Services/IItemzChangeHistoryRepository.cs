// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using ItemzApp.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ItemzApp.API.Services
{
    public interface IItemzChangeHistoryRepository
    {
        Task<IEnumerable<ItemzChangeHistory>> GetItemzChangeHistoryAsync(Guid ItemzId);

        Task<int> DeleteItemzChangeHistoryAsync(Guid ItemzId, DateTimeOffset? DeleteUptoDateTime = null);


    }
}
