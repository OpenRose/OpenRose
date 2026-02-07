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
		/// Used for verifying if a Project contains an ItemzType with the same name
		/// as the one being inserted or updated. This method allows case-only renames
		/// when the ItemzType being updated has the same Id as the existing record.
		/// </summary>
		/// <param name="projectId">Project Id in Guid form in which we are checking for ItemzType with a specific name. </param>
		/// <param name="targetItemzTypeName">New or updated ItemzType name to be checked for uniqueness. </param>
		/// <param name="sourceItemzTypeId">
		/// Optional ItemzType Id of the record being updated. If provided, case-only renames
		/// are allowed when the existing ItemzType with the same name has the same Id.
		/// </param>
		/// <returns>
		/// true if another ItemzType with the same name exists in the repository for the given Project,
		/// otherwise false.
		/// </returns>

		public async Task<bool> UniqueItemzTypeNameRuleAsync(Guid projectId, string targetItemzTypeName, Guid? sourceItemzTypeId = null)
		{
			var existingItemzType = await _itemzTypeRepository.GetItemzTypeByNameAsync(projectId, targetItemzTypeName);

			if (existingItemzType != null)
			{
				// If updating, allow case-only rename if IDs match
				if (sourceItemzTypeId.HasValue && existingItemzType.Id == sourceItemzTypeId.Value)
				{
					return false; // no conflict
				}
				return true; // conflict with another ItemzType
			}

			return false; // no conflict
		}
	}
}
