// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

namespace OpenRose.WebUI.Client.SharedModels
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
		public Dictionary<Guid, Guid> ItemzIdMapping { get; set; } = new();
	}
}
