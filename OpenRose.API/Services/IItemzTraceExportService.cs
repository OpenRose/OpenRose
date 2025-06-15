// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.


using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ItemzApp.API.Models;

namespace ItemzApp.API.Services
{
	public interface IItemzTraceExportService
	{
		/// <summary>
		/// Gets all ItemzTraceDTO where both FromItemzId and ToItemzId are in the provided set.
		/// Ensures no duplicate traces are exported.
		/// </summary>
		/// <param name="exportedItemzIds">Set of Itemz IDs included in the export</param>
		/// <returns>List of unique ItemzTraceDTO </returns>
		Task<List<ItemzTraceDTO>> GetTracesForExportAsync(HashSet<Guid> exportedItemzIds);
	}
}