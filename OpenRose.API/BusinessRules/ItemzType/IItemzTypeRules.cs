// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System.Threading.Tasks;

namespace ItemzApp.API.BusinessRules.ItemzType
{
    public interface IItemzTypeRules
    {
        public Task<bool> UniqueItemzTypeNameRuleAsync(System.Guid ProjectId,  string sourceItemzTypeName, string? targetItemzTypeName = null);
    }
}
