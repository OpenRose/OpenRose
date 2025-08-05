// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.


using ItemzApp.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace ItemzApp.API.Helper
{
	public static class BaselineImportUtility
	{
		public static List<BaselineItemzTraceDTO> FilterValidBaselineItemzTraces(
			List<BaselineItemzTraceDTO> traces,
			IEnumerable<BaselineItemzExportNode> validNodes)
		{
			var validIds = new HashSet<Guid>(
				validNodes.Select(n => n.BaselineItemz.Id)
			);

			return traces
				.Where(t => validIds.Contains(t.FromTraceBaselineItemzId) &&
							validIds.Contains(t.ToTraceBaselineItemzId))
				.ToList();
		}


		public static void FilterExcludedBaselineItemzAcrossRepository(
			RepositoryExportDTO repositoryExportDto,
			bool importExcludedBaselineItemz,
			ILogger logger)
		{
			if (importExcludedBaselineItemz || repositoryExportDto == null) return;

			var allValidNodes = new List<BaselineItemzExportNode>();

			// 1. Filter direct BaselineItemz list
			if (repositoryExportDto.BaselineItemz != null)
			{
				repositoryExportDto.BaselineItemz = FilterNodeList(
					repositoryExportDto.BaselineItemz,
					allValidNodes);
			}

			// 2. Filter BaselineItemz inside top-level BaselineItemzTypes
			if (repositoryExportDto.BaselineItemzTypes != null)
			{
				foreach (var typeNode in repositoryExportDto.BaselineItemzTypes)
				{
					if (typeNode.BaselineItemz != null)
					{
						typeNode.BaselineItemz = FilterNodeList(typeNode.BaselineItemz, allValidNodes);
					}
				}
			}

			// ✅ 3. Filter BaselineItemz inside Baseline → BaselineItemzTypes → BaselineItemz
			if (repositoryExportDto.Baselines != null)
			{
				foreach (var baselineDto in repositoryExportDto.Baselines)
				{
					if (baselineDto.BaselineItemzTypes == null) continue;

					foreach (var typeDto in baselineDto.BaselineItemzTypes)
					{
						if (typeDto.BaselineItemz != null)
						{
							typeDto.BaselineItemz = FilterNodeList(typeDto.BaselineItemz, allValidNodes);
						}
					}
				}
			}

			// 4. Prune traces to match only valid nodes
			repositoryExportDto.BaselineItemzTraces = FilterValidBaselineItemzTraces(
				repositoryExportDto.BaselineItemzTraces ?? new List<BaselineItemzTraceDTO>(),
				allValidNodes);

			logger.LogInformation("Filtered BaselineItemz globally across repository. Valid nodes retained: {Count}", allValidNodes.Count);
		}

		private static List<BaselineItemzExportNode> FilterNodeList(
			List<BaselineItemzExportNode> inputNodes,
			List<BaselineItemzExportNode> validCollector)
		{
			if (inputNodes == null) return new List<BaselineItemzExportNode>();

			var filtered = new List<BaselineItemzExportNode>();

			foreach (var node in inputNodes)
			{
				if (node?.BaselineItemz != null && node.BaselineItemz.isIncluded)
				{
					var newNode = new BaselineItemzExportNode
					{
						BaselineItemz = node.BaselineItemz,
						BaselineSubItemz = FilterNodeList(node.BaselineSubItemz ?? new List<BaselineItemzExportNode>(), validCollector)
					};

					filtered.Add(newNode);
					validCollector.Add(newNode);
				}
			}

			return filtered;
		}



	}
}
