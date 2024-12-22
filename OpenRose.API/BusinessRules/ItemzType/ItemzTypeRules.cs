// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using ItemzApp.API.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ItemzApp.API.BusinessRules.ItemzType
{
    public class ItemzTypeRules : IItemzTypeRules
    {
        private readonly IItemzTypeRepository _itemzTypeRepository;
        private readonly ILogger<ItemzTypeRules> _logger;
        public ItemzTypeRules(IItemzTypeRepository itemzTypeRepository,
                                 ILogger<ItemzTypeRules> logger)
        {
            _itemzTypeRepository = itemzTypeRepository ?? throw new ArgumentNullException(nameof(itemzTypeRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        /// <summary>
        /// Use this method to check if the ItemzType with given name already exists. In General, 
        /// This check shall be performed before inserting or updating ItemzType.
        /// </summary>
        /// <param name="projectId">Project Id in Guid form in which we are checking for ItemzType with a specific name</param>
        /// <param name="itemzTypeName">Name of the ItemzType to be checked for uniqueness</param>
        /// <returns>true if ItemzType with ItemzTypeName found otherwise false</returns>
        private async Task<bool> HasItemzTypeWithNameAsync(Guid projectId, string itemzTypeName)
        {
            if (await _itemzTypeRepository.HasItemzTypeWithNameAsync(projectId ,itemzTypeName.Trim().ToLower()))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Used for verifying if project contains itemzType with the 
        /// same name as the one used for inserting or updating
        /// </summary>
        /// <param name="projectId">Project Id in Guid form in which we are checking for ItemzType with a specific name</param>
        /// <param name="targetItemzTypeName">New or updated itemzType name</param>
        /// <param name="sourceItemzTypeName">Old itemzType name. No need to pass this for checking rule against creating itemzType action</param>
        /// <returns>true if itemzType with same name exist in the repository otherwise false</returns>
        public async Task<bool> UniqueItemzTypeNameRuleAsync(System.Guid projectId, string targetItemzTypeName, string? sourceItemzTypeName = null)
        {
            if (sourceItemzTypeName != null)
            { // Update existing itemzType name
                if (sourceItemzTypeName != targetItemzTypeName)
                { // Source and Target are different names
                    return await HasItemzTypeWithNameAsync(projectId, targetItemzTypeName);
                }
                return false;
            }
            else
            { // Create new itemzType action
                return await HasItemzTypeWithNameAsync(projectId, targetItemzTypeName);
            }
        }
    }
}
