// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;

namespace ItemzApp.API.Models
{
	public class GoToResolutionDTO
	{
		public Guid RecordId { get; set; }
		public string RecordType { get; set; }
		public string Name { get; set; }
		public string RecordHierarchyId { get; set; }
		public int RecordHierarchyLevel { get; set; }
		public Guid? ProjectId { get; set; }
		public string ProjectName { get; set; }
		public string ProjectHierarchyId { get; set; }
		public int ProjectHierarchyLevel { get; set; }
	}
}
