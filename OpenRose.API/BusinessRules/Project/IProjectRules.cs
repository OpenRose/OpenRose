// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System.Threading.Tasks;

namespace ItemzApp.API.BusinessRules.Project
{
    public interface IProjectRules
    {
        public Task<bool> UniqueProjectNameRuleAsync(string sourceProjectName, string? targetProjectName = null);
    }
}
