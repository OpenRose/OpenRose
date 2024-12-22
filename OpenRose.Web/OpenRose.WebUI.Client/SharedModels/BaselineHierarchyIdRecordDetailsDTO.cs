// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;

namespace OpenRose.WebUI.Client.SharedModels
{
	public class BaselineHierarchyIdRecordDetailsDTO
	{
		/// <summary>
		/// Parent Record ID representated by a GUID.
		/// </summary>
		public Guid RecordId { get; set; }

		/// <summary>
		/// Baseline Hierarchy ID in string format for RecordId e.g. "/3/2/1"
		/// </summary>
		public string? BaselineHierarchyId { get; set; }

		/// <summary>
		/// Source Itemz Hierarchy ID in string format for RecordId e.g. "/3/2/1"
		/// </summary>
		public string? SourceHierarchyId { get; set; }

		/// <summary>
		/// Baseline Hierarchy Level for RecordId
		/// </summary>
		public int? Level { get; set; }

		/// <summary>
		/// Record Type within Baseline Hierarchy for RecordId
		/// </summary>
		public string? RecordType { get; set; }

		/// <summary>
		/// Name of the Baseline Hierarchy Record
		/// </summary>
		public string? Name { get; set; }

		/// <summary>
		/// Baseline Hierarchy Id for the TOP Baseline Hierarchy Record within a given Parent Baseline Hierarchy Record
		/// </summary>
		public string? TopChildBaselineHierarchyId { get; set; } = "";

		/// <summary>
		/// Baseline Hierarchy Id for the BOTTOM Baseline Hierarchy Record within a given Parent Baseline Hierarchy Record
		/// </summary>
		public string? BottomChildBaselineHierarchyId { get; set; } = "";

		/// <summary>
		/// Total Number of Child nodes under RecordId
		/// </summary>
		public int? NumberOfChildNodes { get; set; } = 0;

		/// <summary>
		/// TRUE if included in the baseline and FALSE if excluded from the baseline. 
		/// </summary>
		public bool IsIncluded { get; set; }


		/// <summary>
		/// Parent Baseline Itemz Record ID representated by a GUID.
		/// </summary>
		public Guid ParentRecordId { get; set; }

		/// <summary>
		/// Hierarchy ID in string format for ParentRecordId e.g. "/3/2/1"
		/// </summary>
		public string? ParentBaselineHierarchyId { get; set; } = "";

		/// <summary>
		/// Hierarchy Level for Parent Baseline Itemz RecordId
		/// </summary>
		public int? ParentLevel { get; set; } = 0;

		/// <summary>
		/// Record Type within Hierarchy for Parent Baseline Itemz RecordId
		/// </summary>
		public string? ParentRecordType { get; set; } = "";

		/// <summary>
		/// Parent Name of the Baseline Hierarchy Record
		/// </summary>
		public string? ParentName { get; set; }

		/// <summary>
		/// Is Parent Baseline Hierarchy Record Included or Excluded?
		/// </summary>
		public bool ParentIsIncluded { get; set; }
	}
}
