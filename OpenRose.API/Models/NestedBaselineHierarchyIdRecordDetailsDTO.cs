// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Collections.Generic;

namespace ItemzApp.API.Models
{
    public class NestedBaselineHierarchyIdRecordDetailsDTO
    {
        /// <summary>
        /// Baseline Hierarchy Record ID representated by a GUID.
        /// </summary>
        public Guid RecordId { get; set; }

        /// <summary>
        /// Baseline Hierarchy ID in string format for RecordId e.g. "/3/2/1"
        /// </summary>
        public string? BaselineHierarchyId { get; set; }

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
        /// Indicates if Baseline Hierarchy record is included or excluded
        /// </summary>
        public bool isIncluded { get; set; }

		// PHASE 1: Estimation fields
		/// <summary>
		/// Estimation Unit (e.g., "Days", "Hours", "Story Points", "$", "GBP")
		/// </summary>
		public string? EstimationUnit { get; set; }

		/// <summary>
		/// Own Estimation value for this baseline hierarchy record
		/// </summary>
		public decimal OwnEstimation { get; set; } = 0;

		/// <summary>
		/// Roll-up Estimation value
		/// If isIncluded = false, this will be 0 in the API response
		/// </summary>
		public decimal RolledUpEstimation { get; set; } = 0;

		public List<NestedBaselineHierarchyIdRecordDetailsDTO>? Children { get; set; }

	}
}
