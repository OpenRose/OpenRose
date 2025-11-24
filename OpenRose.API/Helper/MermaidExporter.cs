// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Collections.Generic;
using System.Text;
using ItemzApp.API.Models;

namespace ItemzApp.API.Helper
{
	public static class MermaidExporter
	{
		// Live hierarchy
		public static string Generate(NestedHierarchyIdRecordDetailsDTO root,
									  IEnumerable<ItemzTraceDTO> traces,
									  Guid rootId)
		{
			var sb = new StringBuilder();
			sb.AppendLine("flowchart TD");

			EmitHierarchyInline(root, sb, rootId, 1);

			// Emit trace links (dotted)
			foreach (var t in traces)
			{
				sb.AppendLine($"    {t.FromTraceItemzId} -.-> {t.ToTraceItemzId}");
			}

			return sb.ToString();
		}



		// Baseline hierarchy
		public static string GenerateBaseline(NestedBaselineHierarchyIdRecordDetailsDTO root,
											  IEnumerable<BaselineItemzTraceDTO> traces,
											  Guid rootId)
		{
			var sb = new StringBuilder();
			sb.AppendLine("flowchart TD");

			EmitBaselineHierarchyInline(root, sb, rootId, 1);

			foreach (var t in traces)
			{
				sb.AppendLine($"    {t.FromTraceBaselineItemzId} -.-> {t.ToTraceBaselineItemzId}");
			}

			return sb.ToString();
		}

		// Emit live hierarchy edges inline with indentation
		private static void EmitHierarchyInline(NestedHierarchyIdRecordDetailsDTO node,
												StringBuilder sb,
												Guid rootId,
												int indentLevel)
		{
			string parent = RenderNode(node.RecordId, node.RecordType, node.Name, node.RecordId == rootId);
			string indent = new string(' ', indentLevel * 4);

			// Always emit the node itself
			sb.AppendLine($"{indent}{parent}");

			if (node.Children != null && node.Children.Count > 0)
			{
				foreach (var child in node.Children)
				{
					string childText = RenderNode(child.RecordId, child.RecordType, child.Name, child.RecordId == rootId);
					sb.AppendLine($"{indent}{parent} --> {childText}");
					EmitHierarchyInline(child, sb, rootId, indentLevel + 1);
				}
			}
		}


		// Emit baseline hierarchy edges inline with indentation
		private static void EmitBaselineHierarchyInline(NestedBaselineHierarchyIdRecordDetailsDTO node,
														StringBuilder sb,
														Guid rootId,
														int indentLevel)
		{
			string parent = RenderNode(node.RecordId, node.RecordType, node.Name, node.RecordId == rootId);
			string indent = new string(' ', indentLevel * 4);

			// Always emit the node itself
			sb.AppendLine($"{indent}{parent}");

			if (node.Children != null && node.Children.Count > 0)
			{
				foreach (var child in node.Children)
				{
					string childText = RenderNode(child.RecordId, child.RecordType, child.Name, child.RecordId == rootId);
					sb.AppendLine($"{indent}{parent} --> {childText}");
					EmitBaselineHierarchyInline(child, sb, rootId, indentLevel + 1);
				}
			}
		}


		// Render node with correct shape and prefix
		private static string RenderNode(Guid id, string? recordType, string? name, bool isRoot)
		{
			string label = recordType?.ToLowerInvariant() switch
			{
				"project" => $"Project :: {name}",
				"itemztype" => $"ItemzType :: {name}",
				"baseline" => $"Baseline :: {name}",
				"baselineitemztype" => $"BaselineItemzType :: {name}",
				"itemz" => $"{name}",
				"baselineitemz" => $"{name}",
				_ => $"{name}"
			};

			if (isRoot)
			{
				return $"{id}(({label}))"; // circle for root
			}
			return $"{id}[{label}]"; // rectangle for others
		}
	}
}
