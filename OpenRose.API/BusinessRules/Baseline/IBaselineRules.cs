// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System.Threading.Tasks;

namespace ItemzApp.API.BusinessRules.Baseline
{
    public interface IBaselineRules
    {
        public Task<bool> UniqueBaselineNameRuleAsync(System.Guid ProjectId,  string sourceBaselineName, string? targetBaselineName = null);
    }
}
