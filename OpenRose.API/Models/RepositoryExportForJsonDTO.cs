// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.


using System;
using System.Collections.Generic;
using ItemzApp.API.Models;

namespace ItemzApp.API.Models
{
	public class RepositoryExportForJsonDTO
	{
		public Guid RepositoryId { get; set; }

		// Project-based export
		public List<ProjectExportNode>? Projects { get; set; }
		public List<ItemzTypeExportNode>? ItemzTypes { get; set; }
		public List<ItemzExportNode>? Itemz { get; set; }

		// Trace links (for live Itemz)
		public List<ItemzTraceExportNodeDTO>? ItemzTraces { get; set; }

		// Baseline-based export
		public List<BaselineExportNode>? Baselines { get; set; }
		public List<BaselineItemzTypeExportNode>? BaselineItemzTypes { get; set; }
		public List<BaselineItemzExportNode>? BaselineItemz { get; set; }

		// Trace links (for Baseline Itemz)
		public List<BaselineItemzTraceExportNodeDTO>? BaselineItemzTraces { get; set; }
	}

	// Project hierarchy
	public class ProjectExportNode
	{
		public GetProjectDTO Project { get; set; }
		public List<ItemzTypeExportNode>? ItemzTypes { get; set; }
	}
	public class ItemzTypeExportNode
	{
		public GetItemzTypeDTO ItemzType { get; set; }
		public List<ItemzExportNode>? Itemz { get; set; }
	}
	public class ItemzExportNode
	{
		public GetItemzDTO Itemz { get; set; }
		public List<ItemzExportNode>? SubItemz { get; set; }
	}

	// Baseline hierarchy (parallel to Project)
	public class BaselineExportNode
	{
		public GetBaselineDTO Baseline { get; set; }
		public List<BaselineItemzTypeExportNode>? BaselineItemzTypes { get; set; }
	}
	public class BaselineItemzTypeExportNode
	{
		public GetBaselineItemzTypeDTO BaselineItemzType { get; set; }
		public List<BaselineItemzExportNode>? BaselineItemz { get; set; }
	}
	public class BaselineItemzExportNode
	{
		public GetBaselineItemzDTO BaselineItemz { get; set; }
		public List<BaselineItemzExportNode>? BaselineSubItemz { get; set; }
	}
}