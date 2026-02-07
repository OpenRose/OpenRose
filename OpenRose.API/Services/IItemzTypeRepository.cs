// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ItemzApp.API.Entities;

namespace ItemzApp.API.Services
{
    public interface IItemzTypeRepository
    {
        public Task<ItemzType> GetItemzTypeAsync(Guid ItemzTypeId);

        public Task<ItemzType> GetItemzTypeForUpdateAsync(Guid ItemzTypeId);

        public Task<IEnumerable<ItemzType>?> GetItemzTypesAsync();

        public Task<IEnumerable<ItemzType>> GetItemzTypesAsync(IEnumerable<Guid> itemzTypeIds);
                
        public void AddItemzType(ItemzType itemzType);

        public Task AddNewItemzTypeHierarchyAsync(ItemzType itemzTypeEntity);

        public Task<bool> SaveAsync();

        public Task<bool> ItemzTypeExistsAsync(Guid ItemzTypeId);

        public void UpdateItemzType(ItemzType itemzType);

        public void DeleteItemzType(ItemzType itemzType);

        public Task<bool> HasItemzTypeWithNameAsync(Guid projectId, string itemzTypeName);

        public Task<bool> DeleteItemzTypeItemzHierarchyAsync(Guid itemzTypeId);

        //public Task<string?> GetTopItemzHierarchyID(Guid parentItemzTypeId);

        //public Task<string?> GetLastItemzHierarchyID(Guid parentItemzTypeId);

        public Task MoveItemzTypeToAnotherProjectAsync(Guid movingItemzTypeId, Guid targetProjectId, bool atBottomOfChildNodes = true);

        public Task MoveItemzTypeBetweenTwoHierarchyRecordsAsync(Guid between1stItemzTypeId, Guid between2ndItemzTypeId, Guid movingItemzTypeId);

		public Task<Guid> CopyItemzTypeAsync(Guid ItemzTypeId);

        public Task<ItemzType?> GetItemzTypeByNameAsync(Guid projectId, string itemzTypeName);
	}
}
