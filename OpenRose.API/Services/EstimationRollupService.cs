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
	/// Service responsible for calculating and updating roll-up estimations in the project hierarchy.
	/// Supports both individual record updates and bulk recalculations for performance optimization.
	/// PHASE 1: Handles roll-up calculations for Live Project Hierarchy Data
	/// </summary>
	public class EstimationRollupService
	{
		private readonly ItemzContext _context;
		private readonly ILogger<EstimationRollupService> _logger;
		private const int MaxExecutionTimeMs = 2000; // 2 seconds max execution time as per requirements

		public EstimationRollupService(ItemzContext context, ILogger<EstimationRollupService> logger)
		{
			_context = context;
			_logger = logger;
		}

		/// <summary>
		/// Recalculates roll-up estimations for all records under a specific project.
		/// This is an on-demand operation that user can trigger manually.
		/// PHASE 1: Used for Live Project hierarchy data
		/// </summary>
		/// <param name="projectHierarchyRecordId">The ID of the Project hierarchy record to recalculate</param>
		/// <returns>True if successful, False if operation timed out or failed</returns>
		public async Task<bool> RecalculateProjectRollUpEstimationsAsync(Guid projectHierarchyRecordId)
		{
			try
			{
				var startTime = DateTime.UtcNow;

				_logger.LogInformation($"Starting roll-up estimation recalculation for Project hierarchy record ID: {projectHierarchyRecordId}");

				// Get the project hierarchy record
				var projectRecord = await _context.ItemzHierarchy!
					.FirstOrDefaultAsync(ih => ih.Id == projectHierarchyRecordId);

				if (projectRecord == null)
				{
					_logger.LogWarning($"Project hierarchy record not found for ID: {projectHierarchyRecordId}");
					return false;
				}

				// Get all descendants of this project record
				var allProjectDescendants = await _context.ItemzHierarchy!
					.AsNoTracking()
					.Where(ih => ih.ItemzHierarchyId!.IsDescendantOf(projectRecord.ItemzHierarchyId!))
					.OrderByDescending(ih => ih.ItemzHierarchyId!.GetLevel()) // Process from deepest to shallowest
					.ToListAsync();

				// PHASE 1: Calculate roll-ups from bottom to top
				// This ensures child calculations are complete before parent calculations
				foreach (var hierarchyRecord in allProjectDescendants)
				{
					// Check execution time to avoid long-running operations
					if ((DateTime.UtcNow - startTime).TotalMilliseconds > MaxExecutionTimeMs)
					{
						_logger.LogWarning($"Roll-up estimation recalculation exceeded maximum execution time of {MaxExecutionTimeMs}ms for Project ID: {projectHierarchyRecordId}. Operation cancelled to prevent blocking.");
						return false;
					}

					// Calculate roll-up for this record
					await RecalculateSingleRecordRollUpAsync(hierarchyRecord.Id);
				}

				_logger.LogInformation($"Successfully completed roll-up estimation recalculation for Project hierarchy record ID: {projectHierarchyRecordId} in {(DateTime.UtcNow - startTime).TotalMilliseconds}ms");
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError($"Exception occurred while recalculating roll-up estimations for Project ID {projectHierarchyRecordId}: {ex.Message}", ex);
				// PHASE 1: Gentle error handling - log error but don't crash the application
				return false;
			}
		}

		/// <summary>
		/// Recalculates roll-up estimation for a single hierarchy record based on its children.
		/// This is called after any change to a record's own estimation or hierarchy structure.
		/// PHASE 1: Triggered by record updates (own estimation changes, record moved, etc.)
		/// </summary>
		/// <param name="hierarchyRecordId">The ID of the hierarchy record to recalculate</param>
		/// <returns>True if successful, False otherwise</returns>
		public async Task<bool> RecalculateSingleRecordRollUpAsync(Guid hierarchyRecordId)
		{
			try
			{
				var hierarchyRecord = await _context.ItemzHierarchy!
					.FirstOrDefaultAsync(ih => ih.Id == hierarchyRecordId);

				if (hierarchyRecord == null)
				{
					_logger.LogWarning($"Hierarchy record not found for ID: {hierarchyRecordId}");
					return false;
				}

				// Calculate new roll-up value: own estimation + sum of all immediate children's rolled-up estimations
				decimal newRolledUpEstimation = hierarchyRecord.OwnEstimation;

				// Get all immediate children
				var immediateChildren = await _context.ItemzHierarchy!
					.Where(ih => ih.ItemzHierarchyId!.GetAncestor(1) == hierarchyRecord.ItemzHierarchyId!)
					.ToListAsync();

				// Add all children's rolled-up estimations to this record's roll-up
				foreach (var child in immediateChildren)
				{
					newRolledUpEstimation += child.RolledUpEstimation;
				}

				// Update the record if roll-up value changed
				if (hierarchyRecord.RolledUpEstimation != newRolledUpEstimation)
				{
					hierarchyRecord.RolledUpEstimation = newRolledUpEstimation;
					_context.ItemzHierarchy!.Update(hierarchyRecord);
					await _context.SaveChangesAsync();

					_logger.LogDebug($"Updated roll-up estimation for hierarchy record ID {hierarchyRecordId}. New value: {newRolledUpEstimation}");
				}

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError($"Exception occurred while recalculating roll-up for record ID {hierarchyRecordId}: {ex.Message}", ex);
				return false;
			}
		}

		/// <summary>
		/// Sets estimation unit for all records in a project hierarchy.
		/// This is typically called when a project's estimation unit is defined or updated.
		/// PHASE 1: Ensures all hierarchy records have consistent estimation unit
		/// </summary>
		/// <param name="projectHierarchyRecordId">The ID of the Project hierarchy record</param>
		/// <param name="estimationUnit">The estimation unit to set (e.g., "Days", "$", "Story Points")</param>
		/// <returns>Count of records updated</returns>
		public async Task<int> SetEstimationUnitForProjectAsync(Guid projectHierarchyRecordId, string estimationUnit)
		{
			try
			{
				var projectRecord = await _context.ItemzHierarchy!
					.FirstOrDefaultAsync(ih => ih.Id == projectHierarchyRecordId);

				if (projectRecord == null)
				{
					_logger.LogWarning($"Project hierarchy record not found for ID: {projectHierarchyRecordId}");
					return 0;
				}

				// Get all descendants including the project itself
				var allRecords = await _context.ItemzHierarchy!
					.Where(ih => ih.Id == projectHierarchyRecordId ||
								 ih.ItemzHierarchyId!.IsDescendantOf(projectRecord.ItemzHierarchyId!))
					.ToListAsync();

				int updatedCount = 0;
				foreach (var record in allRecords)
				{
					if (record.EstimationUnit != estimationUnit)
					{
						record.EstimationUnit = estimationUnit;
						updatedCount++;
					}
				}

				if (updatedCount > 0)
				{
					await _context.SaveChangesAsync();
					_logger.LogInformation($"Updated estimation unit to '{estimationUnit}' for {updatedCount} records in project ID {projectHierarchyRecordId}");
				}

				return updatedCount;
			}
			catch (Exception ex)
			{
				_logger.LogError($"Exception occurred while setting estimation unit for project ID {projectHierarchyRecordId}: {ex.Message}", ex);
				return 0;
			}
		}

		/// <summary>
		/// Optimized method: Delta-based propagation
		/// Used for: Single record updates (real-time)
		/// </summary>
		public async Task<bool> RecalculateSingleRecordRollUpOptimizedAsync(
			Guid hierarchyRecordId,
			decimal estimationDelta)
		{

			var startTime = DateTime.UtcNow;

			try
			{
				var currentRecord = await _context.ItemzHierarchy!
					.FirstOrDefaultAsync(ih => ih.Id == hierarchyRecordId);

				if (currentRecord == null || currentRecord.ItemzHierarchyId == null)
				{
					_logger.LogWarning($"Hierarchy record not found for ID: {hierarchyRecordId}");
					return false;
				}

				// Check if we've reached the Project level (Level 1) BEFORE updating
				int currentLevel = currentRecord.ItemzHierarchyId.GetLevel();

				if (currentLevel == 1)
				{

					currentRecord.RolledUpEstimation += estimationDelta;
					_context.ItemzHierarchy!.Update(currentRecord);
					await _context.SaveChangesAsync();

					_logger.LogInformation(
						$"Updated rolled-up estimation for Project {hierarchyRecordId} by delta {estimationDelta}. " +
						$"New value: {currentRecord.RolledUpEstimation}");

					_logger.LogInformation(
						$"Current record {hierarchyRecordId} is at Project level (Level 1). " +
						$"Applying delta and STOPPING propagation.");

					return true; // STOP - don't propagate beyond Project level
				}

				// Apply the delta
				currentRecord.RolledUpEstimation += estimationDelta;
				_context.ItemzHierarchy!.Update(currentRecord);
				await _context.SaveChangesAsync();

				_logger.LogInformation(
					$"Updated rolled-up estimation for {hierarchyRecordId} by delta {estimationDelta}. " +
					$"New value: {currentRecord.RolledUpEstimation}");

				// Get immediate parent and continue
				var parentHierarchyId = currentRecord.ItemzHierarchyId.GetAncestor(1);
				if (parentHierarchyId != null)
				{
					var parentRecord = await _context.ItemzHierarchy!
						.FirstOrDefaultAsync(ih => ih.ItemzHierarchyId == parentHierarchyId);

					if (parentRecord != null)
					{
						// Recurse for ancestors
						// await RecalculateSingleRecordRollUpOptimizedAsync(parentRecord.Id, estimationDelta);

						// Add to target parent and its ancestry chain up to Project level
						bool additionSuccess = await AddRollUpToAncestryChainAsync(
							parentRecord,
							estimationDelta,
							startTime);

						if (!additionSuccess)
						{
							_logger.LogWarning(
								$"Step 4 - Failed to add roll-up estimation to target parent ancestry chain.");
							// Non-fatal error - operation still considered successful
						}
						else
						{
							_logger.LogDebug(
								$"Step 4 - Successfully added {estimationDelta} to target parent ancestry chain");
						}
					}
				}

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError($"Exception in RecalculateSingleRecordRollUpOptimizedAsync: {ex.Message}", ex);
				return false;
			}
		}

		#region Record Move Operations

		/// <summary>
		/// Updates roll-up estimations when a hierarchy record moves from one parent to another.
		/// This method implements a delta-based approach by deducting from the old ancestor chain
		/// and adding to the new ancestor chain. Must be called AFTER the record has been successfully moved
		/// in the hierarchy structure.
		/// 
		/// Flow:
		/// Step 0: Early exit if current parent equals target parent (no changes needed)
		/// Step 1: Obtain the moving record's existing rolled-up estimation value (trust existing value)
		/// Step 2: Identify the moving record's current (old) parent
		/// Step 3: Deduct rolled-up estimation from old parent and entire ancestor chain up to Project level
		/// Step 4: Add rolled-up estimation to new parent and entire ancestor chain up to Project level
		/// 
		/// PHASE 1: Non-fatal error handling - logs errors but doesn't fail the operation
		/// </summary>
		/// <param name="movingRecordId">The ID of the record that has been moved</param>
		/// <param name="currentParentId">The ID of the current (old) parent before the move</param>
		/// <param name="targetParentId">The ID of the target (new) parent after the move</param>
		/// <returns>True if successful or if move is within same parent, False only if critical validation fails</returns>
		public async Task<bool> UpdateRollUpEstimationsForRecordMoveAsync(
			Guid movingRecordId,
			Guid currentParentId,
			Guid targetParentId)
		{
			try
			{
				var startTime = DateTime.UtcNow;

				_logger.LogInformation(
					$"Starting roll-up estimation update for record move. " +
					$"MovingRecordId: {movingRecordId}, " +
					$"CurrentParentId: {currentParentId}, " +
					$"TargetParentId: {targetParentId}");

				// STEP 0: Early exit if moving to same parent (Scenario 1)
				if (currentParentId == targetParentId)
				{
					_logger.LogInformation(
						$"Record {movingRecordId} moving to same parent {targetParentId}. " +
						$"No roll-up estimation updates required (Scenario 1).");
					return true;
				}

				// Check execution time
				if ((DateTime.UtcNow - startTime).TotalMilliseconds > MaxExecutionTimeMs)
				{
					_logger.LogWarning(
						$"Roll-up estimation update for record move exceeded maximum execution time of {MaxExecutionTimeMs}ms. " +
						$"Operation cancelled to prevent blocking.");
					return false;
				}

				// STEP 1: Get the moving record and obtain its rolled-up estimation value
				var movingRecord = await _context.ItemzHierarchy!
					.FirstOrDefaultAsync(ih => ih.Id == movingRecordId);

				if (movingRecord == null)
				{
					_logger.LogWarning($"Moving record not found for ID: {movingRecordId}");
					return false;
				}

				decimal rolledUpEstimationDelta = movingRecord.RolledUpEstimation;

				_logger.LogDebug(
					$"Step 1 - Retrieved moving record {movingRecordId} with rolled-up estimation: {rolledUpEstimationDelta}");

				//// Check execution time
				//if ((DateTime.UtcNow - startTime).TotalMilliseconds > MaxExecutionTimeMs)
				//{
				//	_logger.LogWarning($"Operation exceeded maximum execution time during record retrieval.");
				//	return false;
				//}

				// STEP 2: Get current (old) parent record
				var currentParent = await _context.ItemzHierarchy!
					.FirstOrDefaultAsync(ih => ih.Id == currentParentId);

				if (currentParent == null)
				{
					_logger.LogWarning($"Current parent record not found for ID: {currentParentId}. Skipping deduction phase.");
					// Non-fatal: continue to addition phase
				}
				else
				{
					_logger.LogDebug(
						$"Step 2 - Current parent: Id={currentParentId}, Type={currentParent.RecordType}, " +
						$"Level={currentParent.ItemzHierarchyId!.GetLevel()}");

					// STEP 3: Deduct from current parent and its ancestry chain up to Project level
					bool deductionSuccess = await DeductRollUpFromAncestryChainAsync(
						currentParent,
						rolledUpEstimationDelta,
						startTime);

					if (!deductionSuccess)
					{
						_logger.LogWarning(
							$"Step 3 - Failed to deduct roll-up estimation from current parent ancestry chain. " +
							$"Continuing with addition phase...");
						// Non-fatal error - continue to addition phase
					}
					else
					{
						_logger.LogDebug(
							$"Step 3 - Successfully deducted {rolledUpEstimationDelta} from current parent ancestry chain");
					}
				}

				//// Check execution time
				//if ((DateTime.UtcNow - startTime).TotalMilliseconds > MaxExecutionTimeMs)
				//{
				//	_logger.LogWarning($"Operation exceeded maximum execution time during deduction phase.");
				//	return false;
				//}

				// STEP 4: Get target (new) parent record
				var targetParent = await _context.ItemzHierarchy!
					.FirstOrDefaultAsync(ih => ih.Id == targetParentId);

				if (targetParent == null)
				{
					_logger.LogWarning($"Target parent record not found for ID: {targetParentId}. Skipping addition phase.");
					// Non-fatal: at least deduction was performed
					return true;
				}

				_logger.LogDebug(
					$"Step 4 - Target parent: Id={targetParentId}, Type={targetParent.RecordType}, " +
					$"Level={targetParent.ItemzHierarchyId!.GetLevel()}");

				// Add to target parent and its ancestry chain up to Project level
				bool additionSuccess = await AddRollUpToAncestryChainAsync(
					targetParent,
					rolledUpEstimationDelta,
					startTime);

				if (!additionSuccess)
				{
					_logger.LogWarning(
						$"Step 4 - Failed to add roll-up estimation to target parent ancestry chain.");
					// Non-fatal error - operation still considered successful
				}
				else
				{
					_logger.LogDebug(
						$"Step 4 - Successfully added {rolledUpEstimationDelta} to target parent ancestry chain");
				}

				_logger.LogInformation(
					$"Successfully completed roll-up estimation update for record move. " +
					$"RecordId: {movingRecordId}, Delta: {rolledUpEstimationDelta}, " +
					$"Duration: {(DateTime.UtcNow - startTime).TotalMilliseconds}ms. " +
					$"(Scenario 2/3 - Different parents)");

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(
					$"Exception occurred while updating roll-up estimations for record move. " +
					$"MovingRecordId: {movingRecordId}, CurrentParentId: {currentParentId}, " +
					$"TargetParentId: {targetParentId}. Exception: {ex.Message}",
					ex);
				// PHASE 1: Gentle error handling - log error but don't crash
				return false;
			}
		}

		/// <summary>
		/// Helper method: Deducts roll-up estimation value from a parent and traverses up the ancestor chain.
		/// Stops at Project level (Level 1) without going to Repository level (Level 0).
		/// Used in Step 3 of the move operation to remove the moving record's estimation from old ancestor chain.
		/// </summary>
		/// <param name="parentRecord">The immediate parent from which to start deduction</param>
		/// <param name="estimationDelta">The rolled-up estimation value to deduct</param>
		/// <param name="operationStartTime">The start time of the parent operation for timeout checking</param>
		/// <returns>True if successful, False if operation failed or timed out</returns>
		private async Task<bool> DeductRollUpFromAncestryChainAsync(
			ItemzHierarchy parentRecord,
			decimal estimationDelta,
			DateTime operationStartTime)
		{
			try
			{
				var currentRecord = parentRecord;
				int processedCount = 0;

				_logger.LogDebug($"Starting deduction phase for delta {estimationDelta}");

				while (currentRecord != null)
				{
					// Check execution time
					if ((DateTime.UtcNow - operationStartTime).TotalMilliseconds > MaxExecutionTimeMs)
					{
						_logger.LogWarning(
							$"Deduction operation exceeded maximum execution time. " +
							$"Processed {processedCount} records before timeout.");
						return false;
					}

					int currentLevel = currentRecord.ItemzHierarchyId!.GetLevel();

					// Deduct from current record
					decimal previousValue = currentRecord.RolledUpEstimation;
					currentRecord.RolledUpEstimation -= estimationDelta;
					_context.ItemzHierarchy!.Update(currentRecord);
					await _context.SaveChangesAsync();

					_logger.LogDebug(
						$"Deduction: Record {currentRecord.Id} (Type: {currentRecord.RecordType}, Level: {currentLevel}). " +
						$"Previous: {previousValue}, Delta: -{estimationDelta}, New: {currentRecord.RolledUpEstimation}");

					processedCount++;

					// STOP at Project level (Level 1) - don't go to Repository level (Level 0)
					if (currentLevel == 1)
					{
						_logger.LogDebug($"Reached Project level (Level 1). Stopping deduction traversal.");
						break;
					}

					// Get immediate parent and continue
					var parentHierarchyId = currentRecord.ItemzHierarchyId.GetAncestor(1);
					if (parentHierarchyId != null)
					{
						currentRecord = await _context.ItemzHierarchy!
							.FirstOrDefaultAsync(ih => ih.ItemzHierarchyId == parentHierarchyId);

						if (currentRecord == null)
						{
							_logger.LogWarning(
								$"Parent record not found while traversing deduction chain. " +
								$"HierarchyId: {parentHierarchyId}. Stopping traversal.");
							break;
						}
					}
					else
					{
						_logger.LogDebug($"No parent found for current record. Stopping deduction traversal.");
						break;
					}
				}

				_logger.LogDebug($"Completed deduction phase. Total records updated: {processedCount}");
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError($"Exception occurred during deduction phase: {ex.Message}", ex);
				return false;
			}
		}

		/// <summary>
		/// Helper method: Adds roll-up estimation value to a parent and traverses up the ancestor chain.
		/// Stops at Project level (Level 1) without going to Repository level (Level 0).
		/// Used in Step 4 of the move operation to add the moving record's estimation to new ancestor chain.
		/// </summary>
		/// <param name="parentRecord">The immediate parent to which to start addition</param>
		/// <param name="estimationDelta">The rolled-up estimation value to add</param>
		/// <param name="operationStartTime">The start time of the parent operation for timeout checking</param>
		/// <returns>True if successful, False if operation failed or timed out</returns>
		private async Task<bool> AddRollUpToAncestryChainAsync(
			ItemzHierarchy parentRecord,
			decimal estimationDelta,
			DateTime operationStartTime)
		{
			try
			{
				var currentRecord = parentRecord;
				int processedCount = 0;

				_logger.LogDebug($"Starting addition phase for delta {estimationDelta}");

				while (currentRecord != null)
				{
					// Check execution time
					if ((DateTime.UtcNow - operationStartTime).TotalMilliseconds > MaxExecutionTimeMs)
					{
						_logger.LogWarning(
							$"Addition operation exceeded maximum execution time. " +
							$"Processed {processedCount} records before timeout.");
						return false;
					}

					int currentLevel = currentRecord.ItemzHierarchyId!.GetLevel();

					// Add to current record
					decimal previousValue = currentRecord.RolledUpEstimation;
					currentRecord.RolledUpEstimation += estimationDelta;
					_context.ItemzHierarchy!.Update(currentRecord);
					await _context.SaveChangesAsync();

					_logger.LogDebug(
						$"Addition: Record {currentRecord.Id} (Type: {currentRecord.RecordType}, Level: {currentLevel}). " +
						$"Previous: {previousValue}, Delta: +{estimationDelta}, New: {currentRecord.RolledUpEstimation}");

					processedCount++;

					// STOP at Project level (Level 1) - don't go to Repository level (Level 0)
					if (currentLevel == 1)
					{
						_logger.LogDebug($"Reached Project level (Level 1). Stopping addition traversal.");
						break;
					}

					// Get immediate parent and continue
					var parentHierarchyId = currentRecord.ItemzHierarchyId.GetAncestor(1);
					if (parentHierarchyId != null)
					{
						currentRecord = await _context.ItemzHierarchy!
							.FirstOrDefaultAsync(ih => ih.ItemzHierarchyId == parentHierarchyId);

						if (currentRecord == null)
						{
							_logger.LogWarning(
								$"Parent record not found while traversing addition chain. " +
								$"HierarchyId: {parentHierarchyId}. Stopping traversal.");
							break;
						}
					}
					else
					{
						_logger.LogDebug($"No parent found for current record. Stopping addition traversal.");
						break;
					}
				}

				_logger.LogDebug($"Completed addition phase. Total records updated: {processedCount}");
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError($"Exception occurred during addition phase: {ex.Message}", ex);
				return false;
			}
		}

		#endregion

		///// <summary>
		///// Propagates delta through all ancestors (wrapper for optimized method)
		///// Used for: Batch processing of hierarchy updates
		///// </summary>
		//public async Task<bool> PropagateEstimationDeltaToAncestorsAsync(Guid recordId, decimal delta)
		//{
		//	try
		//	{
		//		var currentRecord = await _context.ItemzHierarchy!
		//			.FirstOrDefaultAsync(ih => ih.Id == recordId);

		//		if (currentRecord == null || currentRecord.ItemzHierarchyId == null)
		//		{
		//			_logger.LogWarning($"Record not found for ID: {recordId}");
		//			return false;
		//		}

		//		// Get parent and start propagation
		//		var parentHierarchyId = currentRecord.ItemzHierarchyId.GetAncestor(1);
		//		if (parentHierarchyId != null)
		//		{
		//			var parentRecord = await _context.ItemzHierarchy!
		//				.FirstOrDefaultAsync(ih => ih.ItemzHierarchyId == parentHierarchyId);

		//			if (parentRecord != null)
		//			{
		//				return await RecalculateSingleRecordRollUpOptimizedAsync(parentRecord.Id, delta);
		//			}
		//		}

		//		return true;
		//	}
		//	catch (Exception ex)
		//	{
		//		_logger.LogError($"Exception in PropagateEstimationDeltaToAncestorsAsync: {ex.Message}", ex);
		//		return false;
		//	}
		//}
	}
}