// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

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
	}

	// Represents a parent trace link for an Itemz.
	public class ParentTraceItemz__DTO
	{
		public Guid ItemzID { get; set; }

		// Optional short unicode label for this trace (Parent -> Child).
		public string? TraceLabel { get; set; }
	}
}
