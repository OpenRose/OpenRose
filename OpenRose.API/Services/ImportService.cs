// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using AutoMapper;
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
		private readonly IItemzRepository _itemzRepository; 
		private readonly IItemzTraceRepository _traceRepository; 
		private readonly ILogger<ImportService> _logger;
		private readonly IMapper _mapper;

		public ImportService(IItemzRepository itemzRepository,
							 IItemzTraceRepository traceRepository,
							 ILogger<ImportService> logger,
							 IMapper mapper)
		{
			_itemzRepository = itemzRepository;
			_traceRepository = traceRepository;
			_logger = logger;
			_mapper = mapper;
		}

		public async Task<ImportResult> ImportAsync(
											RepositoryExportDTO repositoryExportDto,
											string detectedType,
											ImportDataPlacementDTO placementDto)
		{
			var result = new ImportResult
			{
				Success = false,
				Errors = new List<string>()
			};

			// Validate import type
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
				// Determine placement mode
				bool isPlacingAsChild = !placementDto.FirstItemzId.HasValue && !placementDto.SecondItemzId.HasValue;

				if (isPlacingAsChild)
				{
					if (!placementDto.TargetParentId.Equals(Guid.Empty))
					{
						bool parentExists = await _itemzRepository.ItemzExistsAsync(placementDto.TargetParentId ?? Guid.Empty)
										 || await _itemzRepository.ItemzTypeExistsAsync(placementDto.TargetParentId ?? Guid.Empty);

						if (!parentExists)
						{
							result.Errors.Add($"Target parent ID '{placementDto.TargetParentId}' not found.");
							return result;
						}
					}
				}
				else
				{
					if (!(placementDto.FirstItemzId.HasValue && placementDto.SecondItemzId.HasValue))
					{
						result.Errors.Add("Both sibling Itemz IDs must be provided for placement between siblings.");
						return result;
					}

					bool firstExists = await _itemzRepository.ItemzExistsAsync(placementDto.FirstItemzId.Value);
					bool secondExists = await _itemzRepository.ItemzExistsAsync(placementDto.SecondItemzId.Value);

					if (!firstExists || !secondExists)
					{
						result.Errors.Add("One or both specified sibling Itemz IDs not found.");
						return result;
					}
				}

				var itemzRecord = repositoryExportDto.Itemz.First();
				var itemzDto = itemzRecord.Itemz;
				var tempDto = _mapper.Map<CreateItemzDTO>(itemzDto);
				var rootEntity = _mapper.Map<Itemz>(tempDto);

				_itemzRepository.AddItemz(rootEntity);
				await _itemzRepository.SaveAsync(); // Root Itemz created

				// EXPLANATION: We collect mapping of original ID to new ID
				var idMap = new Dictionary<Guid, Guid>
				{
					[itemzDto.Id] = rootEntity.Id
				};

				// Apply placement logic before importing sub-itemz
				if (placementDto.FirstItemzId.HasValue && placementDto.SecondItemzId.HasValue)
				{
					await _itemzRepository.AddOrMoveItemzBetweenTwoHierarchyRecordsAsync(
						placementDto.FirstItemzId.Value,
						placementDto.SecondItemzId.Value,
						rootEntity.Id,
						itemzDto.Name);
				}
				else if (!placementDto.TargetParentId.Equals(Guid.Empty))
				{
					await _itemzRepository.MoveItemzHierarchyAsync(
						rootEntity.Id,
						placementDto.TargetParentId ?? Guid.Empty,
						placementDto.AtBottomOfChildNodes,
						itemzDto.Name);
				}
				else
				{
					result.Errors.Add("No valid placement option provided.");
					return result;
				}

				await _itemzRepository.SaveAsync();

				// Now that root is placed, import sub-itemz recursively
				int totalCreated = 1;
				int maxDepth = 1;

				if (itemzRecord.SubItemz != null)
				{
					foreach (var subItemz in itemzRecord.SubItemz)
					{
						var (subId, subCreated, subDepth) =
							await ImportItemzRecursivelyWithStats(subItemz, rootEntity.Id, 2, idMap);

						totalCreated += subCreated;
						maxDepth = Math.Max(maxDepth, subDepth);
					}
				}

				// Import trace links
				int traceCreated = 0;

				foreach (var trace in repositoryExportDto.ItemzTraces ?? Enumerable.Empty<ItemzTraceDTO>())
				{
					if (idMap.TryGetValue(trace.FromTraceItemzId, out var fromId) &&
						idMap.TryGetValue(trace.ToTraceItemzId, out var toId))
					{
						var traceDto = new ItemzTraceDTO
						{
							FromTraceItemzId = fromId,
							ToTraceItemzId = toId
						};
						await _traceRepository.EstablishTraceBetweenItemzAsync(traceDto);
						traceCreated++;
					}
					else
					{
						_logger.LogWarning("Skipping trace due to missing Itemz ID mapping: {From} -> {To}",
							trace.FromTraceItemzId, trace.ToTraceItemzId);
					}
				}

				bool traceSaved = await _traceRepository.SaveAsync();
				if (!traceSaved)
				{
					_logger.LogWarning("Itemz trace save failed after import.");
					result.Errors.Add("Warning: Trace links may not have been saved.");
				}

				_logger.LogInformation("Imported {Count} Itemz records at depth {Depth}. Trace links created: {TraceCount}. Root ID: {RootId}",
					totalCreated, maxDepth, traceCreated, rootEntity.Id);

				result.Success = true;
				result.ImportedRootId = rootEntity.Id;
				result.ImportSummary = new ImportSummaryDTO
				{
					TotalCreated = totalCreated,
					Depth = maxDepth,
					TotalTraces = traceCreated
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
			Guid? parentItemzId,
			int currentDepth,
			Dictionary<Guid, Guid> idMap)
		{
			var itemzDto = itemzExportNode.Itemz;
			var originalId = itemzDto.Id;

			var tempItemzDTO = _mapper.Map<CreateItemzDTO>(itemzDto);
			var itemzEntity = _mapper.Map<Itemz>(tempItemzDTO);

			_itemzRepository.AddItemz(itemzEntity);
			await _itemzRepository.SaveAsync();

			idMap[originalId] = itemzEntity.Id;

			if (parentItemzId.HasValue)
			{
				await _itemzRepository.MoveItemzHierarchyAsync(itemzEntity.Id, parentItemzId.Value, true, itemzEntity.Name);
				await _itemzRepository.SaveAsync();
			}

			int totalCreated = 1;
			int maxDepth = currentDepth;

			if (itemzExportNode.SubItemz != null)
			{
				foreach (var subRecord in itemzExportNode.SubItemz)
				{
					var (subId, subCreated, subDepth) =
						await ImportItemzRecursivelyWithStats(subRecord, itemzEntity.Id, currentDepth + 1, idMap);

					totalCreated += subCreated;
					maxDepth = Math.Max(maxDepth, subDepth);
				}
			}

			return (itemzEntity.Id, totalCreated, maxDepth);
		}

	}
}
