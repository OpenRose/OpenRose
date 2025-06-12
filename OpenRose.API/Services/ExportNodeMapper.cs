// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.


using AutoMapper;
using ItemzApp.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ItemzApp.API.Services
{
	public class ExportNodeMapper : IExportNodeMapper
	{
		private readonly IProjectRepository _projectRepository;
		private readonly IItemzTypeRepository _itemzTypeRepository;
		private readonly IItemzRepository _itemzRepository;
		private readonly IMapper _mapper;

		public ExportNodeMapper(
			IProjectRepository projectRepository,
			IItemzTypeRepository itemzTypeRepository,
			IItemzRepository itemzRepository,
			IMapper mapper)
		{
			_projectRepository = projectRepository;
			_itemzTypeRepository = itemzTypeRepository;
			_itemzRepository = itemzRepository;
			_mapper = mapper;
		}

		public async Task<ProjectExportNode> ConvertToProjectExportNode(NestedHierarchyIdRecordDetailsDTO node)
		{
			// 1. Get the full Project entity for mapping
			var projectEntity = await _projectRepository.GetProjectAsync(node.RecordId);
			var projectDto = _mapper.Map<GetProjectDTO>(projectEntity);

			// 2. Recursively map ItemzTypes
			var itemzTypeExportNodes = new List<ItemzTypeExportNode>();
			if (node.Children != null)
			{
				foreach (var child in node.Children.Where(c => string.Equals(c.RecordType, "ItemzType", StringComparison.OrdinalIgnoreCase)))
				{
					var itemzTypeNode = await ConvertToItemzTypeExportNode(child);
					itemzTypeExportNodes.Add(itemzTypeNode);
				}
			}

			return new ProjectExportNode
			{
				Project = projectDto,
				ItemzTypes = itemzTypeExportNodes.Any() ? itemzTypeExportNodes : null
			};
		}

		public async Task<ItemzTypeExportNode> ConvertToItemzTypeExportNode(NestedHierarchyIdRecordDetailsDTO node)
		{
			var itemzTypeEntity = await _itemzTypeRepository.GetItemzTypeAsync(node.RecordId);
			var itemzTypeDto = _mapper.Map<GetItemzTypeDTO>(itemzTypeEntity);

			// Recursively map Itemz
			var itemzExportNodes = new List<ItemzExportNode>();
			if (node.Children != null)
			{
				foreach (var child in node.Children.Where(c => string.Equals(c.RecordType, "Itemz", StringComparison.OrdinalIgnoreCase)))
				{
					var itemzNode = await ConvertToItemzExportNode(child);
					itemzExportNodes.Add(itemzNode);
				}
			}

			return new ItemzTypeExportNode
			{
				ItemzType = itemzTypeDto,
				Itemz = itemzExportNodes.Any() ? itemzExportNodes : null
			};
		}

		public async Task<ItemzExportNode> ConvertToItemzExportNode(NestedHierarchyIdRecordDetailsDTO node)
		{
			var itemzEntity = await _itemzRepository.GetItemzAsync(node.RecordId);
			var itemzDto = _mapper.Map<GetItemzDTO>(itemzEntity);

			// Recursively map sub-Itemz
			var subItemzExportNodes = new List<ItemzExportNode>();
			if (node.Children != null)
			{
				foreach (var child in node.Children.Where(c => string.Equals(c.RecordType, "Itemz", StringComparison.OrdinalIgnoreCase)))
				{
					var subItemzNode = await ConvertToItemzExportNode(child);
					subItemzExportNodes.Add(subItemzNode);
				}
			}

			return new ItemzExportNode
			{
				Itemz = itemzDto,
				SubItemz = subItemzExportNodes.Any() ? subItemzExportNodes : null
			};
		}
	}
}