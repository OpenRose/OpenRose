// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ItemzApp.API.Models;

namespace ItemzApp.API.Services
{
	public interface IBaselineItemzTraceExportService
	{
		Task<List<BaselineItemzTraceDTO>> GetTracesForExportAsync(HashSet<Guid> exportedBaselineItemzIds);
	}
}