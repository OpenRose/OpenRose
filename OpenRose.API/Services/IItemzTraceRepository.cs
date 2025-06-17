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
    public interface IItemzTraceRepository
    {

        public Task<bool> SaveAsync();

        public Task<IEnumerable<ItemzJoinItemzTrace>> GetAllTracesByItemzIdAsync(Guid itemzId);

        public Task<ItemzParentAndChildTraceDTO> GetAllParentAndChildTracesByItemzIdAsync(Guid itemzId);

        public Task<bool> ItemzsTraceExistsAsync(ItemzTraceDTO itemzTraceDTO);

        public Task EstablishTraceBetweenItemzAsync(ItemzTraceDTO itemzTraceDTO);

        public Task<bool> ItemzExistsAsync(Guid itemzId);

        public Task<int> GetFromTraceCountByItemz(Guid itemzId);

        public Task<int> GetToTraceCountByItemz(Guid itemzId);

        public Task<int> GetAllFromAndToTracesCountByItemzIdAsync(Guid itemzId);

        public Task<bool> RemoveItemzTraceAsync(ItemzTraceDTO itemzTraceDTO);

        public Task<int> RemoveAllFromItemzTraceAsync(Guid itemzId);

        public Task<int> RemoveAllToItemzTraceAsync(Guid itemzId);

		Task<List<ItemzJoinItemzTrace>> GetAllTracesForItemzIdsAsync(IEnumerable<Guid> itemzIds);


	}
}
