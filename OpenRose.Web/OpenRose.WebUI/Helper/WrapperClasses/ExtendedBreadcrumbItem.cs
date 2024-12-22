// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using MudBlazor;
using static MudBlazor.CategoryTypes;

namespace OpenRose.WebUI.Helper.WrapperClasses
{
	public class ExtendedBreadcrumbItem
	{
		public BreadcrumbItem OriginalItem { get; set; }
		/// <summary>
		/// Indicates if Baseline Hierarchy record is included or excluded
		/// </summary>
		public bool isIncluded { get; set; }

		public ExtendedBreadcrumbItem(string text, string href, string? icon = null, bool isIncluded = true)
		{
			OriginalItem = new BreadcrumbItem ( text: text, href: href,  icon: icon );
			this.isIncluded = isIncluded;
		}
	}

}
