// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Collections.Generic;


namespace ItemzApp.API.Models
{
	public class ItemzParentAndChildTraceDTO
	{
		public ItemzParentAndChildTraceDTO()
		{
			SingleItemzAllTrace__DTO singleItemzAllTrace__DTO = new SingleItemzAllTrace__DTO();
			Itemz = singleItemzAllTrace__DTO;
		}

		public SingleItemzAllTrace__DTO? Itemz { get; set; }

	}

	public class SingleItemzAllTrace__DTO
	{
		public Guid ID { get; set; }
		public List<ParentTraceItemz__DTO>? ParentItemz { get; set; }
		public List<ChildTraceItemz__DTO>? ChildItemz { get; set; }
	}

	// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
	public class ChildTraceItemz__DTO
	{
		public Guid ItemzID { get; set; }

		// Optional short unicode label for this trace (Parent -> Child)
		public string? TraceLabel { get; set; }
	}

	public class ParentTraceItemz__DTO
	{
		public Guid ItemzID { get; set; }

		// Optional short unicode label for this trace (Parent -> Child)
		public string? TraceLabel { get; set; }
	}
}