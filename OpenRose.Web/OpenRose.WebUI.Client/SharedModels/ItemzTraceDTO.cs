// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.Serialization;
using OpenRose.WebUI.Client.SharedConstants;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OpenRose.WebUI.Client.SharedModels
{
    public class ItemzTraceDTO
    {
        /// <summary>
        /// Id of the From Trace Itemz representated by a GUID.
        /// </summary>
        [Required]
        public Guid FromTraceItemzId { get; set; }

		/// <summary>
		/// Id of the To Trace Itemz representated by a GUID.
		/// </summary>
		[Required]
		public Guid ToTraceItemzId { get; set; }

		/// <summary>
		/// Optional short unicode label for the trace ( <= 32 characters ).
		/// If omitted in JSON, defaults to sentinel string.
		/// </summary>
		[MaxLength(32)]
		[JsonConverter(typeof(TraceLabelConverter))]
		public string? TraceLabel { get; set; } = Sentinel.TraceLabelDefault;

		// NOTE: We apply [JsonConverter(typeof(TraceLabelConverter))] to TraceLabel
		// specifically for *serialization when sending data to the API layer*.
		// This converter enforces sentinel logic only on the outbound path, not on reads.
		//
		// It distinguishes three cases when the client sends data:
		//   1. Property omitted entirely → treated as sentinel (do not override existing value).
		//   2. Property explicitly set to null → serialize as null (user intends to clear value).
		//   3. Property set to a non-null string → serialize that string (user intends to update value).
		//
		// On the inbound path (reading API responses), we rely on System.Text.Json’s default behavior,
		// where missing properties and explicit nulls both become null. The converter does not alter reads.
		// This distinction is critical for deciding whether to override an existing TraceLabel in the repository.

	}
}
