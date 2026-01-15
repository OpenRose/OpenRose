// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.ComponentModel.DataAnnotations;

namespace ItemzApp.API.Models
{
	/// <summary>
	/// Export-only representation of a trace link between two Baseline Itemz records.
	/// This class is aligned with the JSON Schema definition for BaselineItemzTraceExportNode.
	/// </summary>
	public class BaselineItemzTraceExportNodeDTO
	{
		/// <summary>
		/// Optional human-readable name of the source Baseline Itemz.
		/// This is included for readability in exports only.
		/// </summary>
		public string? FromTraceBaselineItemzName { get; set; }

		/// <summary>
		/// Optional human-readable name of the target Baseline Itemz.
		/// This is included for readability in exports only.
		/// </summary>
		public string? ToTraceBaselineItemzName { get; set; }

		/// <summary>
		/// Id of the source Baseline Itemz in the trace relationship, represented by a GUID.
		/// </summary>
		[Required]
		public Guid FromTraceBaselineItemzId { get; set; }

		/// <summary>
		/// Id of the target Baseline Itemz in the trace relationship, represented by a GUID.
		/// </summary>
		[Required]
		public Guid ToTraceBaselineItemzId { get; set; }

		/// <summary>
		/// Optional short unicode label for the baseline trace (<= 32 characters).
		/// If null, no label is associated with the trace.
		/// </summary>
		[MaxLength(32)]
		public string? TraceLabel { get; set; }
	}
}
