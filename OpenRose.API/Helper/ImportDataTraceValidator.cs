// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using ItemzApp.API.Models;

namespace ItemzApp.API.Helper
{
	public static class ImportDataTraceValidator
	{
		public static List<string> ValidateTraceLinks(RepositoryExportDTO dto, string detectedType)
		{
			var errors = new List<string>();
			var validIds = new HashSet<Guid>();

			if (IsItemzBased(detectedType))
			{
				// Project → ItemzType → Itemz
				foreach (var project in dto.Projects ?? Enumerable.Empty<ProjectExportNode>())
				{
					foreach (var itemzType in project.ItemzTypes ?? Enumerable.Empty<ItemzTypeExportNode>())
					{
						CollectItemzIds(itemzType.Itemz, validIds);
					}
				}

				// Top-level ItemzTypes
				foreach (var itemzType in dto.ItemzTypes ?? Enumerable.Empty<ItemzTypeExportNode>())
				{
					CollectItemzIds(itemzType.Itemz, validIds);
				}

				// Top-level Itemz
				CollectItemzIds(dto.Itemz, validIds);

				// Validate ItemzTraces
				foreach (var trace in dto.ItemzTraces ?? Enumerable.Empty<ItemzTraceDTO>())
				{

					if (trace.FromTraceItemzId == Guid.Empty || trace.ToTraceItemzId == Guid.Empty)
					{
						errors.Add($"Empty GUID in trace: {trace.FromTraceItemzId} → {trace.ToTraceItemzId}");
						continue;
					}

					ValidateTrace(trace.FromTraceItemzId, trace.ToTraceItemzId, validIds, errors);
				}
			}
			else if (IsBaselineBased(detectedType))
			{
				// Baseline → BaselineItemzType → BaselineItemz
				foreach (var baseline in dto.Baselines ?? Enumerable.Empty<BaselineExportNode>())
				{
					foreach (var itemzType in baseline.BaselineItemzTypes ?? Enumerable.Empty<BaselineItemzTypeExportNode>())
					{
						CollectBaselineItemzIds(itemzType.BaselineItemz, validIds);
					}
				}

				// Top-level BaselineItemzTypes
				foreach (var itemzType in dto.BaselineItemzTypes ?? Enumerable.Empty<BaselineItemzTypeExportNode>())
				{
					CollectBaselineItemzIds(itemzType.BaselineItemz, validIds);
				}

				// Top-level BaselineItemz
				CollectBaselineItemzIds(dto.BaselineItemz, validIds);

				// Validate BaselineItemzTraces
				foreach (var trace in dto.BaselineItemzTraces ?? Enumerable.Empty<BaselineItemzTraceDTO>())
				{

					if (trace.FromTraceBaselineItemzId == Guid.Empty || trace.ToTraceBaselineItemzId == Guid.Empty)
					{
						errors.Add($"Empty GUID in baseline trace: {trace.FromTraceBaselineItemzId} → {trace.ToTraceBaselineItemzId}");
						continue;
					}
					ValidateTrace(trace.FromTraceBaselineItemzId, trace.ToTraceBaselineItemzId, validIds, errors);
				}
			}

			return errors;
		}

		private static void ValidateTrace(Guid fromId, Guid toId, HashSet<Guid> validIds, List<string> errors)
		{
			if (!validIds.Contains(fromId))
			{
				errors.Add($"Trace source ID not found: {fromId}");
			}

			if (!validIds.Contains(toId))
			{
				errors.Add($"Trace target ID not found: {toId}");
			}
		}


		private static void CollectItemzIds(List<ItemzImportNode>? nodes, HashSet<Guid> ids)
		{
			if (nodes == null) return;

			foreach (var node in nodes)
			{
				if (node?.Itemz != null)
				{
					ids.Add(node.Itemz.Id);
					CollectItemzIds(node.SubItemz, ids);
				}
			}
		}

		private static void CollectBaselineItemzIds(List<BaselineItemzExportNode>? nodes, HashSet<Guid> ids)
		{
			if (nodes == null) return;

			foreach (var node in nodes)
			{
				if (node?.BaselineItemz != null)
				{
					ids.Add(node.BaselineItemz.Id);
					CollectBaselineItemzIds(node.BaselineSubItemz, ids);
				}
			}
		}

		private static bool IsItemzBased(string type) =>
			type is "Project" or "ItemzType" or "Itemz";

		private static bool IsBaselineBased(string type) =>
			type is "Baseline" or "BaselineItemzType" or "BaselineItemz";
	}
}
