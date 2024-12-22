// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;

namespace ItemzApp.API.Models
{
    /// <summary>
    /// BaselineDTO is a POCO used for serving requests like GetBaselines or GetBaseline by BaselineID.
    /// It will carry specified set of data that are exposed when baseline details are requested throgh "ItemzApp.API"
    /// </summary>
    public class GetBaselineDTO
    {
        /// <summary>
        /// Id of the Baseline representated by a GUID.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Baseline Name 
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// Description of the Baseline
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// User who created the Baseline
        /// </summary>
        public string? CreatedBy { get; set; }
        /// <summary>
        /// Date and Time when Baseline was created
        /// </summary>
        public DateTimeOffset CreatedDate { get; set; }
    }
}
