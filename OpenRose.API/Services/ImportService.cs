// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.


using AutoMapper;
using ItemzApp.API.Controllers;
using ItemzApp.API.Entities;
using ItemzApp.API.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ItemzApp.API.Services
{
	public class ImportService : IImportService
	{
		private readonly IItemzRepository _itemzRepository; // Assume this handles DB access
		private readonly ILogger<ImportService> _logger;
		private readonly IMapper _mapper;

		public ImportService(IItemzRepository itemzRepository, ILogger<ImportService> logger, IMapper mapper)
		{
			_itemzRepository = itemzRepository;
			_logger = logger;
			_mapper = mapper;
		}

		public async Task<ImportResult> ImportAsync( RepositoryExportDTO repositoryExportDto,
													string detectedType,
													ImportDataPlacementDTO placementDto)
		{
			var result = new ImportResult
			{
				Success = false,
				Errors = new List<string>()
			};

			if (!string.Equals(detectedType, "Itemz", StringComparison.OrdinalIgnoreCase))
			{
				result.Errors.Add($"Unsupported import type '{detectedType}' for this implementation.");
				return result;
			}

			if (repositoryExportDto.Itemz == null || repositoryExportDto.Itemz.Count != 1)
			{
				result.Errors.Add("Expected exactly one Itemz to import.");
				return result;
			}

			try
			{
				// Validate parent if one is provided
				if (!placementDto.TargetParentId.Equals(Guid.Empty))
				{
					bool parentExists = await _itemzRepository.ItemzExistsAsync(placementDto.TargetParentId)
									 || await _itemzRepository.ItemzTypeExistsAsync(placementDto.TargetParentId);

					if (!parentExists)
					{
						result.Errors.Add($"Target parent ID '{placementDto.TargetParentId}' not found.");
						return result;
					}
				}

				var itemzRecord = repositoryExportDto.Itemz.First();

				var (rootId, totalCreated, depth) =
					await ImportItemzRecursivelyWithStats(itemzRecord, placementDto.TargetParentId);

				_logger.LogInformation("Imported {Count} Itemz records at depth {Depth}. Root ID: {Id}",
					totalCreated, depth, rootId);

				result.Success = true;
				result.ImportedRootId = rootId;
				result.ImportSummary = new ImportSummaryDTO
				{
					TotalCreated = totalCreated,
					Depth = depth
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to import Itemz.");
				result.Errors.Add("Internal error occurred during Itemz import.");
			}

			return result;
		}


		private async Task<(Guid RootId, int TotalCreated, int Depth)> ImportItemzRecursivelyWithStats(
															ItemzExportNode itemzExportNode,
															Guid? parentItemzId = null,
															int currentDepth = 1)
		{
			var itemzDto = itemzExportNode.Itemz;
			var tempItemzDTO = _mapper.Map<CreateItemzDTO>(itemzDto);
			var itemzEntity = _mapper.Map<Itemz>(tempItemzDTO);

			_itemzRepository.AddItemz(itemzEntity);
			await _itemzRepository.SaveAsync();

			if (parentItemzId.HasValue)
			{
				await _itemzRepository.MoveItemzHierarchyAsync(
					itemzEntity.Id, parentItemzId.Value, true, itemzEntity.Name);
				await _itemzRepository.SaveAsync();
			}

			int totalCreated = 1;
			int maxDepth = currentDepth;

			if (itemzExportNode.SubItemz != null)
			{
				foreach (var subRecord in itemzExportNode.SubItemz)
				{
					var (subId, subCreated, subDepth) =
						await ImportItemzRecursivelyWithStats(subRecord, itemzEntity.Id, currentDepth + 1);

					totalCreated += subCreated;
					maxDepth = Math.Max(maxDepth, subDepth);
				}
			}

			return (itemzEntity.Id, totalCreated, maxDepth);
		}

	}
}
