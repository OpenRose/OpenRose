// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using ItemzApp.API.Constants;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace ItemzApp.API.Models
{
	public class ItemzTraceDTO
	{
		/// <summary>
		/// Id of the From Trace Itemz represented by a GUID.
		/// </summary>
		[Required]
		public Guid FromTraceItemzId { get; set; }

		/// <summary>
		/// Id of the To Trace Itemz represented by a GUID.
		/// </summary>
		[Required]
		public Guid ToTraceItemzId { get; set; }

		/// <summary>
		/// Optional short unicode label for the trace ( <= 32 characters ).
		/// If omitted in JSON, defaults to sentinel string.
		/// </summary>
		[MaxLength(32)]
		[JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public string? TraceLabel { get; set; } = Sentinel.TraceLabelDefault;

		// EXPLANATION: ShouldSerializeTraceLabel tells Newtonsoft whether to include TraceLabel in output
		// By adding this method we control at the DTO level itself that one should not serialize the TraceLabel
		// if it has the sentinel default value. So anywhere this DTO is serialized, the TraceLabel will be omitted
		// One example where this is needed is when we Establish traces between Itemz where user does not 
		// provide a TraceLabel, we want to omit it from the JSON sent back to the client post creation.

		public bool ShouldSerializeTraceLabel()
		{
			// Only serialize if it was actually provided
			return (TraceLabel != "XZQPLMNRTYABWECOVHJKDUSFIGQX");
		}
	}
}
