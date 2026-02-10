// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Collections.Generic;
using System.Text;
using ItemzApp.API.Constants;
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
							  Guid rootId,
							  string? baseUrl)
		{
			var sb = new StringBuilder();
			EmitOpenRoseHeader(sb);

			EmitHierarchyInline(root, sb, rootId, 1, baseUrl);

			// Build lookup dictionary from hierarchy
			var idToName = BuildIdToNameMap(root);

			// Emit trace links (dotted) with optional comments and optional labels
			foreach (var t in traces)
			{
				if (idToName.TryGetValue(t.FromTraceItemzId, out var fromName) &&
					idToName.TryGetValue(t.ToTraceItemzId, out var toName))
				{
					// Apply TransformLabelForMermaid to both sides for the comment
					var fromLabel = TransformLabelForMermaid(fromName);
					var toLabel = TransformLabelForMermaid(toName);

					sb.AppendLine($"    %% {fromLabel} -.-> {toLabel}");
				}

				// If a TraceLabel exists include it on the edge (sanitized & truncated)
				if ( (!string.IsNullOrWhiteSpace(t.TraceLabel)) && (t.TraceLabel != Sentinel.TraceLabelDefault) )
				{
					var edgeLabel = SanitizeLabelForMermaid(t.TraceLabel);
					// Use |label| notation between dashes to show a label on the edge.
					sb.AppendLine($"    {t.FromTraceItemzId} -.{edgeLabel}.-> {t.ToTraceItemzId}");
				}
				else
				{
					sb.AppendLine($"    {t.FromTraceItemzId} -.-> {t.ToTraceItemzId}");
				}
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
		public static string GenerateBaseline(
			NestedBaselineHierarchyIdRecordDetailsDTO root,
			IEnumerable<BaselineItemzTraceDTO> traces,
			Guid rootId,
			string? baseUrl = null)
		{
			var sb = new StringBuilder();
			EmitOpenRoseHeader(sb);

			EmitBaselineHierarchyInline(root, sb, rootId, 1, baseUrl);

			// Build lookup dictionary from baseline hierarchy
			var idToName = BuildBaselineIdToNameMap(root);
			// Emit trace links (dotted) with optional comments and labels
			foreach (var t in traces)
			{
				if (idToName.TryGetValue(t.FromTraceBaselineItemzId, out var fromName) &&
					idToName.TryGetValue(t.ToTraceBaselineItemzId, out var toName))
				{
					// Apply TransformLabelForMermaid to both sides for the comment
					var fromLabel = TransformLabelForMermaid(fromName);
					var toLabel = TransformLabelForMermaid(toName);
					sb.AppendLine($"    %% {fromLabel} -.-> {toLabel}");
				}

				// Add the edge, including label if present
				if ((!string.IsNullOrWhiteSpace(t.TraceLabel)) && (t.TraceLabel != Sentinel.TraceLabelDefault))
				{
					var edgeLabel = SanitizeLabelForMermaid(t.TraceLabel);
					sb.AppendLine($"    {t.FromTraceBaselineItemzId} -.{edgeLabel}.-> {t.ToTraceBaselineItemzId}");
				}
				else
				{
					sb.AppendLine($"    {t.FromTraceBaselineItemzId} -.-> {t.ToTraceBaselineItemzId}");
				}
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
												int indentLevel,
												string? baseUrl = null)
		{
			string parent = RenderNode(node.RecordId, node.RecordType, node.Name, node.RecordId == rootId);
			string indent = new string(' ', indentLevel * 4);

			if (node.RecordId == rootId)
			{
				sb.AppendLine($"{indent}{parent}");
				if (!string.IsNullOrWhiteSpace(baseUrl))
				{
					var gotoUrl = $"{baseUrl}/GoTo/{node.RecordId}";
					var safeLabel = TransformLabelForMermaid(node.Name);
					sb.AppendLine($"{indent}click {node.RecordId} href \"{gotoUrl}\"");
				}
			}

			if (node.Children != null && node.Children.Count > 0)
			{
				foreach (var child in node.Children)
				{
					string childText = RenderNode(child.RecordId, child.RecordType, child.Name, child.RecordId == rootId);
					sb.AppendLine($"{indent}{parent} --> {childText}");

					// Add click directive if baseUrl is provided
					if (!string.IsNullOrWhiteSpace(baseUrl))
					{
						var gotoUrl = $"{baseUrl}/GoTo/{child.RecordId}";
						var safeLabel = TransformLabelForMermaid(child.Name);
						sb.AppendLine($"{indent}click {child.RecordId} href \"{gotoUrl}\"");
					}

					EmitHierarchyInline(child, sb, rootId, indentLevel + 1, baseUrl);
				}
			}
		}

		/// <summary>
		/// Recursively emits nodes and edges for a baseline hierarchy into the StringBuilder.
		/// </summary>
		private static void EmitBaselineHierarchyInline(NestedBaselineHierarchyIdRecordDetailsDTO node,
														StringBuilder sb,
														Guid rootId,
														int indentLevel,
														string? baseUrl = null)
		{
			string parent = RenderNode(node.RecordId, node.RecordType, node.Name, node.RecordId == rootId);
			string indent = new string(' ', indentLevel * 4);

			// Root node
			if (node.RecordId == rootId)
			{
				sb.AppendLine($"{indent}{parent}");

				if (!string.IsNullOrWhiteSpace(baseUrl))
				{
					var gotoUrl = $"{baseUrl}/GoTo/{node.RecordId}";
					sb.AppendLine($"{indent}click {node.RecordId} href \"{gotoUrl}\"");
				}
			}

			// Children
			if (node.Children != null && node.Children.Count > 0)
			{
				foreach (var child in node.Children)
				{
					string childText = RenderNode(child.RecordId, child.RecordType, child.Name, child.RecordId == rootId);
					sb.AppendLine($"{indent}{parent} --> {childText}");

					if (!string.IsNullOrWhiteSpace(baseUrl))
					{
						var gotoUrl = $"{baseUrl}/GoTo/{child.RecordId}";
						sb.AppendLine($"{indent}click {child.RecordId} href \"{gotoUrl}\"");
					}

					EmitBaselineHierarchyInline(child, sb, rootId, indentLevel + 1, baseUrl);
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

		/// <summary>
		/// Prepare a trace label for use as an edge label in Mermaid flows.
		/// This returns an unquoted string where characters that break Mermaid are replaced.
		/// The returned label is trimmed and truncated to 32 chars (if longer).
		/// </summary>
		/// <param name="input">Raw trace label.</param>
		/// <returns>Sanitized, unquoted label suitable for using inside |label| in Mermaid edges.</returns>
		private static string SanitizeLabelForMermaid(string? input)
		{
			if (string.IsNullOrWhiteSpace(input)) return string.Empty;


			//TODO :: We should move Normalize Tracelabel  to a helper method
			//		  so that it can be reused in other places as well.
			// Trim and truncate to 32 characters (same limit as DB/schema)
			var label = input.Trim();
			if (label.Length > 32)
			{
				label = label.Substring(0, 32);
			}

			label = TransformLabelForMermaid(label);

			// Trim again in case replacements added spaces
			return label.Trim();
		}

		#region Itemz lookup dictionary

		/// <summary>
		/// Builds a lookup dictionary mapping Itemz hierarchy node IDs to their display names.
		/// </summary>
		/// <remarks>
		/// - Starts traversal from the given root node.
		/// - Recursively walks the hierarchy using <see cref="TraverseHierarchy"/>.
		/// - Ensures each Guid is mapped to a non-null string (empty if name is missing).
		/// </remarks>
		/// <param name="root">Root node of the Itemz hierarchy.</param>
		/// <returns>
		/// Dictionary where keys are node GUIDs and values are node names.
		/// </returns>
		private static Dictionary<Guid, string> BuildIdToNameMap(NestedHierarchyIdRecordDetailsDTO root)
		{
			var map = new Dictionary<Guid, string>();
			TraverseHierarchy(root, map);
			return map;
		}

		/// <summary>
		/// Recursively traverses an Itemz hierarchy to populate the ID-to-name dictionary.
		/// </summary>
		/// <remarks>
		/// - Adds the current node’s Guid and name if not already present.
		/// - Recursively processes all child nodes.
		/// - Guarantees that every reachable node is included in the dictionary.
		/// </remarks>
		/// <param name="node">Current hierarchy node being visited.</param>
		/// <param name="map">Dictionary being populated with node IDs and names.</param>

		private static void TraverseHierarchy(NestedHierarchyIdRecordDetailsDTO node, Dictionary<Guid, string> map)
		{
			if (!map.ContainsKey(node.RecordId))
			{
				map[node.RecordId] = node.Name ?? string.Empty;
			}

			if (node.Children != null)
			{
				foreach (var child in node.Children)
				{
					TraverseHierarchy(child, map);
				}
			}
		}

		#endregion

		#region Baseline lookup dictionary
		/// <summary>
		/// Builds a lookup dictionary mapping baseline hierarchy node IDs to their display names.
		/// </summary>
		/// <remarks>
		/// - Starts traversal from the given baseline root node.
		/// - Recursively walks the baseline hierarchy using <see cref="TraverseBaselineHierarchy"/>.
		/// - Ensures each Guid is mapped to a non-null string (empty if name is missing).
		/// </remarks>
		/// <param name="root">Root node of the baseline hierarchy.</param>
		/// <returns>
		/// Dictionary where keys are baseline node GUIDs and values are baseline node names.
		/// </returns>

		private static Dictionary<Guid, string> BuildBaselineIdToNameMap(NestedBaselineHierarchyIdRecordDetailsDTO root)
		{
			var map = new Dictionary<Guid, string>();
			TraverseBaselineHierarchy(root, map);
			return map;
		}


		/// <summary>
		/// Recursively traverses a baseline hierarchy to populate the ID-to-name dictionary.
		/// </summary>
		/// <remarks>
		/// - Adds the current baseline node’s Guid and name if not already present.
		/// - Recursively processes all child nodes.
		/// - Guarantees that every reachable baseline node is included in the dictionary.
		/// </remarks>
		/// <param name="node">Current baseline hierarchy node being visited.</param>
		/// <param name="map">Dictionary being populated with baseline node IDs and names.</param>

		private static void TraverseBaselineHierarchy(NestedBaselineHierarchyIdRecordDetailsDTO node,
													  Dictionary<Guid, string> map)
		{
			if (!map.ContainsKey(node.RecordId))
			{
				map[node.RecordId] = node.Name ?? string.Empty;
			}

			if (node.Children != null)
			{
				foreach (var child in node.Children)
				{
					TraverseBaselineHierarchy(child, map);
				}
			}
		}

		#endregion

		private static void EmitOpenRoseHeader(StringBuilder sb)
		{
			sb.AppendLine("flowchart TD");
			sb.AppendLine("  OpenRoseIcon@{ img: \"https://github.com/OpenRose/OpenRose/blob/main/OpenRose.Web/OpenRose.WebUI/wwwroot/icons/OpenRose_Mermaid.png?raw=true\", label: \"By OpenRose\", pos: \"b\", h: 60, constraint: \"on\" }");
			sb.AppendLine("  click OpenRoseIcon href \"https://github.com/OpenRose\" \"OpenRose\" _blank");
		}

	}
}