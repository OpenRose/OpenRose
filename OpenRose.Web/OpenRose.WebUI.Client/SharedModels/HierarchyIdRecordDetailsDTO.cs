// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;

namespace OpenRose.WebUI.Client.SharedModels
{
	public class HierarchyIdRecordDetailsDTO
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
		/// Hierarchy Id for the TOP Hierarchy Record within a given Parent Hierarchy Record
		/// </summary>
		public string? TopChildHierarchyId { get; set; } = "";

		/// <summary>
		/// Hierarchy Id for the BOTTOM Hierarchy Record within a given Parent Hierarchy Record
		/// </summary>
		public string? BottomChildHierarchyId { get; set; } = "";

		/// <summary>
		/// Total Number of Child nodes under RecordId
		/// </summary>
		public int? NumberOfChildNodes { get; set; } = 0;

		/// <summary>
		/// Parent Record ID representated by a GUID.
		/// </summary>
		public Guid ParentRecordId { get; set; }

		/// <summary>
		/// Hierarchy ID in string format for ParentRecordId e.g. "/3/2/1"
		/// </summary>
		public string? ParentHierarchyId { get; set; } = "";

		/// <summary>
		/// Hierarchy Level for ParentRecordId
		/// </summary>
		public int? ParentLevel { get; set; } = 0;

		/// <summary>
		/// Record Type within Hierarchy for ParentRecordId
		/// </summary>
		public string? ParentRecordType { get; set; } = "";

		/// <summary>
		/// Parent Name of the Hierarchy Record
		/// </summary>
		public string? ParentName { get; set; }
	}
}
