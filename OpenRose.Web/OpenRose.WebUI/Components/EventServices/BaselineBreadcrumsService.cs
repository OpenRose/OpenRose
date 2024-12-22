// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.


using MudBlazor;

namespace OpenRose.WebUI.Components.EventServices
{
    public class BaselineBreadcrumsService
	{

		public event Func<bool> OnRequestParentNodeIsIncluded;

        // Method to request parent node IsIncuded status
        public bool RequestParentNodeIsIncluded() 
        {
			return OnRequestParentNodeIsIncluded.Invoke(); 
        }
    }
}



