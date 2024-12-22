// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;

namespace ItemzApp.API.Models
{
    /// <summary>
    /// BaselineItemz class containing several properties that represents it.
    /// This BaselineItemz class is returned by the "ItemzApp.API" in most of the cases when
    /// user sends request to read a BaselineItemz record.
    /// </summary>
    public class GetBaselineItemzDTO
    {
        /// <summary>
        /// Id of the BaselineItemz representated by a GUID.
        /// </summary>
        public Guid Id { get; set; }        
        /// <summary>
        /// Id of the Itemz as GUID based on which BaselineItemz was created.
        /// </summary>
        public Guid ItemzId { get; set; }
        /// <summary>
        /// Name or Title of the BaselineItemz
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// Status of the BaselineItemz
        /// </summary>
        public string? Status { get; set; }
        /// <summary>
        /// Priority of the BaselineItemz
        /// </summary>
        public string? Priority { get; set; }
        /// <summary>
        /// Description of the BaselineItemz
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// User who created the BaselineItemz
        /// </summary>
        public string? CreatedBy { get; set; }
        /// <summary>
        /// Date and Time when BaselineItemz was created
        /// </summary>
        public DateTimeOffset CreatedDate { get; set; }
        /// <summary>
        /// Severity of the BaselineItemz
        /// </summary>
        public string? Severity { get; set; }
        /// <summary>
        /// Indicates if BaselineItemz is included in the Baseline
        /// </summary>
        public bool isIncluded { get; set; }
    }
}
