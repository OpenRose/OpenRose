﻿// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;

namespace OpenRose.WebUI.Client.SharedModels
{
    public class ItemzTraceDTO
    {
        /// <summary>
        /// Id of the From Trace Itemz representated by a GUID.
        /// </summary>
        public Guid FromTraceItemzId { get; set; }

        /// <summary>
        /// Id of the To Trace Itemz representated by a GUID.
        /// </summary>
        public Guid ToTraceItemzId { get; set; }
    }
}
