// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Collections.Generic;

namespace ItemzApp.API.Entities
{
    /// <summary>
    /// BaselineItemz are mainly readonly objects. 
    /// Purpose of this UpdateBaselineItemzDTO is to allow setting property for 
    /// inclusion or exclusion of BaselineItemz from a given baseline itself.
    /// </summary>
    public class UpdateBaselineItemz
    {
        /// <summary>
        /// Id of the Baseline represented by a GUID
        /// </summary>
        public Guid BaselineId { get; set; }
        /// <summary>
        /// True if action is to include BaselineItemzs otherwise False
        /// </summary>
        public bool ShouldBeIncluded;
        /// <summary>
        /// True if action is to only include single node without including it's child breakdown structure. Will be ignored when ShouldBeIncluded is false.
        /// </summary>
        public bool SingleNodeInclusion = false;
        /// <summary>
        /// Id of the BaselineItemz represented by a GUID.
        /// </summary>
        public IEnumerable<Guid>? BaselineItemzIds { get; set; }
    }
}
