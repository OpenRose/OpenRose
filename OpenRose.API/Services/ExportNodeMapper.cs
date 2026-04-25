// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.


using AutoMapper;
using ItemzApp.API.Helper;
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
		private readonly IBaselineRepository _baselineRepository;
		private readonly IBaselineItemzTypeRepository _baselineItemzTypeRepository;
		private readonly IBaselineItemzRepository _baselineItemzRepository;
		private readonly IMapper _mapper;

		public ExportNodeMapper(
			IProjectRepository projectRepository,
			IItemzTypeRepository itemzTypeRepository,
			IItemzRepository itemzRepository,
			IBaselineRepository baselineRepository,
			IBaselineItemzTypeRepository baselineItemzTypeRepository,
			IBaselineItemzRepository baselineItemzRepository,
			IMapper mapper)
		{
			_projectRepository = projectRepository;
			_itemzTypeRepository = itemzTypeRepository;
			_itemzRepository = itemzRepository;
			_baselineRepository = baselineRepository;
			_baselineItemzTypeRepository = baselineItemzTypeRepository;
			_baselineItemzRepository = baselineItemzRepository;
			_mapper = mapper;
		}

		public async Task<ProjectExportNode> ConvertToProjectExportNode(NestedHierarchyIdRecordDetailsDTO node)
		{
			var projectEntity = await _projectRepository.GetProjectAsync(node.RecordId);
			var projectDto = _mapper.Map<GetProjectDTO>(projectEntity);

			var projectExportDto = new GetProjectExportDTO
			{
				Id = projectDto.Id,
				Name = projectDto.Name,
				Status = projectDto.Status,
				Description = projectDto.Description,
				CreatedBy = projectDto.CreatedBy,
				CreatedDate = projectDto.CreatedDate,
				EstimationUnit = node.EstimationUnit,
				OwnEstimation = node.OwnEstimation,
				RolledUpEstimation = node.RolledUpEstimation
			};

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
				Project = projectExportDto,
				ItemzTypes = itemzTypeExportNodes.Any() ? itemzTypeExportNodes : null
			};
		}

		public async Task<ItemzTypeExportNode> ConvertToItemzTypeExportNode(NestedHierarchyIdRecordDetailsDTO node)
		{
			var itemzTypeEntity = await _itemzTypeRepository.GetItemzTypeAsync(node.RecordId);
			var itemzTypeDto = _mapper.Map<GetItemzTypeDTO>(itemzTypeEntity);


			var itemzTypeExportDto = new GetItemzTypeExportDTO
			{
				Id = itemzTypeDto.Id,
				Name = itemzTypeDto.Name,
				Status = itemzTypeDto.Status,
				Description = itemzTypeDto.Description,
				CreatedBy = itemzTypeDto.CreatedBy,
				CreatedDate = itemzTypeDto.CreatedDate,
				IsSystem = itemzTypeDto.IsSystem,
				EstimationUnit = node.EstimationUnit,
				OwnEstimation = node.OwnEstimation,
				RolledUpEstimation = node.RolledUpEstimation
			};

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
				ItemzType = itemzTypeExportDto,
				Itemz = itemzExportNodes.Any() ? itemzExportNodes : null
			};
		}

		public async Task<ItemzExportNode> ConvertToItemzExportNode(NestedHierarchyIdRecordDetailsDTO node)
		{
			var itemzEntity = await _itemzRepository.GetItemzAsync(node.RecordId);
			var itemzDto = _mapper.Map<GetItemzDTO>(itemzEntity);

			var itemzExportDto = new GetItemzExportDTO
			{
				Id = itemzDto.Id,
				Name = itemzDto.Name,
				Status = itemzDto.Status,
				Priority = itemzDto.Priority,
				Description = itemzDto.Description,
				CreatedBy = itemzDto.CreatedBy,
				CreatedDate = itemzDto.CreatedDate,
				Severity = itemzDto.Severity,
				Tags = TagParsingUtility.NormalizeAndRemoveDuplicates(itemzDto.Tags),
				EstimationUnit = node.EstimationUnit,
				OwnEstimation = node.OwnEstimation,
				RolledUpEstimation = node.RolledUpEstimation
			};

			var subItemzExportNodes = new List<ItemzExportNode>();
			if (node.Children != null)
			{
				foreach (var child in node.Children.Where(c =>
					string.Equals(c.RecordType, "Itemz", StringComparison.OrdinalIgnoreCase)))
				{
					var subItemzNode = await ConvertToItemzExportNode(child);
					subItemzExportNodes.Add(subItemzNode);
				}
			}

			return new ItemzExportNode
			{
				Itemz = itemzExportDto,
				SubItemz = subItemzExportNodes.Any() ? subItemzExportNodes : null
			};
		}



		public async Task<BaselineExportNode> ConvertToBaselineExportNode(NestedBaselineHierarchyIdRecordDetailsDTO node)
		{
			// 1. Get the full Baseline entity for mapping
			var baselineEntity = await _baselineRepository.GetBaselineAsync(node.RecordId);
			var baselineDto = _mapper.Map<GetBaselineDTO>(baselineEntity);

			var baselineExportDto = new GetBaselineExportDTO
			{
				Id = baselineDto.Id,
				Name = baselineDto.Name,
				Description = baselineDto.Description,
				CreatedBy = baselineDto.CreatedBy,
				CreatedDate = baselineDto.CreatedDate,
				ProjectId = baselineDto.ProjectId,
				EstimationUnit = node.EstimationUnit,
				OwnEstimation = node.OwnEstimation,
				RolledUpEstimation = node.RolledUpEstimation
			};

			// 2. Recursively map BaselineItemzTypes
			var baselineItemzTypeExportNodes = new List<BaselineItemzTypeExportNode>();
			if (node.Children != null)
			{
				foreach (var child in node.Children.Where(c => string.Equals(c.RecordType, "BaselineItemzType", StringComparison.OrdinalIgnoreCase)))
				{
					var baselineItemzTypeNode = await ConvertToBaselineItemzTypeExportNode(child);
					baselineItemzTypeExportNodes.Add(baselineItemzTypeNode);
				}
			}

			return new BaselineExportNode
			{
				Baseline = baselineExportDto,
				BaselineItemzTypes = baselineItemzTypeExportNodes.Any() ? baselineItemzTypeExportNodes : null
			};
		}

		public async Task<BaselineItemzTypeExportNode> ConvertToBaselineItemzTypeExportNode(NestedBaselineHierarchyIdRecordDetailsDTO node)
		{
			var baselineItemzTypeEntity = await _baselineItemzTypeRepository.GetBaselineItemzTypeAsync(node.RecordId);
			var baselineItemzTypeDto = _mapper.Map<GetBaselineItemzTypeDTO>(baselineItemzTypeEntity);

			var baselineItemzTypeExportDto = new GetBaselineItemzTypeExportDTO		
			{
				Id = baselineItemzTypeDto.Id,
				ItemzTypeId = baselineItemzTypeDto.ItemzTypeId,
				BaselineId = baselineItemzTypeDto.BaselineId,	
				Name = baselineItemzTypeDto.Name,
				Status = baselineItemzTypeDto.Status,
				Description = baselineItemzTypeDto.Description,
				CreatedBy = baselineItemzTypeDto.CreatedBy,
				CreatedDate = baselineItemzTypeDto.CreatedDate,
				IsSystem = baselineItemzTypeDto.IsSystem,
				EstimationUnit = node.EstimationUnit,
				OwnEstimation = node.OwnEstimation,
				RolledUpEstimation = node.RolledUpEstimation
			};


			// Recursively map BaselineItemz
			var baselineItemzExportNodes = new List<BaselineItemzExportNode>();
			if (node.Children != null)
			{
				foreach (var child in node.Children.Where(c => string.Equals(c.RecordType, "BaselineItemz", StringComparison.OrdinalIgnoreCase)))
				{
					var baselineItemzNode = await ConvertToBaselineItemzExportNode(child);
					baselineItemzExportNodes.Add(baselineItemzNode);
				}
			}

			return new BaselineItemzTypeExportNode
			{
				BaselineItemzType = baselineItemzTypeExportDto,
				BaselineItemz = baselineItemzExportNodes.Any() ? baselineItemzExportNodes : null
			};
		}


		public async Task<BaselineItemzExportNode> ConvertToBaselineItemzExportNode(NestedBaselineHierarchyIdRecordDetailsDTO node)
		{
			var baselineItemzEntity = await _baselineItemzRepository.GetBaselineItemzAsync(node.RecordId);
			var baselineItemzDto = _mapper.Map<GetBaselineItemzDTO>(baselineItemzEntity);

			//// --- TAG NORMALIZATION FOR EXPORT ---
			//baselineItemzDto.Tags = TagParsingUtility.NormalizeAndRemoveDuplicates(baselineItemzDto.Tags);

			var baselineItemzExportDto = new GetBaselineItemzExportDTO
			{
				Id = baselineItemzDto.Id,
				ItemzId = baselineItemzDto.ItemzId,
				Name = baselineItemzDto.Name,
				Status = baselineItemzDto.Status,
				Priority = baselineItemzDto.Priority,
				Description = baselineItemzDto.Description,
				CreatedBy = baselineItemzDto.CreatedBy,
				CreatedDate = baselineItemzDto.CreatedDate,
				Severity = baselineItemzDto.Severity,
				isIncluded = baselineItemzDto.isIncluded,
				Tags = TagParsingUtility.NormalizeAndRemoveDuplicates(baselineItemzDto.Tags),
				EstimationUnit = node.EstimationUnit,
				OwnEstimation = node.OwnEstimation,
				// EXPLANATION :: For export, if the item is not included,
				// we set its roll-up estimation to 0 (ZERO). 
				RolledUpEstimation = baselineItemzDto.isIncluded == true ? node.RolledUpEstimation : 0
			};


			// Recursively map sub BaselineItemz (if any)
			var baselineSubItemzExportNodes = new List<BaselineItemzExportNode>();
			if (node.Children != null)
			{
				foreach (var child in node.Children.Where(c =>
					string.Equals(c.RecordType, "BaselineItemz", StringComparison.OrdinalIgnoreCase)))
				{
					var subBaselineItemzNode = await ConvertToBaselineItemzExportNode(child);
					baselineSubItemzExportNodes.Add(subBaselineItemzNode);
				}
			}

			return new BaselineItemzExportNode
			{
				BaselineItemz = baselineItemzExportDto,
				BaselineSubItemz = baselineSubItemzExportNodes.Any() ? baselineSubItemzExportNodes : null
			};
		}

	}
}