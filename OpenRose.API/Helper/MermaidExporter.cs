// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using ItemzApp.API.Constants;
using ItemzApp.API.Models;
using ItemzApp.API.Services;
using System;
using System.Collections.Generic;
using System.Text;

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
								  string? baseUrl,
								  bool includeEstimations = false,
								  bool includeTags = false,
								  string? view = null,
								  IItemzRepository? itemzRepository = null)
		{
			var sb = new StringBuilder();
			EmitOpenRoseHeader(sb);

			EmitHierarchyInline(root, sb, rootId, 1, baseUrl, includeEstimations, includeTags, view, itemzRepository);

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
			string? baseUrl = null,
			bool includeEstimations = false,
			bool includeTags = false,
			string? view = null,
			IBaselineItemzRepository? baselineItemzRepository = null)
		{
			var sb = new StringBuilder();
			EmitOpenRoseHeader(sb);

			EmitBaselineHierarchyInline(root, sb, rootId, 1, baseUrl, includeEstimations, includeTags, view, baselineItemzRepository);

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
		//private static void EmitHierarchyInline(NestedHierarchyIdRecordDetailsDTO node,
		//										StringBuilder sb,
		//										Guid rootId,
		//										int indentLevel,
		//										string? baseUrl = null,
		//										bool includeEstimations = false,
		//										bool includeTags = false,
		//										string? view = null,
		//										IItemzRepository? itemzRepository = null)
		//{
		//	string parent = string.Empty;

		//	if (includeEstimations)
		//	{
		//		 parent = RenderNodeWithEstimation(node.RecordId, node.RecordType, node.Name, node.RecordId == rootId
		//			, node.EstimationUnit, node.RolledUpEstimation);
		//	}
		//	else
		//	{
		//		parent = RenderNode(node.RecordId, node.RecordType, node.Name, node.RecordId == rootId);
		//	}

		//	string indent = new string(' ', indentLevel * 4);

		//	if (node.RecordId == rootId)
		//	{
		//		sb.AppendLine($"{indent}{parent}");
		//		if (!string.IsNullOrWhiteSpace(baseUrl))
		//		{
		//			//var gotoUrl = $"{baseUrl}/GoTo/{node.RecordId}";
		//			var gotoUrl = BuildGotoUrl(baseUrl, node.RecordId, view);
		//			var safeLabel = TransformLabelForMermaid(node.Name);
		//			sb.AppendLine($"{indent}click {node.RecordId} href \"{gotoUrl}\"");
		//		}
		//	}

		//	if (node.Children != null && node.Children.Count > 0)
		//	{
		//		foreach (var child in node.Children)
		//		{
		//			string childText = string.Empty;

		//			if (includeEstimations)
		//			{
		//				childText = RenderNodeWithEstimation(child.RecordId, child.RecordType, child.Name, child.RecordId == rootId
		//				, child.EstimationUnit, child.RolledUpEstimation);
		//			}
		//			else
		//			{
		//				childText = RenderNode(child.RecordId, child.RecordType, child.Name, child.RecordId == rootId);
		//			}

		//			// If includeTags is true and this is an Itemz record, fetch and append tags
		//			if (includeTags && child.RecordType == "Itemz" && itemzRepository != null)
		//			{
		//				var tagsMarkup = GetTagsMarkupAsync(child.RecordId, itemzRepository).Result;
		//				if (!string.IsNullOrWhiteSpace(tagsMarkup))
		//				{
		//					// Reconstruct the child node text with tags appended
		//					childText = RenderNodeWithTags(child.RecordId, child.RecordType, child.Name,
		//						child.RecordId == rootId, includeEstimations,
		//						child.EstimationUnit, child.RolledUpEstimation, tagsMarkup);
		//				}
		//			}

		//			sb.AppendLine($"{indent}{parent} --> {childText}");

		//			// Add click directive if baseUrl is provided
		//			if (!string.IsNullOrWhiteSpace(baseUrl))
		//			{
		//				//var gotoUrl = $"{baseUrl}/GoTo/{child.RecordId}";
		//				var gotoUrl = BuildGotoUrl(baseUrl, child.RecordId, view);
		//				var safeLabel = TransformLabelForMermaid(child.Name);
		//				sb.AppendLine($"{indent}click {child.RecordId} href \"{gotoUrl}\"");
		//			}

		//			EmitHierarchyInline(child, sb, rootId, indentLevel + 1, baseUrl, includeEstimations, includeTags, view, itemzRepository);
		//		}
		//	}
		//}

		private static void EmitHierarchyInline(NestedHierarchyIdRecordDetailsDTO node,
												StringBuilder sb,
												Guid rootId,
												int indentLevel,
												string? baseUrl = null,
												bool includeEstimations = false,
												bool includeTags = false,
												string? view = null,
												IItemzRepository? itemzRepository = null)
		{
			// First pass: Define all nodes with their complete information (including tags)
			DefineNodeRecursive(node, sb, rootId, includeEstimations, includeTags, itemzRepository);

			// Second pass: Add all click directives
			if (!string.IsNullOrWhiteSpace(baseUrl))
			{
				sb.AppendLine();
				sb.AppendLine("    %% Click directives");
				AddClickDirectivesRecursive(node, sb, baseUrl, view);
			}

			// Third pass: Add all links
			sb.AppendLine();
			sb.AppendLine("    %% Links");
			EmitLinksRecursive(node, sb);
		}

		/// <summary>
		/// First pass: Recursively define all nodes with their complete information including tags.
		/// </summary>
		private static void DefineNodeRecursive(NestedHierarchyIdRecordDetailsDTO node,
												StringBuilder sb,
												Guid rootId,
												bool includeEstimations = false,
												bool includeTags = false,
												IItemzRepository? itemzRepository = null)
		{
			string nodeDefinition = string.Empty;

			if (includeEstimations)
			{
				nodeDefinition = RenderNodeWithEstimation(node.RecordId, node.RecordType, node.Name, node.RecordId == rootId
					, node.EstimationUnit, node.RolledUpEstimation);
			}
			else
			{
				nodeDefinition = RenderNode(node.RecordId, node.RecordType, node.Name, node.RecordId == rootId);
			}

			// If includeTags is true and this is an Itemz record, fetch and append tags
			if (includeTags && node.RecordType == "Itemz" && itemzRepository != null)
			{
				var tagsMarkup = GetTagsMarkupAsync(node.RecordId, itemzRepository).Result;
				if (!string.IsNullOrWhiteSpace(tagsMarkup))
				{
					// Reconstruct the node definition with tags appended
					nodeDefinition = RenderNodeWithTags(node.RecordId, node.RecordType, node.Name,
						node.RecordId == rootId, includeEstimations,
						node.EstimationUnit, node.RolledUpEstimation, tagsMarkup);
				}
			}

			sb.AppendLine($"    {nodeDefinition}");

			// Recursively define all children
			if (node.Children != null && node.Children.Count > 0)
			{
				foreach (var child in node.Children)
				{
					DefineNodeRecursive(child, sb, rootId, includeEstimations, includeTags, itemzRepository);
				}
			}
		}

		/// <summary>
		/// Second pass: Recursively add click directives for all nodes.
		/// </summary>
		private static void AddClickDirectivesRecursive(NestedHierarchyIdRecordDetailsDTO node,
														StringBuilder sb,
														string? baseUrl,
														string? view)
		{
			var gotoUrl = BuildGotoUrl(baseUrl, node.RecordId, view);
			sb.AppendLine($"    click {node.RecordId} href \"{gotoUrl}\"");

			// Recursively add click directives for all children
			if (node.Children != null && node.Children.Count > 0)
			{
				foreach (var child in node.Children)
				{
					AddClickDirectivesRecursive(child, sb, baseUrl, view);
				}
			}
		}

		/// <summary>
		/// Third pass: Recursively emit all links between parent and child nodes.
		/// </summary>
		private static void EmitLinksRecursive(NestedHierarchyIdRecordDetailsDTO node,
												StringBuilder sb)
		{
			if (node.Children != null && node.Children.Count > 0)
			{
				foreach (var child in node.Children)
				{
					sb.AppendLine($"    {node.RecordId} --> {child.RecordId}");
					EmitLinksRecursive(child, sb);
				}
			}
		}

		/// <summary>
		/// Recursively emits nodes and edges for a baseline hierarchy into the StringBuilder.
		/// </summary>
		//private static void EmitBaselineHierarchyInline(NestedBaselineHierarchyIdRecordDetailsDTO node,
		//												StringBuilder sb,
		//												Guid rootId,
		//												int indentLevel,
		//												string? baseUrl = null,
		//												bool includeEstimations = false,
		//												bool includeTags = false,
		//												string? view = null,
		//												IItemzRepository? itemzRepository = null)
		//{
		//	string parent = string.Empty;

		//	if (includeEstimations)
		//	{
		//		parent = RenderNodeWithEstimation(node.RecordId, node.RecordType, node.Name, node.RecordId == rootId
		//						, node.EstimationUnit, node.RolledUpEstimation);
		//	}
		//	else 
		//	{
		//		parent = RenderNode(node.RecordId, node.RecordType, node.Name, node.RecordId == rootId);

		//	}
		//	string indent = new string(' ', indentLevel * 4);

		//	// Root node
		//	if (node.RecordId == rootId)
		//	{
		//		sb.AppendLine($"{indent}{parent}");

		//		if (!string.IsNullOrWhiteSpace(baseUrl))
		//		{
		//			//var gotoUrl = $"{baseUrl}/GoTo/{node.RecordId}";
		//			var gotoUrl = BuildGotoUrl(baseUrl, node.RecordId, view);
		//			sb.AppendLine($"{indent}click {node.RecordId} href \"{gotoUrl}\"");
		//		}
		//	}

		//	// Children
		//	if (node.Children != null && node.Children.Count > 0)
		//	{
		//		foreach (var child in node.Children)
		//		{

		//			string childText = string.Empty;

		//			if(includeEstimations)
		//			{
		//				childText = RenderNodeWithEstimation(child.RecordId, child.RecordType, child.Name, child.RecordId == rootId
		//				, child.EstimationUnit, child.RolledUpEstimation);
		//			}
		//			else
		//			{
		//				 childText = RenderNode(child.RecordId, child.RecordType, child.Name, child.RecordId == rootId);
		//			}

		//			sb.AppendLine($"{indent}{parent} --> {childText}");

		//			if (!string.IsNullOrWhiteSpace(baseUrl))
		//			{
		//				//var gotoUrl = $"{baseUrl}/GoTo/{child.RecordId}";
		//				var gotoUrl = BuildGotoUrl(baseUrl, child.RecordId, view);
		//				sb.AppendLine($"{indent}click {child.RecordId} href \"{gotoUrl}\"");
		//			}

		//			EmitBaselineHierarchyInline(child, sb, rootId, indentLevel + 1, baseUrl, includeEstimations, includeTags, view, itemzRepository);
		//		}
		//	}
		//}

		private static void EmitBaselineHierarchyInline(NestedBaselineHierarchyIdRecordDetailsDTO node,
												StringBuilder sb,
												Guid rootId,
												int indentLevel,
												string? baseUrl = null,
												bool includeEstimations = false,
												bool includeTags = false,
												string? view = null,
												IBaselineItemzRepository? baselineItemzRepository = null)
		{
			// First pass: Define all nodes with their complete information (including tags)
			DefineBaselineNodeRecursive(node, sb, rootId, includeEstimations, includeTags, baselineItemzRepository);

			// Second pass: Add all click directives
			if (!string.IsNullOrWhiteSpace(baseUrl))
			{
				sb.AppendLine();
				sb.AppendLine("    %% Click directives");
				AddBaselineClickDirectivesRecursive(node, sb, baseUrl, view);
			}

			// Third pass: Add all links
			sb.AppendLine();
			sb.AppendLine("    %% Links");
			EmitBaselineLinksRecursive(node, sb);
		}

		/// <summary>
		/// First pass: Recursively define all baseline nodes with their complete information including tags.
		/// </summary>
		private static void DefineBaselineNodeRecursive(NestedBaselineHierarchyIdRecordDetailsDTO node,
														StringBuilder sb,
														Guid rootId,
														bool includeEstimations = false,
														bool includeTags = false,
														IBaselineItemzRepository? baselineItemzRepository = null)
		{
			string nodeDefinition = string.Empty;

			if (includeEstimations)
			{
				nodeDefinition = RenderNodeWithEstimation(node.RecordId, node.RecordType, node.Name, node.RecordId == rootId
					, node.EstimationUnit, node.RolledUpEstimation);
			}
			else
			{
				nodeDefinition = RenderNode(node.RecordId, node.RecordType, node.Name, node.RecordId == rootId);
			}

			// If includeTags is true and this is a BaselineItemz record, fetch and append tags
			if (includeTags && node.RecordType == "BaselineItemz" && baselineItemzRepository != null)
			{
				var tagsMarkup = GetBaselineTagsMarkupAsync(node.RecordId, baselineItemzRepository).Result;
				if (!string.IsNullOrWhiteSpace(tagsMarkup))
				{
					// Reconstruct the node definition with tags appended
					nodeDefinition = RenderNodeWithTags(node.RecordId, node.RecordType, node.Name,
						node.RecordId == rootId, includeEstimations,
						node.EstimationUnit, node.RolledUpEstimation, tagsMarkup);
				}
			}

			sb.AppendLine($"    {nodeDefinition}");

			// Recursively define all children
			if (node.Children != null && node.Children.Count > 0)
			{
				foreach (var child in node.Children)
				{
					DefineBaselineNodeRecursive(child, sb, rootId, includeEstimations, includeTags, baselineItemzRepository);
				}
			}
		}

		/// <summary>
		/// Second pass: Recursively add click directives for all baseline nodes.
		/// </summary>
		private static void AddBaselineClickDirectivesRecursive(NestedBaselineHierarchyIdRecordDetailsDTO node,
															StringBuilder sb,
															string? baseUrl,
															string? view)
		{
			var gotoUrl = BuildGotoUrl(baseUrl, node.RecordId, view);
			sb.AppendLine($"    click {node.RecordId} href \"{gotoUrl}\"");

			// Recursively add click directives for all children
			if (node.Children != null && node.Children.Count > 0)
			{
				foreach (var child in node.Children)
				{
					AddBaselineClickDirectivesRecursive(child, sb, baseUrl, view);
				}
			}
		}

		/// <summary>
		/// Third pass: Recursively emit all links between parent and child baseline nodes.
		/// </summary>
		private static void EmitBaselineLinksRecursive(NestedBaselineHierarchyIdRecordDetailsDTO node,
													StringBuilder sb)
		{
			if (node.Children != null && node.Children.Count > 0)
			{
				foreach (var child in node.Children)
				{
					sb.AppendLine($"    {node.RecordId} --> {child.RecordId}");
					EmitBaselineLinksRecursive(child, sb);
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
			string label = TransformLabelForMermaid(name);

			return isRoot
				? $"{id}(({label}))"   // circle for root
				: $"{id}[{label}]";    // rectangle for others
		}

		/// <summary>
		/// Renders a node into Mermaid syntax with appropriate shape and label.
		/// Includes estimation unit and rolled-up estimation when available.
		/// </summary>
		/// <param name="id">Unique Guid identifier of the node.</param>
		/// <param name="recordType">Type of record (e.g., Project, ItemzType, Baseline).</param>
		/// <param name="name">Display name of the node.</param>
		/// <param name="isRoot">True if this node is the root, otherwise false.</param>
		/// <param name="estimationUnit">Estimation Unit (e.g., "GBP", "Hours"). If null, "N/A" is used.</param>
		/// <param name="rolledUpEstimation">Rolled-up estimation value for the node.</param>
		/// <returns>
		/// Mermaid syntax string for the node. Root nodes are circles, others are rectangles.
		/// Estimation information is appended on a new line using HTML line break.
		/// </returns>
		private static string RenderNodeWithEstimation(Guid id, string? recordType, string? name, bool isRoot,
			string? estimationUnit = null,
			decimal rolledUpEstimation = 0)
		{
			// Format estimation unit with fallback to "N/A"
			string estimationDisplay = string.IsNullOrWhiteSpace(estimationUnit)
				? "N/A"
				: estimationUnit;

			// Format the estimation line
			string estimationLine = $"<br/>{estimationDisplay} {rolledUpEstimation:F2}";

			string label = TransformLabelForMermaid($"{name}{estimationLine}");

			return isRoot
				? $"{id}(({label}))"   // circle for root
				: $"{id}[{label}]";    // rectangle for others
		}


		/// <summary>
		/// Renders a node into Mermaid syntax with estimation and tags.
		/// </summary>
		private static string RenderNodeWithTags(Guid id, string? recordType, string? name, bool isRoot,
			bool includeEstimations = false,
			string? estimationUnit = null,
			decimal rolledUpEstimation = 0,
			string? tagsMarkup = null)
		{
			// First, transform the base name and estimation
			var labelParts = new List<string> { TransformLabelForMermaid(name) };

			if (includeEstimations)
			{
				string estimationDisplay = string.IsNullOrWhiteSpace(estimationUnit) ? "N/A" : estimationUnit;
				labelParts.Add($"<br/>{estimationDisplay} {rolledUpEstimation:F2}");
			}

			if (!string.IsNullOrWhiteSpace(tagsMarkup))
			{
				labelParts.Add($"<br/>Tags:<br/>{tagsMarkup}");
			}

			string fullLabel = string.Concat(labelParts);

			return isRoot
				? $"{id}(({fullLabel}))"   // circle for root
				: $"{id}[{fullLabel}]";    // rectangle for others
		}
		
		/// <summary>
		/// Fetches tags for an Itemz record and formats them with HTML markup.
		/// Tags are stored as a pipe-delimited string and rendered as highlighted badges.
		/// </summary>
		/// <param name="itemzId">The ID of the Itemz record.</param>
		/// <param name="itemzRepository">Repository for fetching Itemz details.</param>
		/// <returns>A string containing HTML markup for tags, or null if no tags exist.</returns>
		private static async System.Threading.Tasks.Task<string?> GetTagsMarkupAsync(Guid itemzId, IItemzRepository itemzRepository)
		{
			try
			{
				var itemz = await itemzRepository.GetItemzAsync(itemzId);
				if (itemz == null || string.IsNullOrWhiteSpace(itemz.Tags))
				{
					return null;
				}

				// Tags are pipe-delimited; split and trim each tag
				var tags = itemz.Tags.Split('|');
				var tagMarkups = new List<string>();

				foreach (var tag in tags)
				{
					var trimmedTag = tag.Trim();
					if (!string.IsNullOrWhiteSpace(trimmedTag))
					{
						// Create a mark element with LIGHTGREEN background for each tag
						// Note: Using << >> instead of < > to preserve angle brackets in Mermaid output
						tagMarkups.Add($"<<mark style='background:LIGHTGREEN'>{trimmedTag}</mark>>");
					}
				}

				if (tagMarkups.Count == 0)
				{
					return null;
				}

				// Join all tags with line breaks
				return string.Join("<br/>", tagMarkups);
			}
			catch
			{
				// Gracefully handle any errors fetching tags
				return null;
			}
		}


		/// <summary>
		/// Fetches tags for a BaselineItemz record and formats them with HTML markup.
		/// Tags are stored as a pipe-delimited string and rendered as highlighted badges.
		/// </summary>
		/// <param name="baselineItemzId">The ID of the BaselineItemz record.</param>
		/// <param name="baselineItemzRepository">Repository for fetching BaselineItemz details.</param>
		/// <returns>A string containing HTML markup for tags, or null if no tags exist.</returns>
		private static async System.Threading.Tasks.Task<string?> GetBaselineTagsMarkupAsync(Guid baselineItemzId, IBaselineItemzRepository baselineItemzRepository)
		{
			try
			{
				var baselineItemz = await baselineItemzRepository.GetBaselineItemzAsync(baselineItemzId);
				if (baselineItemz == null || string.IsNullOrWhiteSpace(baselineItemz.Tags))
				{
					return null;
				}

				// Tags are pipe-delimited; split and trim each tag
				var tags = baselineItemz.Tags.Split('|');
				var tagMarkups = new List<string>();

				foreach (var tag in tags)
				{
					var trimmedTag = tag.Trim();
					if (!string.IsNullOrWhiteSpace(trimmedTag))
					{
						// Create a mark element with LIGHTGREEN background for each tag
						// Note: Using << >> instead of < > to preserve angle brackets in Mermaid output
						tagMarkups.Add($"<<mark style='background:LIGHTGREEN'>{trimmedTag}</mark>>");
					}
				}

				if (tagMarkups.Count == 0)
				{
					return null;
				}

				// Join all tags with line breaks
				return string.Join("<br/>", tagMarkups);
			}
			catch
			{
				// Gracefully handle any errors fetching tags
				return null;
			}
		}

		/// <summary>
		/// Transforms a raw label string into a Mermaid-safe format.
		/// Mermaid diagrams break if labels contain certain special characters,
		/// so we sanitize them by replacing or escaping problematic symbols.
		/// Preserves HTML line breaks (&lt;br/&gt;) for multi-line labels.
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
				.Replace(">", ")")    // replace > with )
				.Replace("%", "percent") // percent sign → word "percent"
				.Replace("end", "END")   // lowercase "end" → uppercase
				.Replace("[", "(")       // square bracket → parenthesis
				.Replace("]", ")")       // square bracket → parenthesis
				.Replace("{", "(")       // curly brace → parenthesis
				.Replace("}", ")");      // curly brace → parenthesis

			// Restore the <br/> tag for line breaks (was converted to "(br/)" above)
			// Mermaid requires <br/> for HTML line breaks in labels
			transformed = transformed.Replace("(br/)", "<br/>");

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

		private static string BuildGotoUrl(string? baseUrl, Guid recordId, string? view)
		{
			if (string.IsNullOrWhiteSpace(baseUrl))
				return $"GoTo/{recordId}";

			var url = $"{baseUrl}/GoTo/{recordId}";

			if (!string.IsNullOrWhiteSpace(view) &&
				view.Equals("treeview", StringComparison.OrdinalIgnoreCase))
			{
				url += "?view=treeview";
			}

			return url;
		}


	}
}