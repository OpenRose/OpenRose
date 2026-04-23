// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Collections.Generic;

namespace ItemzApp.API.Models
{
	public class NestedHierarchyIdRecordDetailsDTO
	{
		/// <summary>
		/// Record ID representated by a GUID.
		/// </summary>
		public Guid RecordId { get; set; }

		/// <summary>
		/// Hierarchy ID in string format for RecordId e.g. "/3/2/1"
		/// </summary>
		public string? HierarchyId { get; set; }

		/// <summary>
		/// Hierarchy Level for RecordId
		/// </summary>
		public int? Level { get; set; }

		/// <summary>
		/// Record Type within Hierarchy for RecordId
		/// </summary>
		public string? RecordType { get; set; }

		/// <summary>
		/// Name of the Hierarchy Record
		/// </summary>
		public string? Name { get; set; }

		/// <summary>
		/// Estimation Unit (e.g., "Days", "Hours", "Story Points", "$", "GBP")
		/// Consistent across entire project hierarchy
		/// </summary>
		public string? EstimationUnit { get; set; }

		/// <summary>
		/// Own Estimation value for this hierarchy record
		/// </summary>
		public decimal OwnEstimation { get; set; } = 0;

		/// <summary>
		/// Roll-up Estimation - sum of own estimation plus all child estimations
		/// </summary>
		public decimal RolledUpEstimation { get; set; } = 0;
		public List<NestedHierarchyIdRecordDetailsDTO>? Children { get; set; }

	}
}