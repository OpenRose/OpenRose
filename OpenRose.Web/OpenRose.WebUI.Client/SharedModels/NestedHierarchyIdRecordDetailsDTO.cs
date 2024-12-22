// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Collections.Generic;

namespace OpenRose.WebUI.Client.SharedModels
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

        public List<NestedHierarchyIdRecordDetailsDTO>? Children { get; set; }

	}
}
