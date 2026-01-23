// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.SharedConstants;

namespace OpenRose.WebUI.Helper.TraceLabel
{

	public static class TraceLabelHelper
	{
		public static string? NormalizeTraceLabel(string? label)
		{
			return string.IsNullOrWhiteSpace(label) || label == Sentinel.TraceLabelDefault
				? null
				: label;
		}
	}
}


