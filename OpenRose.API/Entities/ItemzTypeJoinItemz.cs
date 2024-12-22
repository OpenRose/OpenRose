// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ItemzApp.API.Entities
{
    public class ItemzTypeJoinItemz
    {
        public Guid ItemzTypeId { get; set; }
        public ItemzType? ItemzType { get; set; }

        public Guid ItemzId { get; set; }
        public Itemz? Itemz { get; set; }
    }
}
