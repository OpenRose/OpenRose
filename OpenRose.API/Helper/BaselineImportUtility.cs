// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.


using ItemzApp.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ItemzApp.API.Helper
{
	public static class BaselineImportUtility
	{
		public static List<BaselineItemzTraceDTO> FilterValidBaselineItemzTraces(
			List<BaselineItemzTraceDTO> traces,
			IEnumerable<BaselineItemzExportNode> validNodes)
		{
			var validIds = new HashSet<Guid>(
				validNodes.Select(n => n.BaselineItemz.Id)
			);

			return traces
				.Where(t => validIds.Contains(t.FromTraceBaselineItemzId) &&
							validIds.Contains(t.ToTraceBaselineItemzId))
				.ToList();
		}
	}

}
