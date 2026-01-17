// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.ComponentModel.DataAnnotations;

namespace ItemzApp.API.Models
{
	/// <summary>
	/// Export-only representation of a trace link between two Itemz records.
	/// This class is aligned with the JSON Schema definition for ItemzTraceExportNode.
	/// </summary>
	public class ItemzTraceExportNodeDTO
	{

		/// <summary>
		/// Optional human-readable name of the source Itemz.
		/// This is included for readability in exports only.
		/// </summary>
		public string? FromTraceName { get; set; }

		/// <summary>
		/// Optional human-readable name of the target Itemz.
		/// This is included for readability in exports only.
		/// </summary>
		public string? ToTraceName { get; set; }

		/// <summary>
		/// Id of the source Itemz in the trace relationship, represented by a GUID.
		/// </summary>
		[Required]
		public Guid FromTraceItemzId { get; set; }

		/// <summary>
		/// Id of the target Itemz in the trace relationship, represented by a GUID.
		/// </summary>
		[Required]
		public Guid ToTraceItemzId { get; set; }

		/// <summary>
		/// Optional short unicode label for the trace (<= 32 characters).
		/// If null, no label is associated with the trace.
		/// </summary>
		[MaxLength(32)]
		public string? TraceLabel { get; set; }

	}
}
