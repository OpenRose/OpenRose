// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Threading.Tasks;

namespace ItemzApp.API.Services
{
    public interface IItemzChangeHistoryByProjectRepository
    {
        Task<int> DeleteItemzChangeHistoryByProjectAsync(Guid ProjectId, DateTimeOffset? DeleteUptoDateTime = null);
        Task<int> TotalNumberOfItemzChangeHistoryByProjectAsync(Guid ProjectId);
        Task<int> TotalNumberOfItemzChangeHistoryByProjectUptoDateTimeAsync(Guid ProjectId, DateTimeOffset? GetUptoDateTime = null);

    }
}
