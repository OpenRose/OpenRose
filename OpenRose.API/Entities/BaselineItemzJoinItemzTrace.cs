// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;

namespace ItemzApp.API.Entities
{
    public class BaselineItemzJoinItemzTrace
    {
        public Guid BaselineFromItemzId { get; set; }
        public virtual BaselineItemz? BaselineFromItemz { get; set; }

        public Guid BaselineToItemzId { get; set; }
        public virtual BaselineItemz? BaselineToItemz { get; set; }

        public Guid BaselineId { get; set; }
    }
}
