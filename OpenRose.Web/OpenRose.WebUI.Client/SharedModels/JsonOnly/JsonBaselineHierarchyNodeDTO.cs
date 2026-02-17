// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

namespace OpenRose.WebUI.Client.SharedModels.JsonOnly
{
	public class JsonBaselineHierarchyNodeDTO
	{
		public Guid RecordId { get; set; }
		public string? Name { get; set; }
		public string? RecordType { get; set; }
		public string? HierarchyId { get; set; }
		public int Level { get; set; }

		// NEW — only for JSON mode
		public bool IsIncluded { get; set; }

		public List<JsonBaselineHierarchyNodeDTO> Children { get; set; } = new();
	}
}
