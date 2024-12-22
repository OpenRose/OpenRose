// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using MudBlazor;

namespace OpenRose.WebUI.Helper.TreeViewNodes
{
    public static class TreeItemExtensions
    {
        public static IEnumerable<TreeItemData<Guid>> Flatten(this IEnumerable<TreeItemData<Guid>> items)
        {
            foreach (var item in items)
            {
                yield return item;
                if (item.Children != null)
                {
                    foreach (var child in item.Children.Flatten())
                    {
                        yield return child;
                    }
                }
            }
        }
    }

}
