// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;

namespace OpenRose.WebUI.Client.SharedModels.BetweenPagesAndComponent
{
	public class ParameterForBaselineBreadcrums
	{
		/// <summary>
		/// Id of the ItemzType representated by a GUID.
		/// </summary>
		public Guid Id { get; set; }
		/// <summary>
		/// Name or Title of the ItemzType
		/// </summary>
		public string? Name { get; set; }
		/// <summary>
		/// Name or Title of the Itemz
		/// </summary>
		public string RecordType { get; set; }

		/// <summary>
		/// Indicates if Baseline Hierarchy record is included or excluded
		/// </summary>
		public bool isIncluded { get; set; }
	}
}


