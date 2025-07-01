// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ItemzApp.API.Models;

namespace ItemzApp.API.Services
{
	public class ItemzTraceExportService : IItemzTraceExportService
	{
		private readonly IItemzTraceRepository _itemzTraceRepository;

		public ItemzTraceExportService(IItemzTraceRepository itemzTraceRepository)
		{
			_itemzTraceRepository = itemzTraceRepository;
		}

		public async Task<List<ItemzTraceDTO>> GetTracesForExportAsync(HashSet<Guid> exportedItemzIds)
		{
			var allTraces = await _itemzTraceRepository.GetAllTracesForItemzIdsAsync(exportedItemzIds);

			var uniqueTraces = new HashSet<(Guid, Guid)>();
			var result = new List<ItemzTraceDTO>();

			foreach (var trace in allTraces)
			{
				// Only include traces where both ends are in exported scope
				if (exportedItemzIds.Contains(trace.FromItemzId) && exportedItemzIds.Contains(trace.ToItemzId))
				{
					var traceKey = (trace.FromItemzId, trace.ToItemzId);
					if (!uniqueTraces.Contains(traceKey))
					{
						uniqueTraces.Add(traceKey);
						result.Add(new ItemzTraceDTO
						{
							FromTraceItemzId = trace.FromItemzId,
							ToTraceItemzId = trace.ToItemzId
						});
					}
				}
			}

			return result;
		}
	}
}