// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Threading.Tasks;

namespace ItemzApp.API.BusinessRules.ItemzType
{

	public interface IItemzTypeRules
	{
		/// <summary>
		/// Ensures ItemzType name is unique within a project.
		/// For update scenarios, pass the source ItemzTypeId so case-only renames are allowed.
		/// </summary>
		/// <param name="projectId">Project Id in which uniqueness is checked</param>
		/// <param name="targetItemzTypeName">New or updated ItemzType name</param>
		/// <param name="sourceItemzTypeId">Optional: existing ItemzType Id when updating</param>
		/// <returns>true if conflict exists, false otherwise</returns>
		public Task<bool> UniqueItemzTypeNameRuleAsync(Guid projectId, string targetItemzTypeName, Guid? sourceItemzTypeId = null);
	}


}
