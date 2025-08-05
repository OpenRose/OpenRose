﻿// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.


using ItemzApp.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ItemzApp.API.Helper
{
	public static class BaselineImportTransformationUtility
	{

		public static ProjectExportNode TransformBaselineToProject(BaselineExportNode baselineNode)
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

			return new ProjectExportNode
			{
				Project = projectDto,
				ItemzTypes = itemzTypeNodes
			};
		}


		public static ItemzTypeExportNode TransformBaselineItemzTypeToItemzTypeNode(
											BaselineItemzTypeExportNode baselineNode)
		{
			return new ItemzTypeExportNode
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
