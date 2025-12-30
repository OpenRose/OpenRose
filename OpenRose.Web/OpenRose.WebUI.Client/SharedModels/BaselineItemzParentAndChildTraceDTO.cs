// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenRose.WebUI.Client.SharedModels
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
		public List<ParentTraceBaselineItemz__DTO>? ParentBaselineItemz { get; set; }
		public List<ChildTraceBaselineItemz__DTO>? ChildBaselineItemz { get; set; }
	}

	// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
	public class ChildTraceBaselineItemz__DTO
	{
		public Guid BaselineItemzID { get; set; }

		// Optional short unicode label for this baseline trace
		public string? TraceLabel { get; set; }
	}

	public class ParentTraceBaselineItemz__DTO
	{
		public Guid BaselineItemzID { get; set; }

		// Optional short unicode label for this baseline trace
		public string? TraceLabel { get; set; }
	}
}