// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.ComponentModel.DataAnnotations;

namespace OpenRose.WebUI.Client.SharedModels
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
	}
}
