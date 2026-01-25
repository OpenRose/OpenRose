// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using ItemzApp.API.Constants;
using System;
using System.ComponentModel.DataAnnotations;

namespace ItemzApp.API.Models
{
    public class BaselineItemzTraceDTO
    {
        /// <summary>
        /// Id of the From Trace Baseline Itemz representated by a GUID.
        /// </summary>
        public Guid FromTraceBaselineItemzId { get; set; }

        /// <summary>
        /// Id of the To Trace Baseline Itemz representated by a GUID.
        /// </summary>
        public Guid ToTraceBaselineItemzId { get; set; }

		/// <summary>
		/// Optional short unicode label for the trace ( <= 32 characters )
		/// </summary>
		[MaxLength(32)]
		public string? TraceLabel { get; set; }


		// EXPLANATION: ShouldSerializeTraceLabel tells Newtonsoft whether to include TraceLabel in output
		// By adding this method we control at the DTO level itself that one should not serialize the TraceLabel
		// if it has the sentinel default value. So anywhere this DTO is serialized, the TraceLabel will be omitted.

		public bool ShouldSerializeTraceLabel()
		{
			// Only serialize if it was actually provided
			return (TraceLabel != Sentinel.TraceLabelDefault);
		}

	}
}
