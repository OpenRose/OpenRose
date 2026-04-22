// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;

namespace ItemzApp.API.Models
{
    /// <summary>
    /// BaselineItemzTypeDTO is a POCO used for serving requests like 
    /// GetBaselineItemzTypes or GetBaselineItemzType by BaselineItemzTypeID.
    /// It will carry specified set of data that are exposed when 
    /// BaselineItemzType details are requested throgh "ItemzApp.API"
    /// 
    /// Remember that many of the fields are copied over from original ItemzType
    /// that was used at the time when BaselineItemzType was created.
    /// </summary>
    public class GetBaselineItemzTypeExportDTO : GetBaselineItemzTypeDTO
	{
        /// <summary>
        /// Id of the BaselineItemzType representated by a GUID.
        /// </summary>
        public new  Guid Id { get; set; }
        /// <summary>
        /// Id of the ItemzType based on which BaselineItemzType was created
        /// </summary>
        public new Guid ItemzTypeId { get; set; }
        /// <summary>
        /// Id of the Parent Baseline underwhich BaselineItemzType is associated as child
        /// </summary>
        public new Guid BaselineId { get; set; }
        /// <summary>
        /// BaselineItemzType Name 
        /// </summary>
        public new string? Name { get; set; }
        /// <summary>
        /// Status of the BaselineItemzType
        /// </summary>
        public new string? Status { get; set; }
        /// <summary>
        /// Description of the BaselineItemzType
        /// </summary>
        public new string? Description { get; set; }
        /// <summary>
        /// User who created the original ItemzType which was used for creating BaselineItemzType
        /// </summary>
        public new string? CreatedBy { get; set; }
        /// <summary>
        /// Date and Time when original ItemzType was created from which BaselineItemzType as created
        /// </summary>
        public new DateTimeOffset CreatedDate { get; set; }
        /// <summary>
        /// Returns true if it's system BaselineItemzType otherwise false
        /// </summary>
        public new bool IsSystem { get; set; }

		/// <summary>
		/// PHASE 1: Estimation Unit for the ItemzType (e.g., "Days", "Hours", "Story Points", "$", "GBP")
		/// Optional field from ItemzHierarchy table
		/// </summary>
		public string? EstimationUnit { get; set; }

		/// <summary>
		/// PHASE 1: Own Estimation value for this ItemzType
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
