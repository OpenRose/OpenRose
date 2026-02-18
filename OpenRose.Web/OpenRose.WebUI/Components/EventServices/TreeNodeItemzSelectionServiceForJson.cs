// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

namespace OpenRose.WebUI.Components.EventServices
{
    public class TreeNodeItemzSelectionServiceForJson
	{
        public event Func<Guid, Task>? OnScrollToTreeViewNode;
					
		public async Task ScrollToTreeViewNodeAsync(Guid recordId)
		{
			if (OnScrollToTreeViewNode != null)
			{
				await OnScrollToTreeViewNode.Invoke(recordId);
			}
		}

	}
}



