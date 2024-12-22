// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.


using MudBlazor;

namespace OpenRose.WebUI.Components.EventServices
{
    public class BaselineTreeNodeItemzSelectionService
	{
        public event Action<Guid> OnBaselineTreeNodeItemzSelected;
        public event Action<Guid, string> OnBaselineTreeNodeItemzNameUpdated;
		public event Action<Guid, bool> OnLoadingOfBaselineItemzTreeViewComponent;
        public event Action<Guid> OnSingleBaselineItemzIsIncludedChanged;
        public event Action<Guid> OnExcludeAllChildrenBaselineItemzTreeNodes;
		public event Action<Guid> OnIncludeAllChildrenBaselineItemzTreeNodes;
		public event Func<Guid, bool> OnRequestNodeWithParent;
        public event Action<Guid> OnScrollToTreeViewNode;


		public void SelectBaselineTreeNodeItemz(Guid recordId)
        {
            OnBaselineTreeNodeItemzSelected?.Invoke(recordId);

        }

		public void UpdateBaselineTreeNodeItemzName(Guid recordId, string newName)
        {
            OnBaselineTreeNodeItemzNameUpdated?.Invoke(recordId, newName);
        }

		public void LoadingOfBaselineItemzTreeViewComponent (Guid recordId, bool isIncluded)
		{
            OnLoadingOfBaselineItemzTreeViewComponent?.Invoke(recordId, isIncluded);

        }
        public void SingleBaselineItemzIsIncludedChanged(Guid recordId)
        {
			OnSingleBaselineItemzIsIncludedChanged?.Invoke(recordId);
		}

        public void ExcludeAllChildrenBaselineItemzTreeNodes(Guid recordId)
        {
			OnExcludeAllChildrenBaselineItemzTreeNodes?.Invoke(recordId);

		}
		public void IncludeAllChildrenBaselineItemzTreeNodes(Guid recordId)
		{
			OnIncludeAllChildrenBaselineItemzTreeNodes?.Invoke(recordId);

		}

        // Method to request parent node IsIncuded status
        public bool RequestNodeWithParent(Guid recordId) 
        { 
            return OnRequestNodeWithParent.Invoke(recordId); 
        }

        public void ScrollToTreeViewNode(Guid recordId) 
        {
            OnScrollToTreeViewNode?.Invoke(recordId);
        }

    }
}



