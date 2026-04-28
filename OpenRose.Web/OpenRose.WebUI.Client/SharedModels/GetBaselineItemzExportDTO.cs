// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;

namespace OpenRose.WebUI.Client.SharedModels
{
    /// <summary>
    /// BaselineItemz class containing several properties that represents it.
    /// This BaselineItemz class is returned by the "ItemzApp.API" in most of the cases when
    /// user sends request to read a BaselineItemz record.
    /// </summary>
    public class GetBaselineItemzExportDTO : GetBaselineItemzDTO
	{
        /// <summary>
        /// Id of the BaselineItemz representated by a GUID.
        /// </summary>
        public new Guid Id { get; set; }        
        /// <summary>
        /// Id of the Itemz as GUID based on which BaselineItemz was created.
        /// </summary>
        public new Guid ItemzId { get; set; }
        /// <summary>
        /// Name or Title of the BaselineItemz
        /// </summary>
        public new string? Name { get; set; }
        /// <summary>
        /// Status of the BaselineItemz
        /// </summary>
        public new string? Status { get; set; }
        /// <summary>
        /// Priority of the BaselineItemz
        /// </summary>
        public new string? Priority { get; set; }
        /// <summary>
        /// Description of the BaselineItemz
        /// </summary>
        public new string? Description { get; set; }
        /// <summary>
        /// User who created the BaselineItemz
        /// </summary>
        public new string? CreatedBy { get; set; }
        /// <summary>
        /// Date and Time when BaselineItemz was created
        /// </summary>
        public new DateTimeOffset CreatedDate { get; set; }
        /// <summary>
        /// Severity of the BaselineItemz
        /// </summary>
        public new string? Severity { get; set; }
        /// <summary>
        /// Indicates if BaselineItemz is included in the Baseline
        /// </summary>
        public new bool isIncluded { get; set; }
		/// <summary>
    	/// Tags associated with this BaselineItemz.
		/// Tags are returned as a delimited string using pipe (|) as separator.
		/// The UI layer will parse this string and display tags using MudChips.
		/// These tags are captured from the source Itemz when the baseline is created,
		/// providing a snapshot of the tagged state at that point in time.
		/// </summary>
		public new string? Tags { get; set; }

		/// <summary>
		/// PHASE 1: Estimation Unit for the Itemz (e.g., "Days", "Hours", "Story Points", "$", "GBP")
		/// Optional field from ItemzHierarchy table
		/// </summary>
		public string? EstimationUnit { get; set; }

		/// <summary>
		/// PHASE 1: Own Estimation value for this Itemz
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
