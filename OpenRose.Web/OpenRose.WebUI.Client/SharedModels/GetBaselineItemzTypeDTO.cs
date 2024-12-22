// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;

namespace OpenRose.WebUI.Client.SharedModels
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
    public class GetBaselineItemzTypeDTO
    {
        /// <summary>
        /// Id of the BaselineItemzType representated by a GUID.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Id of the ItemzType based on which BaselineItemzType was created
        /// </summary>
        public Guid ItemzTypeId { get; set; }
        /// <summary>
        /// Id of the Parent Baseline underwhich BaselineItemzType is associated as child
        /// </summary>
        public Guid BaselineId { get; set; }
        /// <summary>
        /// BaselineItemzType Name 
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// Status of the BaselineItemzType
        /// </summary>
        public string? Status { get; set; }
        /// <summary>
        /// Description of the BaselineItemzType
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// User who created the original ItemzType which was used for creating BaselineItemzType
        /// </summary>
        public string? CreatedBy { get; set; }
        /// <summary>
        /// Date and Time when original ItemzType was created from which BaselineItemzType as created
        /// </summary>
        public DateTimeOffset CreatedDate { get; set; }
        /// <summary>
        /// Returns true if it's system BaselineItemzType otherwise false
        /// </summary>
        public bool IsSystem { get; set; }
    }
}
