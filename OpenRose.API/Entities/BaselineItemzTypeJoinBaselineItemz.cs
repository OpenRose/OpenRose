// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.ComponentModel.DataAnnotations;

namespace ItemzApp.API.Entities
{
    public class BaselineItemzTypeJoinBaselineItemz
    {
        public Guid BaselineItemzTypeId { get; set; }
        public BaselineItemzType? BaselineItemzType { get; set; }

        public Guid BaselineItemzId { get; set; }
        public BaselineItemz? BaselineItemz { get; set; }
    }
}
