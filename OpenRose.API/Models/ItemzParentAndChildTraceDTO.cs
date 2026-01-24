// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using ItemzApp.API.Constants;
using System;
using System.Collections.Generic;

namespace ItemzApp.API.Models
{
	// Represents a parent and child trace relationship for a single Itemz.
	public class ItemzParentAndChildTraceDTO
	{
		public ItemzParentAndChildTraceDTO()
		{
			// Ensure Itemz is always initialized to avoid null reference issues.
			SingleItemzAllTrace__DTO singleItemzAllTrace__DTO = new SingleItemzAllTrace__DTO();
			Itemz = singleItemzAllTrace__DTO;
		}

		public SingleItemzAllTrace__DTO? Itemz { get; set; }
	}

	// Represents all traceability information for a single Itemz.
	public class SingleItemzAllTrace__DTO
	{
		public Guid ID { get; set; }

		// Always initialized to an empty list to prevent null reference errors.
		public List<ParentTraceItemz__DTO> ParentItemz { get; set; } = new();

		// Always initialized to an empty list to prevent null reference errors.
		public List<ChildTraceItemz__DTO> ChildItemz { get; set; } = new();
	}

	// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
	// Represents a child trace link for an Itemz.
	public class ChildTraceItemz__DTO
	{
		public Guid ItemzID { get; set; }

		// Optional short unicode label for this trace (Parent -> Child).
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

	// Represents a parent trace link for an Itemz.
	public class ParentTraceItemz__DTO
	{
		public Guid ItemzID { get; set; }

		// Optional short unicode label for this trace (Parent -> Child).
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
