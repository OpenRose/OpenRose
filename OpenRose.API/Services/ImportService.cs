// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using AutoMapper;
using ItemzApp.API.Entities;
using ItemzApp.API.Models;
using ItemzApp.API.Helper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ItemzApp.API.Services
{
	public class ImportService : IImportService
	{
		private readonly IProjectRepository _projectRepository;
		private readonly IItemzTypeRepository _itemzTypeRepository;
		private readonly IItemzRepository _itemzRepository; 
		private readonly IItemzTraceRepository _traceRepository; 
		private readonly ILogger<ImportService> _logger;
		private readonly IMapper _mapper;

		public ImportService(IProjectRepository projectRepository, 
							IItemzTypeRepository itemzTypeRepository, 
							IItemzRepository itemzRepository,
							 IItemzTraceRepository traceRepository,
							 ILogger<ImportService> logger,
							 IMapper mapper)
		{
			_projectRepository = projectRepository;
			_itemzTypeRepository = itemzTypeRepository;
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

				// EXPLANATION: Purpose of first creating 'tempDto' of type 'CreateItemzDTO'
				// is to make sure that we do not map ID, Created By, Created Date fields and rather 
				// we remove data related to such properties that are supposed to be created manually 
				// at the time of creating new record. So we had to introduce 'tempDto'.

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

				// Process trace links
				int traceCreated = await ProcessItemzTracesAsync(repositoryExportDto.ItemzTraces, idMap, result.Errors);

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
				result.ItemzIdMapping = idMap;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to import Itemz.");
				result.Errors.Add("Internal error occurred during Itemz import.");
			}

			return result;
		}


		private async Task<(Guid RootId, int TotalCreated, int Depth)> ImportItemzRecursivelyWithStats(
									ItemzImportNode itemzImportNode,
									Guid? parentItemzId,
									int currentDepth,
									Dictionary<Guid, Guid> idMap)
		{
			var itemzDto = itemzImportNode.Itemz;
			var originalId = itemzDto.Id;

			// EXPLANATION: Purpose of first creating 'tempItemzDTO' of type 'CreateItemzDTO'
			// is to make sure that we do not map ID, Created By, Created Date fields and rather 
			// we remove data related to such properties that are supposed to be created manually 
			// at the time of creating new record. So we had to introduce 'tempItemzDTO'.


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

			if (itemzImportNode.SubItemz != null)
			{
				foreach (var subRecord in itemzImportNode.SubItemz)
				{
					var (subId, subCreated, subDepth) =
						await ImportItemzRecursivelyWithStats(subRecord, itemzEntity.Id, currentDepth + 1, idMap);

					totalCreated += subCreated;
					maxDepth = Math.Max(maxDepth, subDepth);
				}
			}

			return (itemzEntity.Id, totalCreated, maxDepth);
		}



		public async Task<ImportResult> ImportItemzTypeHierarchyAsync(
											RepositoryExportDTO repositoryExportDto,
											ImportDataPlacementDTO placementDto)
		{
			var result = new ImportResult
			{
				Success = false,
				Errors = new List<string>()
			};

			if (repositoryExportDto.ItemzTypes == null || repositoryExportDto.ItemzTypes.Count != 1)
			{
				result.Errors.Add("Expected exactly one ItemzType to import.");
				return result;
			}

			try
			{
				// Resolve ProjectId
				Guid? resolvedProjectId = null;

				if (placementDto.TargetParentId.HasValue && placementDto.TargetParentId.Value != Guid.Empty)
				{
					resolvedProjectId = placementDto.TargetParentId;
				}
				else if (placementDto.FirstItemzTypeId.HasValue)
				{
					var siblingItemzType = await _itemzTypeRepository.GetItemzTypeAsync(placementDto.FirstItemzTypeId.Value);
					if (siblingItemzType == null)
					{
						result.Errors.Add($"Could not find ItemzType with ID '{placementDto.FirstItemzTypeId.Value}' to resolve ProjectId.");
						return result;
					}
					resolvedProjectId = siblingItemzType.ProjectId;
				}
				else
				{
					result.Errors.Add("Project ID could not be resolved for ItemzType import.");
					return result;
				}

				// Import a single ItemzType
				var idMap = new Dictionary<Guid, Guid>();
				var itemzTypeRecord = repositoryExportDto.ItemzTypes.First();

				var (itemzTypeId, totalCreated, maxDepth) = await ImportSingleItemzTypeAsync(
					itemzTypeRecord,
					(Guid)resolvedProjectId,
					idMap);

				// Placement mode handling
				if (placementDto.FirstItemzTypeId.HasValue && placementDto.SecondItemzTypeId.HasValue)
				{
					await _itemzTypeRepository.MoveItemzTypeBetweenTwoHierarchyRecordsAsync(
						placementDto.FirstItemzTypeId.Value,
						placementDto.SecondItemzTypeId.Value,
						itemzTypeId);
				}
				else if (!placementDto.AtBottomOfChildNodes)
				{
					await _itemzTypeRepository.MoveItemzTypeToAnotherProjectAsync(
						movingItemzTypeId: itemzTypeId,
						targetProjectId: (Guid)resolvedProjectId,
						atBottomOfChildNodes: placementDto.AtBottomOfChildNodes
					);
				}
				await _itemzTypeRepository.SaveAsync();

				// Process trace links
				int traceCreated = await ProcessItemzTracesAsync(repositoryExportDto.ItemzTraces, idMap, result.Errors);

				_logger.LogInformation("Imported 1 ItemzType '{Name}' with {Count} Itemz at depth {Depth}. Traces: {Traces}. Root ID: {RootId}",
					itemzTypeRecord.ItemzType.Name, totalCreated, maxDepth, traceCreated, itemzTypeId);

				result.Success = true;
				result.ImportedRootId = itemzTypeId;
				result.ImportSummary = new ImportSummaryDTO
				{
					TotalCreated = totalCreated,
					Depth = maxDepth,
					TotalTraces = traceCreated
				};
				result.ItemzIdMapping = idMap;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to import ItemzType.");
				result.Errors.Add("Internal error during ItemzType import.");
			}

			return result;
		}


		private async Task<(Guid RootId, int TotalCreated, int MaxDepth)> ImportSingleItemzTypeAsync(
													ItemzTypeImportNode itemzTypeNode,
													Guid targetProjectId,
													Dictionary<Guid, Guid> idMap)
		{
			var itemzTypeDto = itemzTypeNode.ItemzType;

			// Check for System "Parking Lot"
			if (itemzTypeDto.Name == "Parking Lot" && itemzTypeDto.IsSystem)
			{
				var existingItemzTypes = await _projectRepository.GetItemzTypesAsync(targetProjectId);

				var systemParkingLot = existingItemzTypes?
					.FirstOrDefault(it =>
						string.Equals(it.Name, "Parking Lot", StringComparison.OrdinalIgnoreCase) &&
						it.IsSystem);

				if (systemParkingLot != null)
				{
					idMap[itemzTypeDto.Id] = systemParkingLot.Id;

					int totalCreated = 0;
					int maxDepth = 1;

					foreach (var itemzNode in itemzTypeNode.Itemz ?? Enumerable.Empty<ItemzImportNode>())
					{
						var (_, created, depth) = await ImportItemzRecursivelyWithStats(
							itemzNode,
							systemParkingLot.Id,
							2,
							idMap);

						totalCreated += created;
						maxDepth = Math.Max(maxDepth, depth);
					}

					// Log merge event
					_logger.LogInformation("Merged imported Itemz into existing System 'Parking Lot' ItemzType with ID {Id}", systemParkingLot.Id);

					return (systemParkingLot.Id, totalCreated, maxDepth);
				}
			}

			// Normal creation flow
			var createDto = new CreateItemzTypeDTO
			{
				ProjectId = targetProjectId,
				Name = itemzTypeDto.Name,
				Status = itemzTypeDto.Status,
				Description = itemzTypeDto.Description
			};

			var itemzTypeEntity = _mapper.Map<ItemzType>(createDto);
			_itemzTypeRepository.AddItemzType(itemzTypeEntity);
			await _itemzTypeRepository.SaveAsync();

			await _itemzTypeRepository.AddNewItemzTypeHierarchyAsync(itemzTypeEntity);
			await _itemzTypeRepository.SaveAsync();

			idMap[itemzTypeDto.Id] = itemzTypeEntity.Id;

			int normalCreated = 0;
			int normalDepth = 1;

			foreach (var itemzNode in itemzTypeNode.Itemz ?? Enumerable.Empty<ItemzImportNode>())
			{
				var (_, created, depth) = await ImportItemzRecursivelyWithStats(
					itemzNode,
					itemzTypeEntity.Id,
					2,
					idMap);

				normalCreated += created;
				normalDepth = Math.Max(normalDepth, depth);
			}

			return (itemzTypeEntity.Id, normalCreated, normalDepth);
		}



		public async Task<ImportResult> ImportProjectHierarchyAsync(
			RepositoryExportDTO repositoryExportDto,
			ImportDataPlacementDTO placementDto)
		{
			var result = new ImportResult
			{
				Success = false,
				Errors = new List<string>()
			};

			if (repositoryExportDto.Projects == null || repositoryExportDto.Projects.Count != 1)
			{
				result.Errors.Add("Expected exactly one Project to import.");
				return result;
			}

			try
			{
				var projectRecord = repositoryExportDto.Projects.First();
				var idMap = new Dictionary<Guid, Guid>();

				var (projectId, totalCreated, maxDepth) = await ImportSingleProjectAsync(projectRecord, idMap);

				// 📎 Process trace links across all Itemz in the project
				int traceCreated = await ProcessItemzTracesAsync(repositoryExportDto.ItemzTraces, idMap, result.Errors);

				_logger.LogInformation("Imported 1 Project '{Name}' with {Count} Itemz across {Types} ItemzTypes at depth {Depth}. Traces: {Traces}. Root ID: {RootId}",
					projectRecord.Project.Name,
					totalCreated,
					projectRecord.ItemzTypes?.Count ?? 0,
					maxDepth,
					traceCreated,
					projectId);

				result.Success = true;
				result.ImportedRootId = projectId;
				result.ImportSummary = new ImportSummaryDTO
				{
					TotalCreated = totalCreated,
					Depth = maxDepth,
					TotalTraces = traceCreated
				};
				result.ItemzIdMapping = idMap;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to import Project.");
				result.Errors.Add("Internal error during Project import.");
			}

			return result;
		}






		private async Task<(Guid ProjectId, int TotalCreated, int MaxDepth)> ImportSingleProjectAsync(
			ProjectImportNode projectNode,
			Dictionary<Guid, Guid> idMap)
		{
			var projectDto = projectNode.Project;

			var safeProjectName = GenerateImportSafeProjectName(projectDto.Name);


			var createDto = new CreateProjectDTO
			{
				Name = safeProjectName,
				Status = projectDto.Status,
				Description = projectDto.Description,
				// CreatedBy = projectDto.CreatedBy,
				// CreatedDate = projectDto.CreatedDate
			};

			var projectEntity = _mapper.Map<Project>(createDto);
			_projectRepository.AddProject(projectEntity);
			await _projectRepository.SaveAsync();

			try
			{
				await _projectRepository.AddNewProjectHierarchyAsync(projectEntity);
				await _projectRepository.SaveAsync();
			}
			catch (Microsoft.EntityFrameworkCore.DbUpdateException dbUpdateException)
			{
				_logger.LogError(dbUpdateException, "Failed to add project hierarchy for imported project '{Name}'", projectEntity.Name);
				throw new InvalidOperationException($"Could not establish hierarchy for project '{projectEntity.Name}'.", dbUpdateException);
			}

			idMap[projectDto.Id] = projectEntity.Id;

			int totalCreated = 0;
			int maxDepth = 1;

			foreach (var itemzTypeNode in projectNode.ItemzTypes ?? Enumerable.Empty<ItemzTypeImportNode>())
			{
				var (_, created, depth) = await ImportSingleItemzTypeAsync(
					itemzTypeNode,
					projectEntity.Id,
					idMap);

				totalCreated += created;
				maxDepth = Math.Max(maxDepth, depth);
			}

			return (projectEntity.Id, totalCreated, maxDepth);
		}




		public async Task<ImportResult> ImportBaselineItemzAsync(
										RepositoryExportDTO repositoryExportDto,
										ImportDataPlacementDTO placementDto)
		{
			// Transform BaselineItemz hierarchy to Itemz-compatible format
			var itemzNodes = repositoryExportDto.BaselineItemz
				.Select(BaselineImportTransformationUtility.TransformBaselineNodeToItemzNode)
				.ToList();

			var itemzTraces = BaselineImportTransformationUtility.TransformBaselineTracesToItemzTraces(repositoryExportDto.BaselineItemzTraces ?? new List<BaselineItemzTraceDTO>());

			var convertedRepository = new RepositoryExportDTO
			{
				Itemz = itemzNodes,
				ItemzTraces = itemzTraces
			};

			// Call into existing Itemz import logic
			return await ImportAsync(convertedRepository, "Itemz", placementDto);
		}




		public async Task<ImportResult> ImportBaselineItemzTypeAsync(
										RepositoryExportDTO repositoryExportDto,
										ImportDataPlacementDTO placementDto)
		{
			var result = new ImportResult
			{
				Success = false,
				Errors = new List<string>()
			};

			// Validate input
			if (repositoryExportDto.BaselineItemzTypes == null || !repositoryExportDto.BaselineItemzTypes.Any())
			{
				result.Errors.Add("No BaselineItemzType records found in the import data.");
				return result;
			}

			try
			{
				// Transform BaselineItemzTypes → ItemzTypes
				var itemzTypeNodes = repositoryExportDto.BaselineItemzTypes
					.Select(BaselineImportTransformationUtility.TransformBaselineItemzTypeToItemzTypeNode)
					.ToList();

				// Transform BaselineItemzTraces → ItemzTraces
				var itemzTraces = BaselineImportTransformationUtility
					.TransformBaselineTracesToItemzTraces(repositoryExportDto.BaselineItemzTraces ?? new List<BaselineItemzTraceDTO>());

				// Build converted repository DTO
				var convertedRepository = new RepositoryExportDTO
				{
					ItemzTypes = itemzTypeNodes,
					ItemzTraces = itemzTraces
				};

				// Delegate to existing ItemzType import method
				var itemzTypeResult = await ImportItemzTypeHierarchyAsync(convertedRepository, placementDto);

				// Return delegated result
				return itemzTypeResult;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception during BaselineItemzType import.");
				result.Errors.Add("Internal error occurred during BaselineItemzType import.");
				return result;
			}
		}


		public async Task<ImportResult> ImportBaselineAsProjectAsync(
			RepositoryExportDTO repositoryExportDto,
			ImportDataPlacementDTO placementDto)
		{
			var result = new ImportResult();

			if (repositoryExportDto?.Baselines == null || !repositoryExportDto.Baselines.Any())
			{
				result.Success = false;
				result.Errors = new List<string> { "No Baselines found to import." };
				return result;
			}

			try
			{
				var projectNodes = repositoryExportDto.Baselines
					.Select(BaselineImportTransformationUtility.TransformBaselineToProject)
					.ToList();

				var convertedRepository = new RepositoryExportDTO
				{
					Projects = projectNodes,
					ItemzTraces = BaselineImportTransformationUtility
						.TransformBaselineTracesToItemzTraces(repositoryExportDto.BaselineItemzTraces ?? new List<BaselineItemzTraceDTO>())
				};

				return await ImportProjectHierarchyAsync(convertedRepository, placementDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to import Baseline as Project.");
				return new ImportResult
				{
					Success = false,
					Errors = new List<string> { "Error occurred during Baseline import." }
				};
			}
		}




		private async Task<int> ProcessItemzTracesAsync(
						IEnumerable<ItemzTraceDTO>? traceDtos,
						Dictionary<Guid, Guid> idMap,
						List<string> errorList)
		{
			int traceCreated = 0;

			foreach (var trace in traceDtos ?? Enumerable.Empty<ItemzTraceDTO>())
			{
				if (idMap.TryGetValue(trace.FromTraceItemzId, out var fromId) &&
					idMap.TryGetValue(trace.ToTraceItemzId, out var toId))
				{
					//TODO :: We should move Normalize Tracelabel  to a helper method
					//		  so that it can be reused in other places as well.
					// Normalize TraceLabel: trim, treat empty as null, and enforce max length 32
					string? label = string.IsNullOrWhiteSpace(trace.TraceLabel) ? null : trace.TraceLabel.Trim();
					if (label != null && label.Length > 32)
					{
						_logger.LogWarning("TraceLabel too long for trace {From}->{To}; truncating to 32 chars",
							trace.FromTraceItemzId, trace.ToTraceItemzId);
						label = label.Substring(0, 32);
					}

					await _traceRepository.EstablishTraceBetweenItemzAsync(new ItemzTraceDTO
					{
						FromTraceItemzId = fromId,
						ToTraceItemzId = toId,
						TraceLabel = label
					});

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
				errorList.Add("Warning: Trace links may not have been saved.");
				_logger.LogWarning("Trace save failed after import.");
			}

			return traceCreated;
		}

		private string GenerateImportSafeProjectName(string originalName)
		{
			const int maxLength = 128;
			string newSuffix = $"IMP {Random.Shared.Next(10000, 99999)}";

			string trimmedName = originalName?.Trim() ?? "";

			// Detect and remove existing IMP suffix (e.g., "IMP 31244")
			var impSuffixRegex = new Regex(@"IMP\s\d{5}$", RegexOptions.IgnoreCase);
			if (impSuffixRegex.IsMatch(trimmedName))
			{
				trimmedName = impSuffixRegex.Replace(trimmedName, "").TrimEnd();
			}

			// Make room for the new suffix
			int availableLength = maxLength - newSuffix.Length - 1; // space between name and suffix
			if (trimmedName.Length > availableLength)
			{
				trimmedName = trimmedName.Substring(0, availableLength);
			}

			return $"{trimmedName} {newSuffix}";
		}



	}
}
