// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

namespace ItemzApp.API.Models
{
	public class ImportSummaryDTO
	{
		public int TotalCreated { get; set; }
		public int Depth { get; set; }
		public int TotalTraces { get; set; }
	}
}
