// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItemzApp.API.Entities
{
    public class BaselineItemzHierarchy
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(128)]
        public string? RecordType { get; set; }  // TODO: Make it possible to use predefined enum list instead of passing in text. 

		[MaxLength(128)]
		public string? Name { get; set; }
		public HierarchyId? BaselineItemzHierarchyId { get; set; }

        public HierarchyId? SourceItemzHierarchyId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public bool isIncluded { get; set; } = true;

		/// <summary>
		/// Estimation Unit (e.g., "Days", "Hours", "Story Points", "$", "GBP")
		/// Maximum 16 characters for consistency across hierarchy
		/// Copied from ItemzHierarchy at baseline creation time
		/// </summary>
		[MaxLength(16)]
		public string? EstimationUnit { get; set; }

		/// <summary>
		/// Own Estimation value for this baseline hierarchy record
		/// Decimal for precision (e.g., 10.5 days)
		/// Copied from ItemzHierarchy at baseline creation time
		/// Never changes during baseline lifecycle (baseline is immutable)
		/// </summary>
		public decimal OwnEstimation { get; set; } = 0;

		/// <summary>
		/// Roll-up Estimation - sum of own estimation plus all child estimations
		/// Calculated based on isIncluded flag of child records
		/// Updated when inclusion/exclusion status changes
		/// When isIncluded = false, this is set to 0 for read operations
		/// </summary>
		public decimal RolledUpEstimation { get; set; } = 0;
	}
}
