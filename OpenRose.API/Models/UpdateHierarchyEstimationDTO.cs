// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.ComponentModel.DataAnnotations;

namespace ItemzApp.API.Models
{
	/// <summary>
	/// PHASE 1: DTO for updating estimation-related fields on hierarchy records
	/// Used when user updates own estimation or estimation unit via API
	/// </summary>
	public class UpdateHierarchyEstimationDTO
	{
		/// <summary>
		/// The ID of the hierarchy record being updated
		/// </summary>
		[Required]
		public Guid RecordId { get; set; }

		/// <summary>
		/// New estimation unit (e.g., "Days", "Hours", "Story Points", "$")
		/// Maximum 16 characters as per requirements
		/// Optional - only update if provided
		/// </summary>
		[MaxLength(16)]
		public string? EstimationUnit { get; set; }

		/// <summary>
		/// New own estimation value
		/// Decimal for precision (e.g., 10.5)
		/// Optional - only update if provided
		/// </summary>
		public decimal? OwnEstimation { get; set; }
	}
}