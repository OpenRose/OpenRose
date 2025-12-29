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


						//TODO :: We should move Normalize Tracelabel  to a helper method
						//		  so that it can be reused in other places as well.
						// Normalize TraceLabel: preserve null, trim whitespace, and defensively truncate to 32 chars
						string? label = trace.TraceLabel;
						if (!string.IsNullOrWhiteSpace(label))
						{
							label = label.Trim();
							if (label.Length > 32)
							{
								label = label.Substring(0, 32);
							}
						}
						else
						{
							label = null;
						}

						result.Add(new BaselineItemzTraceDTO
						{
							FromTraceBaselineItemzId = trace.BaselineFromItemzId,
							ToTraceBaselineItemzId = trace.BaselineToItemzId,
							TraceLabel = label
						});
					}
				}
			}

			return result;
		}
	}
}