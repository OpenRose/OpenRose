// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.


using System;
using System.Collections.Generic;
using ItemzApp.API.Models;

namespace ItemzApp.API.Models
{

	// TODO:: We just introduced a separate class called as RepositoryExportForJsonDTO. 
	// Following RepositoryExportDTO is now only supposed to be used for importing data. 
	// In OpenRose, we now have a small difference between export and import DTOs because
	// we want to include 'FromBaselineItemzName' and 'ToBaselineItemzName' in the export DTO
	// And while we import data then we do not use values for 'FromBaselineItemzName'
	// and 'ToBaselineItemzName' at all. This is why it wil be ignored during import.
	// At the time of writing this comment, overall, this was the main differenciater 
	// for which we had to create a separate class for export purpose.
	// Now we should rename RepositoryExportDTO to RepositoryImportDTO along with all
	// sub classes and variables used in it to avoid confusion.
	public class RepositoryExportDTO
	{
		public Guid RepositoryId { get; set; }

		// Project-based export
		public List<ProjectExportNode>? Projects { get; set; }
		public List<ItemzTypeImportNode>? ItemzTypes { get; set; }
		public List<ItemzImportNode>? Itemz { get; set; }

		// Trace links (for live Itemz)
		public List<ItemzTraceDTO>? ItemzTraces { get; set; }

		// Baseline-based export
		public List<BaselineExportNode>? Baselines { get; set; }
		public List<BaselineItemzTypeExportNode>? BaselineItemzTypes { get; set; }
		public List<BaselineItemzExportNode>? BaselineItemz { get; set; }

		// Trace links (for Baseline Itemz)
		public List<BaselineItemzTraceDTO>? BaselineItemzTraces { get; set; }
	}

	// Project hierarchy
	public class ProjectExportNode
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