// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;

namespace ItemzApp.API.Entities
{
    public class ItemzJoinItemzTrace
    {
        public Guid FromItemzId { get; set; }
        public virtual Itemz? FromItemz { get; set; }

        public Guid ToItemzId { get; set; }
        public virtual Itemz? ToItemz { get; set; }
    }
}
