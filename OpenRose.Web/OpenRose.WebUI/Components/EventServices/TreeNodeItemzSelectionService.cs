// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.SharedModels;

namespace OpenRose.WebUI.Components.EventServices
{
    public class TreeNodeItemzSelectionService
    {
		public event Func<Guid, Task> OnTreeNodeItemzSelected;
		public event Action<Guid, string> OnTreeNodeItemzNameUpdated;
		public event Action<Guid> OnScrollToTreeViewNode;
		public event Func<Guid, GetItemzDTO, Task> OnCreatedNewItemz;
		public event Action<Guid> OnTreeNodeItemzDeleted;
		public event Func<Guid, GetItemzTypeDTO, Task> OnCreatedNewItemzType;


		public void SelectTreeNodeItemz(Guid recordId)
        {
            OnTreeNodeItemzSelected?.Invoke(recordId);
        }

		public void UpdateTreeNodeItemzName(Guid recordId, string newName)
        {
            OnTreeNodeItemzNameUpdated?.Invoke(recordId, newName);
        }

		public void ScrollToTreeViewNode(Guid recordId)
		{
			OnScrollToTreeViewNode?.Invoke(recordId);
		}

        public void CreatedNewItemz(Guid recordId, GetItemzDTO newlyCreatedSiblingItemz)
        {
			OnCreatedNewItemz(recordId, newlyCreatedSiblingItemz);
		}
		public void DeletedTreeNodeItemz(Guid recordId)
		{
			OnTreeNodeItemzDeleted?.Invoke(recordId);
		}

		public void CreatedNewItemzType(Guid recordId, GetItemzTypeDTO newlyCreatedSiblingItemzType)
		{
			OnCreatedNewItemzType(recordId, newlyCreatedSiblingItemzType);
		}

	}
}



