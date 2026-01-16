// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.


using ItemzApp.API.Constants;
using ItemzApp.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ItemzApp.API.Helper
{
	public static class BaselineImportTransformationUtility
	{

		public static ProjectImportNode TransformBaselineToProject(BaselineImportNode baselineNode)
		{
			var projectDto = new GetProjectDTO
			{
				Id = Guid.NewGuid(),
				Name = baselineNode.Baseline?.Name,
				Description = baselineNode.Baseline?.Description,
				Status = "New",
				CreatedBy = baselineNode.Baseline?.CreatedBy,
				CreatedDate = baselineNode.Baseline?.CreatedDate ?? DateTimeOffset.UtcNow
			};

			var itemzTypeNodes = baselineNode.BaselineItemzTypes?
				.Select(TransformBaselineItemzTypeToItemzTypeNode)
				.ToList();

			return new ProjectImportNode
			{
				Project = projectDto,
				ItemzTypes = itemzTypeNodes
			};
		}


		public static ItemzTypeImportNode TransformBaselineItemzTypeToItemzTypeNode(
											BaselineItemzTypeImportNode baselineNode)
		{
			return new ItemzTypeImportNode
			{
				ItemzType = new GetItemzTypeDTO
				{
					Id = baselineNode.BaselineItemzType.Id,
					Name = baselineNode.BaselineItemzType.Name,
					Status = baselineNode.BaselineItemzType.Status,
					Description = baselineNode.BaselineItemzType.Description,
					CreatedBy = baselineNode.BaselineItemzType.CreatedBy,
					CreatedDate = baselineNode.BaselineItemzType.CreatedDate,
					IsSystem = baselineNode.BaselineItemzType.IsSystem
				},
				Itemz = baselineNode.BaselineItemz?
					.Select(TransformBaselineNodeToItemzNode) // this already exists
					.ToList()
			};
		}


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

		public static ItemzImportNode TransformBaselineNodeToItemzNode(BaselineItemzImportNode baselineNode)
		{
			return new ItemzImportNode
			{
				Itemz = TransformBaselineItemzToItemz(baselineNode.BaselineItemz),
				SubItemz = baselineNode.BaselineSubItemz?.Select(TransformBaselineNodeToItemzNode).ToList()
			};
		}

		public static List<ItemzTraceDTO> TransformBaselineTracesToItemzTraces(IEnumerable<BaselineItemzTraceDTO>? baselineTraces)
		{
			return (baselineTraces ?? Enumerable.Empty<BaselineItemzTraceDTO>())
				.Select(t =>
				{

					//TODO :: We should move Normalize Tracelabel  to a helper method
					//		  so that it can be reused in other places as well.

					// Normalize TraceLabel: preserve null, trim, and defensively truncate to 32 chars
					string? label = t.TraceLabel;
					if ((!string.IsNullOrWhiteSpace(label)) && (label != Sentinel.TraceLabelDefault))
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

					return new ItemzTraceDTO
					{
						FromTraceItemzId = t.FromTraceBaselineItemzId,
						ToTraceItemzId = t.ToTraceBaselineItemzId,
						TraceLabel = label
					};
				})
				.ToList();
		}
	}
}
