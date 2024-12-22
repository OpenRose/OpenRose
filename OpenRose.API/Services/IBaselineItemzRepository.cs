// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ItemzApp.API.Entities;

namespace ItemzApp.API.Services
{
    public interface IBaselineItemzRepository
    {
        public Task<BaselineItemz?> GetBaselineItemzAsync(Guid BaselineItemzId);

        public Task<IEnumerable<BaselineItemz>?> GetBaselineItemzByItemzIdAsync(Guid ItemzId);

        public Task<bool> BaselineItemzExistsAsync(Guid baselineItemzId);

        public Task<int> GetBaselineItemzCountByItemzIdAsync(Guid ItemzId);

        public Task<IEnumerable<BaselineItemz>> GetBaselineItemzsAsync(IEnumerable<Guid> baselineItemzIds);

        public Task<bool> UpdateBaselineItemzsAsync(UpdateBaselineItemz updateBaselineItemz);
        public Task<bool> NOT_IN_USE_CheckBaselineitemzForInclusionBeforeImplementingAsync(UpdateBaselineItemz updateBaselineItemz);
    }
}
