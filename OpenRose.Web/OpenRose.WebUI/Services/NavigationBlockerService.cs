// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using Microsoft.JSInterop;

namespace OpenRose.WebUI.Services
{
	public class NavigationBlockerService
	{
		private readonly IJSRuntime _js;

		public NavigationBlockerService(IJSRuntime js)
		{
			_js = js;
		}

		public ValueTask Enable() =>
			_js.InvokeVoidAsync("openroseNavigationBlocker.enable");

		public ValueTask Disable() =>
			_js.InvokeVoidAsync("openroseNavigationBlocker.disable");
	}

}
