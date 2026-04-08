// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using ItemzApp.API.DbContexts;
using ItemzApp.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ItemzApp.API.Services
{
	/// <summary>
	/// PHASE 1: Service that handles trigger events for ItemzHierarchy changes
	/// Automatically recalculates roll-up estimations when specific events occur:
	/// - New Itemz/ItemzType added with estimation != 0
	/// - Itemz/ItemzType estimation value changed
	/// - Itemz/ItemzType moved to new parent (handles both record types)
	/// - Itemz/ItemzType deleted
	/// - Itemz/ItemzType becomes orphaned
	/// - Itemz/ItemzType copied
	/// </summary>
	public class ItemzHierarchyTriggerService
	{
		private readonly ItemzContext _context;
		private readonly ILogger<ItemzHierarchyTriggerService> _logger;
		private readonly EstimationRollupService _estimationRollupService;

		public ItemzHierarchyTriggerService(
			ItemzContext context,
			ILogger<ItemzHierarchyTriggerService> logger,
			EstimationRollupService estimationRollupService)
		{
			_context = context;
			_logger = logger;
			_estimationRollupService = estimationRollupService;
		}

		/// <summary>
		/// PHASE 1: Trigger Event 1 - New Itemz Added
		/// Called when a new Itemz is added to the hierarchy with estimation value
		/// </summary>
		/// <param name="itemzHierarchyRecordId">The ID of the newly created ItemzHierarchy record</param>
		/// <returns>True if successful</returns>
		public async Task<bool> OnItemzAddedAsync(Guid itemzHierarchyRecordId)
		{
			try
			{
				_logger.LogInformation($"PHASE 1 TRIGGER: New Itemz added with hierarchy record ID: {itemzHierarchyRecordId}");

				var hierarchyRecord = await _context.ItemzHierarchy!
					.FirstOrDefaultAsync(ih => ih.Id == itemzHierarchyRecordId);

				if (hierarchyRecord == null)
				{
					_logger.LogWarning($"ItemzHierarchy record not found for ID: {itemzHierarchyRecordId}");
					return false;
				}

				// PHASE 1: Only trigger recalculation if own estimation is not zero
				if (hierarchyRecord.OwnEstimation != 0)
				{
					return await RecalculateParentHierarchyAsync(hierarchyRecord.Id);
				}

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError($"Exception in OnItemzAddedAsync: {ex.Message}", ex);
				return false;
			}
		}

		/// <summary>
		/// PHASE 1: Trigger Event 3 - Hierarchy Record Moved to New Parent
		/// Called when a record (Itemz or ItemzType) is moved from one parent to another
		/// Uses delta-based roll-up estimation propagation for efficiency
		/// Handles all hierarchy record types: Itemz, ItemzType
		/// </summary>
		/// <param name="movingHierarchyRecordId">The ID of the moved hierarchy record</param>
		/// <param name="previousParentHierarchyId">Optional: ID of previous parent before the move</param>
		/// <returns>True if successful</returns>
		public async Task<bool> OnHierarchyRecordMovedAsync(Guid movingHierarchyRecordId, Guid? previousParentHierarchyId = null)
		{
			try
			{
				_logger.LogInformation(
					$"PHASE 1 TRIGGER: Hierarchy record moved. Moving record ID: {movingHierarchyRecordId}, " +
					$"Previous parent: {(previousParentHierarchyId.HasValue ? previousParentHierarchyId.ToString() : "None")}");

				var movingHierarchyRecord = await _context.ItemzHierarchy!
					.FirstOrDefaultAsync(ih => ih.Id == movingHierarchyRecordId);

				if (movingHierarchyRecord == null)
				{
					_logger.LogWarning($"Hierarchy record not found for ID: {movingHierarchyRecordId}");
					return false;
				}

				if (movingHierarchyRecord.ItemzHierarchyId == null)
				{
					_logger.LogWarning($"HierarchyId path is null for record ID: {movingHierarchyRecordId}");
					return false;
				}

				// Get the current (new) parent after the move
				var currentParentHierarchyIdPath = movingHierarchyRecord.ItemzHierarchyId.GetAncestor(1);
				if (currentParentHierarchyIdPath == null)
				{
					_logger.LogWarning(
						$"Could not determine current parent HierarchyId path for moving record {movingHierarchyRecordId}");
					return false;
				}

				var currentParentRecord = await _context.ItemzHierarchy!
					.FirstOrDefaultAsync(ih => ih.ItemzHierarchyId == currentParentHierarchyIdPath);

				if (currentParentRecord == null)
				{
					_logger.LogWarning(
						$"Current parent record not found for HierarchyId path: {currentParentHierarchyIdPath}");
					return false;
				}

				Guid currentParentHierarchyId = currentParentRecord.Id;

				_logger.LogDebug(
					$"PHASE 1 TRIGGER: Moving record {movingHierarchyRecordId} (Type: {movingHierarchyRecord.RecordType}) " +
					$"from parent {previousParentHierarchyId} to parent {currentParentHierarchyId}");

				// PHASE 1: Use new optimized method for move-based roll-up updates
				// This handles all scenarios: same parent, different parent, cross-type moves
				if (previousParentHierarchyId.HasValue && previousParentHierarchyId != Guid.Empty)
				{
					var moveUpdateResult = await _estimationRollupService.UpdateRollUpEstimationsForRecordMoveAsync(
						movingHierarchyRecordId,
						previousParentHierarchyId.Value,
						currentParentHierarchyId);

					if (!moveUpdateResult)
					{
						_logger.LogWarning(
							$"PHASE 1 TRIGGER: Roll-up estimation update for move failed. " +
							$"Moving record: {movingHierarchyRecordId} (Type: {movingHierarchyRecord.RecordType}), " +
							$"Previous parent: {previousParentHierarchyId}, " +
							$"Current parent: {currentParentHierarchyId}");
						// Non-fatal: continue despite failure
					}
					else
					{
						_logger.LogInformation(
							$"PHASE 1 TRIGGER: Successfully updated roll-up estimations for moved record. " +
							$"Moving record: {movingHierarchyRecordId} (Type: {movingHierarchyRecord.RecordType})");
					}
				}
				else
				{
					_logger.LogInformation(
						$"PHASE 1 TRIGGER: No previous parent available. " +
						$"Skipping roll-up estimation update for moved record {movingHierarchyRecordId} " +
						$"(Type: {movingHierarchyRecord.RecordType})");
				}

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(
					$"Exception in OnHierarchyRecordMovedAsync for moving record {movingHierarchyRecordId}: {ex.Message}", ex);
				return false;
			}
		}

		///// <summary>
		///// PHASE 1: Trigger Event 4 - Itemz Deleted
		///// Called when an Itemz is deleted from the hierarchy
		///// Recalculates parent hierarchy to reflect removed estimation
		///// </summary>
		///// <param name="parentItemzHierarchyId">The ID of the parent ItemzHierarchy record</param>
		///// <returns>True if successful</returns>
		//public async Task<bool> OnItemzDeletedAsync(Guid parentItemzHierarchyId)
		//{
		//	try
		//	{
		//		_logger.LogInformation($"PHASE 1 TRIGGER: Itemz deleted. Parent hierarchy record ID: {parentItemzHierarchyId}");

		//		return await RecalculateParentHierarchyAsync(parentItemzHierarchyId);
		//	}
		//	catch (Exception ex)
		//	{
		//		_logger.LogError($"Exception in OnItemzDeletedAsync: {ex.Message}", ex);
		//		return false;
		//	}
		//}
		///// <summary>
		///// PHASE 1: Trigger Event 5 - Itemz Becomes Orphaned
		///// Called when an Itemz becomes orphaned (parent deleted, not part of any project)
		///// Orphaned Itemz lose their hierarchy record as per design
		///// </summary>
		///// <param name="orphanedItemzId">The ID of the Itemz that became orphaned</param>
		///// <returns>True if successful</returns>
		//public async Task<bool> OnItemzBecameOrphanedAsync(Guid orphanedItemzId)
		//{
		//	try
		//	{
		//		_logger.LogInformation($"PHASE 1 TRIGGER: Itemz became orphaned. Itemz ID: {orphanedItemzId}");

		//		// PHASE 1: Get the hierarchy record before it's deleted
		//		var hierarchyRecord = await _context.ItemzHierarchy!
		//			.FirstOrDefaultAsync(ih => ih.Id == orphanedItemzId);

		//		if (hierarchyRecord != null && hierarchyRecord.ItemzHierarchyId != null)
		//		{
		//			// Get parent before hierarchy record is removed
		//			var parentHierarchyId = hierarchyRecord.ItemzHierarchyId.GetAncestor(1);
		//			if (parentHierarchyId != null)
		//			{
		//				var parentRecord = await _context.ItemzHierarchy!
		//					.FirstOrDefaultAsync(ih => ih.ItemzHierarchyId == parentHierarchyId);

		//				if (parentRecord != null)
		//				{
		//					// Recalculate parent's roll-up now that this child is being removed
		//					return await _estimationRollupService.RecalculateSingleRecordRollUpAsync(parentRecord.Id);
		//				}
		//			}
		//		}

		//		return true;
		//	}
		//	catch (Exception ex)
		//	{
		//		_logger.LogError($"Exception in OnItemzBecameOrphanedAsync: {ex.Message}", ex);
		//		return false;
		//	}
		//}

		/// <summary>
		/// PHASE 1: Trigger Event 6 - Itemz Copied
		/// Called when an existing Itemz is copied (with or without child records)
		/// The copy inherits the original's estimation unit and own estimation
		/// </summary>
		/// <param name="copiedItemzHierarchyId">The ID of the newly copied ItemzHierarchy record</param>
		/// <returns>True if successful</returns>
		public async Task<bool> OnItemzCopiedAsync(Guid copiedItemzHierarchyId)
		{
			try
			{
				_logger.LogInformation($"PHASE 1 TRIGGER: Itemz copied. Copied hierarchy record ID: {copiedItemzHierarchyId}");

				var copiedHierarchyRecord = await _context.ItemzHierarchy!
					.FirstOrDefaultAsync(ih => ih.Id == copiedItemzHierarchyId);

				if (copiedHierarchyRecord == null)
				{
					_logger.LogWarning($"Copied ItemzHierarchy record not found for ID: {copiedItemzHierarchyId}");
					return false;
				}

				// PHASE 1: Only trigger recalculation if copied Itemz has estimation value
				if (copiedHierarchyRecord.OwnEstimation != 0)
				{
					return await RecalculateParentHierarchyAsync(copiedItemzHierarchyId);
				}

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError($"Exception in OnItemzCopiedAsync: {ex.Message}", ex);
				return false;
			}
		}

		/// <summary>
		/// PHASE 1: Similar trigger events for ItemzType records
		/// ItemzType added with estimation value
		/// </summary>
		/// <param name="itemzTypeHierarchyRecordId">The ID of the newly created ItemzTypeHierarchy record</param>
		/// <returns>True if successful</returns>
		public async Task<bool> OnItemzTypeAddedAsync(Guid itemzTypeHierarchyRecordId)
		{
			try
			{
				_logger.LogInformation($"PHASE 1 TRIGGER: New ItemzType added with hierarchy record ID: {itemzTypeHierarchyRecordId}");

				var hierarchyRecord = await _context.ItemzHierarchy!
					.FirstOrDefaultAsync(ih => ih.Id == itemzTypeHierarchyRecordId);

				if (hierarchyRecord == null)
				{
					_logger.LogWarning($"ItemzHierarchy record not found for ID: {itemzTypeHierarchyRecordId}");
					return false;
				}

				// PHASE 1: Only trigger recalculation if own estimation is not zero
				if (hierarchyRecord.OwnEstimation != 0)
				{
					return await RecalculateParentHierarchyAsync(itemzTypeHierarchyRecordId);
				}

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError($"Exception in OnItemzTypeAddedAsync: {ex.Message}", ex);
				return false;
			}
		}

		///// <summary>
		///// PHASE 1: ItemzType deleted
		///// </summary>
		///// <param name="parentHierarchyRecordId">The ID of the parent ItemzHierarchy record</param>
		///// <returns>True if successful</returns>
		//public async Task<bool> OnItemzTypeDeletedAsync(Guid parentHierarchyRecordId)
		//{
		//	try
		//	{
		//		_logger.LogInformation($"PHASE 1 TRIGGER: ItemzType deleted. Parent hierarchy record ID: {parentHierarchyRecordId}");

		//		return await RecalculateParentHierarchyAsync(parentHierarchyRecordId);
		//	}
		//	catch (Exception ex)
		//	{
		//		_logger.LogError($"Exception in OnItemzTypeDeletedAsync: {ex.Message}", ex);
		//		return false;
		//	}
		//}


		#region On Record Deletion
		/// <summary>
		/// PHASE 1: Trigger Event 4 - Hierarchy Record Deleted
		/// Called when a hierarchy record (Itemz or ItemzType) is deleted from the hierarchy
		/// Uses delta-based roll-up estimation deduction to update parent ancestry chain
		/// Handles all hierarchy record types: Itemz, ItemzType
		/// </summary>
		/// <param name="deletedRecordRolledUpEstimation">The rolled-up estimation value of the deleted record (needed for delta calculation)</param>
		/// <param name="parentHierarchyRecordId">The ID of the parent ItemzHierarchy record</param>
		/// <returns>True if successful</returns>
		public async Task<bool> OnHierarchyRecordDeletedAsync(decimal deletedRecordRolledUpEstimation, Guid parentHierarchyRecordId)
		{
			try
			{
				_logger.LogInformation(
					$"PHASE 1 TRIGGER: Hierarchy record deleted. " +
					$"Deleted record rolled-up estimation: {deletedRecordRolledUpEstimation}, " +
					$"Parent hierarchy record ID: {parentHierarchyRecordId}");

				// If no parent or no estimation delta, nothing to update
				if (parentHierarchyRecordId == Guid.Empty || deletedRecordRolledUpEstimation == 0)
				{
					_logger.LogDebug(
						$"PHASE 1 TRIGGER: Skipping roll-up estimation update. " +
						$"ParentId is empty: {parentHierarchyRecordId == Guid.Empty}, " +
						$"EstimationDelta is zero: {deletedRecordRolledUpEstimation == 0}");
					return true;
				}

				// Get the parent record to determine its parent (grandparent of deleted record)
				var parentRecord = await _context.ItemzHierarchy!
					.FirstOrDefaultAsync(ih => ih.Id == parentHierarchyRecordId);

				if (parentRecord == null)
				{
					_logger.LogWarning($"Parent hierarchy record not found for ID: {parentHierarchyRecordId}");
					return false;
				}

				if (parentRecord.ItemzHierarchyId == null)
				{
					_logger.LogWarning($"HierarchyId path is null for parent record ID: {parentHierarchyRecordId}");
					return false;
				}

				// Get the current (parent of the deleted record's parent) for deduction propagation
				var currentParentHierarchyIdPath = parentRecord.ItemzHierarchyId.GetAncestor(1);

				_logger.LogDebug(
					$"PHASE 1 TRIGGER: Deleted record (Type: {(await _context.ItemzHierarchy!.FirstOrDefaultAsync(ih => ih.Id == parentHierarchyRecordId))?.RecordType}) " +
					$"with rolled-up estimation: {deletedRecordRolledUpEstimation} from parent {parentHierarchyRecordId}");

				// PHASE 1: Use optimized method for deletion-based roll-up updates
				// This deducts the deleted record's estimation from entire ancestor chain up to Project level
				if (currentParentHierarchyIdPath != null)
				{
					var currentParentRecord = await _context.ItemzHierarchy!
						.FirstOrDefaultAsync(ih => ih.ItemzHierarchyId == currentParentHierarchyIdPath);

					if (currentParentRecord != null)
					{
						var deductionResult = await _estimationRollupService.UpdateRollUpEstimationsForRecordDeletionAsync(
							parentHierarchyRecordId,
							deletedRecordRolledUpEstimation);

						if (!deductionResult)
						{
							_logger.LogWarning(
								$"PHASE 1 TRIGGER: Roll-up estimation deduction for deletion failed. " +
								$"Parent record: {parentHierarchyRecordId}, " +
								$"Estimation delta: {deletedRecordRolledUpEstimation}");
							// Non-fatal: continue despite failure
						}
						else
						{
							_logger.LogInformation(
								$"PHASE 1 TRIGGER: Successfully updated roll-up estimations for deleted record. " +
								$"Deducted {deletedRecordRolledUpEstimation} from parent chain");
						}
					}
				}
				else
				{
					// Parent has no parent (is at Project level or higher), only update the immediate parent
					await _estimationRollupService.RecalculateSingleRecordRollUpAsync(parentHierarchyRecordId);
					_logger.LogDebug(
						$"PHASE 1 TRIGGER: Parent record is at top level. " +
						$"Recalculated parent {parentHierarchyRecordId}");
				}

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(
					$"Exception in OnHierarchyRecordDeletedAsync: {ex.Message}", ex);
				return false;
			}
		}

		#endregion

		/// <summary>
		/// PHASE 1: ItemzType copied
		/// </summary>
		/// <param name="copiedItemzTypeHierarchyId">The ID of the newly copied ItemzTypeHierarchy record</param>
		/// <returns>True if successful</returns>
		public async Task<bool> OnItemzTypeCopiedAsync(Guid copiedItemzTypeHierarchyId)
		{
			try
			{
				_logger.LogInformation($"PHASE 1 TRIGGER: ItemzType copied. Copied hierarchy record ID: {copiedItemzTypeHierarchyId}");

				var copiedHierarchyRecord = await _context.ItemzHierarchy!
					.FirstOrDefaultAsync(ih => ih.Id == copiedItemzTypeHierarchyId);

				if (copiedHierarchyRecord == null)
				{
					_logger.LogWarning($"Copied ItemzTypeHierarchy record not found for ID: {copiedItemzTypeHierarchyId}");
					return false;
				}

				// PHASE 1: Only trigger recalculation if copied ItemzType has estimation value
				if (copiedHierarchyRecord.OwnEstimation != 0)
				{
					return await RecalculateParentHierarchyAsync(copiedItemzTypeHierarchyId);
				}

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError($"Exception in OnItemzTypeCopiedAsync: {ex.Message}", ex);
				return false;
			}
		}

		// ============= PRIVATE HELPER METHODS =============

		/// <summary>
		/// PHASE 1: Helper method to recalculate parent and all ancestors
		/// Called after a child record changes
		/// </summary>
		private async Task<bool> RecalculateParentHierarchyAsync(Guid childHierarchyRecordId)
		{
			try
			{
				var childRecord = await _context.ItemzHierarchy!
					.FirstOrDefaultAsync(ih => ih.Id == childHierarchyRecordId);

				if (childRecord == null || childRecord.ItemzHierarchyId == null)
				{
					return false;
				}

				// Get parent hierarchy
				var parentHierarchyId = childRecord.ItemzHierarchyId.GetAncestor(1);
				if (parentHierarchyId == null)
				{
					return true; // No parent to recalculate
				}

				var parentRecord = await _context.ItemzHierarchy!
					.FirstOrDefaultAsync(ih => ih.ItemzHierarchyId == parentHierarchyId);

				if (parentRecord != null)
				{
					// PHASE 1: Recalculate immediate parent
					await _estimationRollupService.RecalculateSingleRecordRollUpAsync(parentRecord.Id);

					// PHASE 1: Also recalculate all ancestors up the hierarchy
					await RecalculateAncestorsAsync(parentRecord.Id);
				}

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogWarning($"Exception in RecalculateParentHierarchyAsync: {ex.Message}");
				return false;
			}
		}

		/// <summary>
		/// PHASE 1: Helper method to recalculate all ancestor records up the hierarchy
		/// Ensures complete hierarchy consistency
		/// </summary>
		private async Task<bool> RecalculateAncestorsAsync(Guid recordId)
		{
			try
			{
				var currentRecord = await _context.ItemzHierarchy!
					.FirstOrDefaultAsync(ih => ih.Id == recordId);

				if (currentRecord == null || currentRecord.ItemzHierarchyId == null)
				{
					return false;
				}

				// Get all ancestors by following the hierarchy upward
				var currentLevel = currentRecord.ItemzHierarchyId.GetLevel();

				for (int i = currentLevel - 1; i > 0; i--)
				{
					var ancestorHierarchyId = currentRecord.ItemzHierarchyId.GetAncestor(currentLevel - i);
					if (ancestorHierarchyId == null)
					{
						break;
					}

					var ancestorRecord = await _context.ItemzHierarchy!
						.FirstOrDefaultAsync(ih => ih.ItemzHierarchyId == ancestorHierarchyId);

					if (ancestorRecord != null)
					{
						await _estimationRollupService.RecalculateSingleRecordRollUpAsync(ancestorRecord.Id);
					}
				}

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogWarning($"Exception in RecalculateAncestorsAsync: {ex.Message}");
				return false;
			}
		}
	}
}