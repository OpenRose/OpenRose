// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System.Net;

namespace OpenRose.WebUI.Client.Utilities
{
	public static class HttpStatusCodesHelper
	{
		public static readonly List<HttpStatusCode> ErrorStatusCodes = new List<HttpStatusCode>
		{
			HttpStatusCode.Conflict,
			HttpStatusCode.NotFound,
			HttpStatusCode.BadRequest,
			HttpStatusCode.Forbidden
            // Add other status codes you want to handle
        };
	}
}