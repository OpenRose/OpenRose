// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.


using ItemzApp.API.Models;
using System.Collections.Generic;
using System.Linq;

namespace ItemzApp.API.Helper
{
	public static class BaselineImportTransformationUtility
	{
		public static GetItemzDTO TransformBaselineItemzToItemz(GetBaselineItemzDTO baselineItemz)
		{
			return new GetItemzDTO
			{
				Id = baselineItemz.Id,
				Name = baselineItemz.Name,
				Status = baselineItemz.Status,
				Priority = baselineItemz.Priority,
				Description = baselineItemz.Description,
				CreatedBy = baselineItemz.CreatedBy,
				CreatedDate = baselineItemz.CreatedDate,
				Severity = baselineItemz.Severity
			};
		}

		public static ItemzExportNode TransformBaselineNodeToItemzNode(BaselineItemzExportNode baselineNode)
		{
			return new ItemzExportNode
			{
				Itemz = TransformBaselineItemzToItemz(baselineNode.BaselineItemz),
				SubItemz = baselineNode.BaselineSubItemz?.Select(TransformBaselineNodeToItemzNode).ToList()
			};
		}

		public static List<ItemzTraceDTO> TransformBaselineTracesToItemzTraces(IEnumerable<BaselineItemzTraceDTO>? baselineTraces)
		{
			return (baselineTraces ?? Enumerable.Empty<BaselineItemzTraceDTO>())
				.Select(t => new ItemzTraceDTO
				{
					FromTraceItemzId = t.FromTraceBaselineItemzId,
					ToTraceItemzId = t.ToTraceBaselineItemzId
				})
				.ToList();
		}
	}
}
