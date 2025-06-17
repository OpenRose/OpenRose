// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ItemzApp.API.Entities;
using ItemzApp.API.Helper;
using ItemzApp.API.Models;
using ItemzApp.API.ResourceParameters;

namespace ItemzApp.API.Services
{
    public interface IBaselineItemzTraceRepository
    {
        // public Task<bool> SaveAsync(); // This method is not expected to be called because Baseline Itemz Traces are managed via Stored Procedure

        public Task<IEnumerable<BaselineItemzJoinItemzTrace>> GetAllTracesByBaselineItemzIdAsync(Guid baselineItemzId);

        public Task<BaselineItemzParentAndChildTraceDTO> GetAllParentAndChildTracesByBaselineItemzIdAsync(Guid baselineItemzId);

        public Task<bool> BaselineItemzsTraceExistsAsync(BaselineItemzTraceDTO baselineItemzTraceDTO);

        public Task<bool> BaselineItemzExistsAsync(Guid baselineItemzId);

        public Task<int> GetFromTraceCountByBaselineItemz(Guid baselineItemzId);

        public Task<int> GetToTraceCountByBaselineItemz(Guid baselineItemzId);

        public Task<int> GetAllFromAndToTracesCountByBaselineItemzIdAsync(Guid baselineItemzId);

		//public Task<IEnumerable<BaselineItemzJoinItemzTrace>> GetAllTracesForBaselineItemzIdsAsync(HashSet<Guid> exportedBaselineItemzIds);

		Task<List<BaselineItemzJoinItemzTrace>> GetAllTracesForBaselineItemzIdsAsync(IEnumerable<Guid> baselineItemzIds);
	}
}
