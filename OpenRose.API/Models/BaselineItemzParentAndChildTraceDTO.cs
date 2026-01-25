// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using ItemzApp.API.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ItemzApp.API.Models
{
	public class BaselineItemzParentAndChildTraceDTO
	{
		public BaselineItemzParentAndChildTraceDTO()
		{
			SingleBaselineItemzAllTrace__DTO singleBaselineItemzAllTrace__DTO = new SingleBaselineItemzAllTrace__DTO();
			BaselineItemz = singleBaselineItemzAllTrace__DTO;
		}

		public SingleBaselineItemzAllTrace__DTO? BaselineItemz { get; set; }

	}

	public class SingleBaselineItemzAllTrace__DTO
	{
		public Guid ID { get; set; }
		public List<ParentTraceBaselineItemz__DTO>? ParentBaselineItemz { get; set; } = new();
		public List<ChildTraceBaselineItemz__DTO>? ChildBaselineItemz { get; set; } = new();
	}

	// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
	public class ChildTraceBaselineItemz__DTO
	{
		public Guid BaselineItemzID { get; set; }

		// Optional short unicode label for this baseline trace
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

	public class ParentTraceBaselineItemz__DTO
	{
		public Guid BaselineItemzID { get; set; }

		// Optional short unicode label for this baseline trace
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