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


		public List<NestedBaselineHierarchyIdRecordDetailsDTO>? Children { get; set; }

	}
}
