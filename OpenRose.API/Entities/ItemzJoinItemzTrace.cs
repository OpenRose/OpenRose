// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.ComponentModel.DataAnnotations;

namespace ItemzApp.API.Entities
{
    public class ItemzJoinItemzTrace
    {
        public Guid FromItemzId { get; set; }
        public virtual Itemz? FromItemz { get; set; }

        public Guid ToItemzId { get; set; }
        public virtual Itemz? ToItemz { get; set; }

		// Optional short Unicode label up to 32 characters to describe purpose of trace.
		[MaxLength(32)]
		public string? TraceLabel { get; set; }
	}
}
