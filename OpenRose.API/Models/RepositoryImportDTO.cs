// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.


using System;
using System.Collections.Generic;
using ItemzApp.API.Models;

namespace ItemzApp.API.Models
{
	public class RepositoryImportDTO
	{
		public Guid RepositoryId { get; set; }

		// Project-based import
		public List<ProjectImportNode>? Projects { get; set; }
		public List<ItemzTypeImportNode>? ItemzTypes { get; set; }
		public List<ItemzImportNode>? Itemz { get; set; }

		// Trace links (for live Itemz)
		public List<ItemzTraceDTO>? ItemzTraces { get; set; }

		// Baseline-based import
		public List<BaselineImportNode>? Baselines { get; set; }
		public List<BaselineItemzTypeImportNode>? BaselineItemzTypes { get; set; }
		public List<BaselineItemzImportNode>? BaselineItemz { get; set; }

		// Trace links (for Baseline Itemz)
		public List<BaselineItemzTraceDTO>? BaselineItemzTraces { get; set; }
	}

	// Project hierarchy
	public class ProjectImportNode
	{
		public GetProjectDTO Project { get; set; }
		public List<ItemzTypeImportNode>? ItemzTypes { get; set; }
	}
	public class ItemzTypeImportNode
	{
		public GetItemzTypeDTO ItemzType { get; set; }
		public List<ItemzImportNode>? Itemz { get; set; }
	}
	public class ItemzImportNode
	{
		public GetItemzDTO Itemz { get; set; }
		public List<ItemzImportNode>? SubItemz { get; set; }
	}

	// Baseline hierarchy (parallel to Project)
	public class BaselineImportNode
	{
		public GetBaselineDTO Baseline { get; set; }
		public List<BaselineItemzTypeImportNode>? BaselineItemzTypes { get; set; }
	}
	public class BaselineItemzTypeImportNode
	{
		public GetBaselineItemzTypeDTO BaselineItemzType { get; set; }
		public List<BaselineItemzImportNode>? BaselineItemz { get; set; }
	}
	public class BaselineItemzImportNode
	{
		public GetBaselineItemzDTO BaselineItemz { get; set; }
		public List<BaselineItemzImportNode>? BaselineSubItemz { get; set; }
	}
}