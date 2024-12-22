// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;

namespace ItemzApp.API.Models
{
    public class ItemzTypeItemzDTO
    {
        /// <summary>
        /// Id of the Itemz representated by a GUID.
        /// </summary>
        public Guid ItemzId { get; set; }

        /// <summary>
        /// Id of the ItemzType representated by a GUID.
        /// </summary>
        public Guid ItemzTypeId { get; set; }

    }
}
