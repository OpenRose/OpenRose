// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Collections.Generic;
using System.Text;
using ItemzApp.API.Models;

namespace ItemzApp.API.Helper
{
	/// <summary>
	/// MermaidExporter is responsible for converting Itemz OR BaselineItemz hierarchies and traces
	/// into Mermaid.js flowchart syntax for visualization.
	/// </summary>
	public static class MermaidExporter
	{
		/// <summary>
		/// Generates a Mermaid flowchart for a live hierarchy of Itemz. This includes Projects, ItemzTypes, and Itemz.
		/// </summary>
		/// <param name="root">
		/// The root node of the hierarchy (NestedHierarchyIdRecordDetailsDTO).
		/// </param>
		/// <param name="traces">
		/// A collection of trace links between Itemz (ItemzTraceDTO).
		/// These are rendered as dotted arrows in the diagram.
		/// </param>
		/// <param name="rootId">
		/// The unique identifier (Guid) of the root node. Used to highlight the root.
		/// </param>
		/// <returns>
		/// A string containing Mermaid.js flowchart syntax representing the hierarchy and traces.
		/// </returns>
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

		/// <summary>
		/// Generates a Mermaid flowchart for a baseline hierarchy of Itemz. This includes 
		/// Baselines, BaselineItemzTypes, and BaselineItemz.
		/// </summary>
		/// <param name="root">
		/// The root node of the baseline hierarchy (NestedBaselineHierarchyIdRecordDetailsDTO).
		/// </param>
		/// <param name="traces">
		/// A collection of baseline trace links (BaselineItemzTraceDTO).
		/// </param>
		/// <param name="rootId">
		/// The unique identifier (Guid) of the root node. Used to highlight the root.
		/// </param>
		/// <returns>
		/// A string containing Mermaid.js flowchart syntax representing the baseline hierarchy and traces.
		/// </returns>
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

		/// <summary>
		/// Recursively emits nodes and edges for a live hierarchy into the StringBuilder.
		/// </summary>
		/// <param name="node">The current hierarchy node.</param>
		/// <param name="sb">The StringBuilder accumulating Mermaid syntax.</param>
		/// <param name="rootId">The Guid of the root node.</param>
		/// <param name="indentLevel">Indentation level for readability.</param>
		private static void EmitHierarchyInline(NestedHierarchyIdRecordDetailsDTO node,
												StringBuilder sb,
												Guid rootId,
												int indentLevel)
		{
			string parent = RenderNode(node.RecordId, node.RecordType, node.Name, node.RecordId == rootId);
			string indent = new string(' ', indentLevel * 4);

			if (node.RecordId == rootId)
			{
				sb.AppendLine($"{indent}{parent}");
			}

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

		/// <summary>
		/// Recursively emits nodes and edges for a baseline hierarchy into the StringBuilder.
		/// </summary>
		private static void EmitBaselineHierarchyInline(NestedBaselineHierarchyIdRecordDetailsDTO node,
														StringBuilder sb,
														Guid rootId,
														int indentLevel)
		{
			string parent = RenderNode(node.RecordId, node.RecordType, node.Name, node.RecordId == rootId);
			string indent = new string(' ', indentLevel * 4);

			if(node.RecordId == rootId)
			{
				sb.AppendLine($"{indent}{parent}");
			}

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

		/// <summary>
		/// Renders a node into Mermaid syntax with appropriate shape and label.
		/// </summary>
		/// <param name="id">Unique Guid identifier of the node.</param>
		/// <param name="recordType">Type of record (e.g., Project, ItemzType, Baseline).</param>
		/// <param name="name">Display name of the node.</param>
		/// <param name="isRoot">True if this node is the root, otherwise false.</param>
		/// <returns>
		/// Mermaid syntax string for the node. Root nodes are circles, others are rectangles.
		/// </returns>
		private static string RenderNode(Guid id, string? recordType, string? name, bool isRoot)
		{
			string label = recordType?.ToLowerInvariant() switch
			{
				"project" => TransformLabelForMermaid($"Project :: {name}"),
				"itemztype" => TransformLabelForMermaid($"ItemzType :: {name}"),
				"baseline" => TransformLabelForMermaid($"Baseline :: {name}"),
				"baselineitemztype" => TransformLabelForMermaid($"BaselineItemzType :: {name}"),
				"itemz" => TransformLabelForMermaid(name),
				"baselineitemz" => TransformLabelForMermaid(name),
				_ => TransformLabelForMermaid(name)
			};

			return isRoot
				? $"{id}(({label}))"   // circle for root
				: $"{id}[{label}]";    // rectangle for others
		}

		/// <summary>
		/// Transforms a raw label string into a Mermaid-safe format.
		/// Mermaid diagrams break if labels contain certain special characters,
		/// so we sanitize them by replacing or escaping problematic symbols.
		/// </summary>
		/// <param name="input">The raw label text to be transformed.</param>
		/// <returns>A sanitized label string wrapped in double quotes.</returns>
		private static string TransformLabelForMermaid(string? input)
		{
			if (string.IsNullOrEmpty(input)) return "\"\"";

			string transformed = input
				.Replace("\"", "'")   // replace double quotes with single quotes
				.Replace("&", "and")  // replace ampersand with 'and'
				.Replace("<", "(")    // replace < with (
				.Replace(">", ")")   // replace > with )
				.Replace("%", "percent") // percent sign → word "percent"
				.Replace("end", "END")   // lowercase "end" → uppercase
				.Replace("[", "(")       // square bracket → parenthesis
				.Replace("]", ")")       // square bracket → parenthesis
				.Replace("{", "(")       // curly brace → parenthesis
				.Replace("}", ")");      // curly brace → parenthesis

			// Wrap the whole label in double quotes
			return $"\"{transformed}\"";
		}

	}
}
