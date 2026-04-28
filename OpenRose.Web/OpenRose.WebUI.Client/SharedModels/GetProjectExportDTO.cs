// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;

namespace OpenRose.WebUI.Client.SharedModels
{
	/// <summary>
	/// GetProjectExportDTO extends GetProjectDTO with estimation fields from ItemzHierarchy table
	/// Used specifically for exporting Project data with estimation roll-ups
	/// PHASE 1: Includes EstimationUnit, OwnEstimation, and RolledUpEstimation
	/// </summary>
	// EXPLANATION : We decided to new keyword for every field that is inherited. This way we control the order of 
	// fields in the exported JSON file and ensure that the estimation fields are included in the correct position.
	// If any new field is added in the base class then it will automatically be available for exporting data.
	public class GetProjectExportDTO : GetProjectDTO
	{

		/// <summary>
		/// Id of the Project representated by a GUID.
		/// </summary>
		public new Guid Id { get; set; }
		/// <summary>
		/// Project Name 
		/// </summary>
		public new string? Name { get; set; }
		/// <summary>
		/// Status of the Project
		/// </summary>
		public new string? Status { get; set; }
		/// <summary>
		/// Description of the Project
		/// </summary>
		public new string? Description { get; set; }
		/// <summary>
		/// User who created the Project
		/// </summary>
		public new string? CreatedBy { get; set; }
		/// <summary>
		/// Date and Time when Project was created
		/// </summary>
		public new DateTimeOffset CreatedDate { get; set; }

		/// <summary>
		/// PHASE 1: Estimation Unit for the Project (e.g., "Days", "Hours", "Story Points", "$", "GBP")
		/// Optional field from ItemzHierarchy table
		/// </summary>
		public string? EstimationUnit { get; set; }

		/// <summary>
		/// PHASE 1: Own Estimation value for this Project
		/// Optional field from ItemzHierarchy table
		/// </summary>
		public decimal OwnEstimation { get; set; } = 0;

		/// <summary>
		/// PHASE 1: Roll-up Estimation - sum of own estimation plus all child estimations
		/// Optional field from ItemzHierarchy table
		/// </summary>
		public decimal RolledUpEstimation { get; set; } = 0;
	}
}