// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System.Threading.Tasks;
using ItemzApp.API.Models;

namespace ItemzApp.API.Services
{
	public interface IExportNodeMapper
	{
		Task<ProjectExportNode> ConvertToProjectExportNode(NestedHierarchyIdRecordDetailsDTO node);
		Task<ItemzTypeExportNode> ConvertToItemzTypeExportNode(NestedHierarchyIdRecordDetailsDTO node);
		Task<ItemzExportNode> ConvertToItemzExportNode(NestedHierarchyIdRecordDetailsDTO node);
	}
}