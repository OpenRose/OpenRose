// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using Microsoft.JSInterop;

namespace OpenRose.WebUI.Services
{
	public class FullScreenService
	{
		private readonly IJSRuntime _js;

		public FullScreenService(IJSRuntime js)
		{
			_js = js;
		}

		public ValueTask EnterFullscreen() =>
			_js.InvokeVoidAsync("openroseFullscreen.enter");

		public ValueTask ExitFullscreen() =>
			_js.InvokeVoidAsync("openroseFullscreen.exit");

		public ValueTask<bool> IsFullscreen() =>
			_js.InvokeAsync<bool>("openroseFullscreen.isFullscreen");
	}
}