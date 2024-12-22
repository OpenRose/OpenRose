// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using MudBlazor;

namespace OpenRose.WebUI.Components.EventServices
{
    public class BreadcrumsService
	{

		public event Func<bool> OnRequestIsOrphanStatus;

        // Method to request for Orphan status
        public bool RequestIsOrphanStatus() 
        {
			return OnRequestIsOrphanStatus.Invoke(); 
        }
    }
}



