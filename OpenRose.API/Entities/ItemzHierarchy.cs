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
	public class ItemzHierarchy
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		[MaxLength(128)]
		public string? RecordType { get; set; }  // TODO: Make it possible to use predefined enum list instead of passing in text. 

		[MaxLength(128)]
		public string? Name { get; set; }

		public HierarchyId? ItemzHierarchyId { get; set; }

		/// <summary>
		/// Estimation Unit (e.g., "Days", "Hours", "Story Points", "$", "GBP")
		/// Maximum 16 characters for consistency across hierarchy
		/// </summary>
		[MaxLength(16)]
		public string? EstimationUnit { get; set; }

		/// <summary>
		/// Own Estimation value for this hierarchy record
		/// Decimal for precision (e.g., 10.5 days)
		/// </summary>
		public decimal OwnEstimation { get; set; } = 0;

		/// <summary>
		/// Roll-up Estimation - sum of own estimation plus all child estimations
		/// Calculated automatically based on hierarchy
		/// </summary>
		public decimal RolledUpEstimation { get; set; } = 0;

	}

}