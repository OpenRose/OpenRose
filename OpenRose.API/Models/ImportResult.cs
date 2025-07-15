using ItemzApp.API.Models;
using System;
using System.Collections.Generic;

namespace ItemzApp.API.Models
{
	/// <summary>
	/// Represents the result of an import operation.
	/// </summary>
	public class ImportResult
	{
		public bool Success { get; set; }
		public Guid? ImportedRootId { get; set; }
		public ImportSummaryDTO? ImportSummary { get; set; }
		public List<string>? Errors { get; set; }
	}
}
