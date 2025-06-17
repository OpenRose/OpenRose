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
	public class BaselineItemzTraceExportService : IBaselineItemzTraceExportService
	{
		private readonly IBaselineItemzTraceRepository _baselineItemzTraceRepository;

		public BaselineItemzTraceExportService(IBaselineItemzTraceRepository baselineItemzTraceRepository)
		{
			_baselineItemzTraceRepository = baselineItemzTraceRepository;
		}

		public async Task<List<BaselineItemzTraceDTO>> GetTracesForExportAsync(HashSet<Guid> exportedBaselineItemzIds)
		{
			var allTraces = await _baselineItemzTraceRepository.GetAllTracesForBaselineItemzIdsAsync(exportedBaselineItemzIds);

			var uniqueTraces = new HashSet<(Guid, Guid)>();
			var result = new List<BaselineItemzTraceDTO>();

			foreach (var trace in allTraces)
			{
				// Only include traces where both ends are in exported scope
				if (exportedBaselineItemzIds.Contains(trace.BaselineFromItemzId) && exportedBaselineItemzIds.Contains(trace.BaselineToItemzId))
				{
					var traceKey = (trace.BaselineFromItemzId, trace.BaselineToItemzId);
					if (!uniqueTraces.Contains(traceKey))
					{
						uniqueTraces.Add(traceKey);
						result.Add(new BaselineItemzTraceDTO
						{
							FromTraceBaselineItemzId = trace.BaselineFromItemzId,
							ToTraceBaselineItemzId = trace.BaselineToItemzId
						});
					}
				}
			}

			return result;
		}
	}
}