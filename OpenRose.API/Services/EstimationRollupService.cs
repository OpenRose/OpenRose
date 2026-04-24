// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using ItemzApp.API.DbContexts;
using ItemzApp.API.Entities;
using Microsoft.Data.SqlClient;
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
		/// 
		/// Uses optimized SQL Server stored procedure for superior performance and scalability.
		/// The stored procedure uses CTE-based recursive query combined with set-based UPDATE,
		/// leveraging SQL Server's built-in HierarchyId optimizations.
		/// 
		/// Single database roundtrip with minimal server load and memory usage.
		/// </summary>
		/// <param name="projectHierarchyRecordId">The ID of the Project hierarchy record to recalculate</param>
		/// <returns>True if successful, False if operation failed</returns>
		public async Task<bool> RecalculateProjectRollUpEstimationsAsync(Guid projectHierarchyRecordId)
		{
			if (projectHierarchyRecordId == Guid.Empty)
			{
				throw new ArgumentNullException(nameof(projectHierarchyRecordId));
			}

			// EXPLANATION: Check if record exists in ItemzHierarchy table
			if (!_context.ItemzHierarchy!.AsNoTracking()
							.Where(ih => ih.Id == projectHierarchyRecordId)
							.Any())
			{
				throw new ArgumentException("Project hierarchy record not found", nameof(projectHierarchyRecordId));
			}

			try
			{
				var sqlParameters = new[]
				{
					new SqlParameter
					{
						ParameterName = "ProjectHierarchyRecordId",
						Value = projectHierarchyRecordId,
						SqlDbType = System.Data.SqlDbType.UniqueIdentifier
					}
				};

				// EXPLANATION:
				// Matches your existing pattern for calling stored procedures.
				// No OUTPUT parameter needed because your SP does not return one.
				await _context.Database.ExecuteSqlRawAsync(
					"EXEC userProcRecalculateProjectRollUpEstimations @ProjectHierarchyRecordId",
					sqlParameters);

				return true;
			}
			catch (SqlException sqlEx)
			{
				_logger.LogError(
					$"RecalculateProjectRollUpEstimationsAsync: SQL Exception occurred while recalculating roll-up estimations " +
					$"for Project ID {projectHierarchyRecordId}: {sqlEx.Message}", sqlEx);

				return false;
			}
			catch (Exception ex)
			{
				_logger.LogError(
					$"RecalculateProjectRollUpEstimationsAsync: Exception occurred while recalculating roll-up estimations " +
					$"for Project ID {projectHierarchyRecordId}: {ex.Message}", ex);

				return false;
			}
		}



		/// <summary>
		/// Recalculates roll-up estimations for all records under a specific baseline.
		/// This is an on-demand operation that user can trigger manually.
		/// PHASE 5: Used for Baseline hierarchy data with INCLUDED/EXCLUDED support
		/// 
		/// Uses optimized SQL Server stored procedure for superior performance and scalability.
		/// The stored procedure uses CTE-based recursive query combined with set-based UPDATE,
		/// leveraging SQL Server's built-in HierarchyId optimizations.
		/// 
		/// Respects the isIncluded flag for each BaselineItemz record:
		/// - If EXCLUDED: RolledUpEstimation = ZERO (never contributes to parent)
		/// - If INCLUDED: RolledUpEstimation = OwnEstimation + SUM(INCLUDED descendants' OwnEstimation)
		/// 
		/// Single database roundtrip with minimal server load and memory usage.
		/// </summary>
		/// <param name="baselineHierarchyRecordId">The ID of the Baseline hierarchy record to recalculate</param>
		/// <returns>True if successful, False if operation failed</returns>
		public async Task<bool> RecalculateBaselineRollUpEstimationsAsync(Guid baselineHierarchyRecordId)
		{
			if (baselineHierarchyRecordId == Guid.Empty)
			{
				throw new ArgumentNullException(nameof(baselineHierarchyRecordId));
			}

			// EXPLANATION: Check if record exists in BaselineItemzHierarchy table
			if (!_context.BaselineItemzHierarchy!.AsNoTracking()
							.Where(bih => bih.Id == baselineHierarchyRecordId)
							.Any())
			{
				throw new ArgumentException("Baseline hierarchy record not found", nameof(baselineHierarchyRecordId));
			}

			try
			{
				var sqlParameters = new[]
				{
			new SqlParameter
			{
				ParameterName = "BaselineHierarchyRecordId",
				Value = baselineHierarchyRecordId,
				SqlDbType = System.Data.SqlDbType.UniqueIdentifier
			}
		};

				// EXPLANATION:
				// Matches your existing pattern for calling stored procedures.
				// No OUTPUT parameter needed because your SP does not return one.
				// Calls userProcRecalculateBaselineRollUpEstimations which handles isIncluded flag logic.
				await _context.Database.ExecuteSqlRawAsync(
					"EXEC userProcRecalculateBaselineRollUpEstimations @BaselineHierarchyRecordId",
					sqlParameters);

				return true;
			}
			catch (SqlException sqlEx)
			{
				_logger.LogError(
					$"RecalculateBaselineRollUpEstimationsAsync: SQL Exception occurred while recalculating roll-up estimations " +
					$"for Baseline ID {baselineHierarchyRecordId}: {sqlEx.Message}", sqlEx);

				return false;
			}
			catch (Exception ex)
			{
				_logger.LogError(
					$"RecalculateBaselineRollUpEstimationsAsync: Exception occurred while recalculating roll-up estimations " +
					$"for Baseline ID {baselineHierarchyRecordId}: {ex.Message}", ex);

				return false;
			}
		}


		/// <summary>
		/// Synchronizes EstimationUnit for all descendants to match the Project record's value.
		/// This is called after updating a Project's EstimationUnit to ensure all child records are synchronized.
		/// PHASE 1: Ensures all hierarchy records have EstimationUnit synchronized to Project level
		/// 
		/// Uses optimized SQL Server stored procedure for superior performance and scalability.
		/// The stored procedure uses CTE-based recursive query combined with set-based UPDATE,
		/// leveraging SQL Server's built-in HierarchyId optimizations.
		/// 
		/// Project's EstimationUnit value always takes precedence and overrides all child values.
		/// Handles case-sensitive comparison and trims whitespace.
		/// Safe to call at any time - synchronizes to current Project EstimationUnit value.
		/// 
		/// Single database roundtrip with minimal server load and memory usage.
		/// </summary>
		/// <param name="projectHierarchyRecordId">The ID of the Project hierarchy record</param>
		/// <returns>True if successful, False if operation failed</returns>
		public async Task<bool> SetEstimationUnitForProjectAsync(Guid projectHierarchyRecordId)
		{
			if (projectHierarchyRecordId == Guid.Empty)
			{
				throw new ArgumentNullException(nameof(projectHierarchyRecordId));
			}

			// EXPLANATION: Check if record exists in ItemzHierarchy table
			if (!_context.ItemzHierarchy!.AsNoTracking()
							.Where(ih => ih.Id == projectHierarchyRecordId)
							.Any())
			{
				throw new ArgumentException("Project hierarchy record not found", nameof(projectHierarchyRecordId));
			}

			try
			{
				var sqlParameters = new[]
				{
					new SqlParameter
					{
						ParameterName = "ProjectHierarchyRecordId",
						Value = projectHierarchyRecordId,
						SqlDbType = System.Data.SqlDbType.UniqueIdentifier
					}
				};

				// EXPLANATION:
				// Matches your existing pattern for calling stored procedures.
				// The stored procedure:
				// - Validates the project record exists and is type 'Project'
				// - Extracts EstimationUnit from Project record (with whitespace trimmed)
				// - Performs case-sensitive comparison against all child records
				// - Updates only child records where EstimationUnit differs from Project value
				// - Trims whitespace from all values before comparison and update
				// - Project's EstimationUnit always takes precedence (can be NULL or empty)
				// - Returns silently on success (no output parameter needed)
				await _context.Database.ExecuteSqlRawAsync(
					"EXEC userProcSetEstimationUnitForProject @ProjectHierarchyRecordId",
					sqlParameters);

				_logger.LogInformation(
					$"SetEstimationUnitForProjectAsync: Successfully synchronized EstimationUnit " +
					$"for all descendants of Project ID: {projectHierarchyRecordId}");

				return true;
			}
			catch (SqlException sqlEx)
			{
				_logger.LogError(
					$"SetEstimationUnitForProjectAsync: SQL Exception occurred while synchronizing EstimationUnit " +
					$"for Project ID {projectHierarchyRecordId}: {sqlEx.Message}", sqlEx);

				return false;
			}
			catch (Exception ex)
			{
				_logger.LogError(
					$"SetEstimationUnitForProjectAsync: Exception occurred while synchronizing EstimationUnit " +
					$"for Project ID {projectHierarchyRecordId}: {ex.Message}", ex);

				return false;
			}
		}


		/// <summary>
		/// Optimized method: Delta-based propagation for single record updates
		/// Used for: Single record updates (real-time)
		/// This is used when a single record's estimation changes and we need to propagate the delta upward
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
		/// Enhanced with comprehensive validation and logging for direct controller calls.
		/// 
		/// Flow:
		/// Step 0: Validate inputs and check if parents are the same (no update needed)
		/// Step 1: Obtain the moving record's existing rolled-up estimation value (trust existing value)
		/// Step 2: Identify the moving record's current (old) parent
		/// Step 3: Deduct rolled-up estimation from old parent and entire ancestor chain up to Project level
		/// Step 4: Add rolled-up estimation to new parent and entire ancestor chain up to Project level
		/// 
		/// PHASE 1: Non-fatal error handling - logs errors but doesn't fail the operation
		/// </summary>
		/// <param name="movingRecordId">The ID of the record that has been moved</param>
		/// <param name="previousParentId">The ID of the previous parent (old parent before move)</param>
		/// <param name="currentParentId">The ID of the current parent (new parent after move)</param>
		/// <returns>True if successful or if move is within same parent, False only if critical validation fails</returns>
		public async Task<bool> UpdateRollUpEstimationsForRecordMoveAsync(
			Guid movingRecordId,
			Guid previousParentId,
			Guid currentParentId)
		{
			try
			{
				var startTime = DateTime.UtcNow;

				// STEP 0: Validate inputs
				if (movingRecordId == Guid.Empty)
				{
					_logger.LogWarning("UpdateRollUpEstimationsForRecordMoveAsync: Moving record ID is empty");
					return false;
				}

				if (previousParentId == Guid.Empty)
				{
					_logger.LogWarning(
						$"UpdateRollUpEstimationsForRecordMoveAsync: Previous parent ID is empty for moving record {movingRecordId}. " +
						$"Skipping roll-up updates.");
					return true; // Not an error - record may have been orphaned before
				}

				if (currentParentId == Guid.Empty)
				{
					_logger.LogWarning(
						$"UpdateRollUpEstimationsForRecordMoveAsync: Current parent ID is empty for moving record {movingRecordId}. " +
						$"Record may have become orphaned.");
					return true; // Not an error - moved to orphan state
				}

				_logger.LogInformation(
					$"UpdateRollUpEstimationsForRecordMoveAsync: Starting roll-up estimation update for record move. " +
					$"MovingRecordId: {movingRecordId}, PreviousParentId: {previousParentId}, CurrentParentId: {currentParentId}");

				// Verify moving record exists
				var movingRecord = await _context.ItemzHierarchy!
					.FirstOrDefaultAsync(ih => ih.Id == movingRecordId);

				if (movingRecord == null)
				{
					_logger.LogWarning(
						$"UpdateRollUpEstimationsForRecordMoveAsync: Moving record not found for ID: {movingRecordId}");
					return false;
				}

				_logger.LogDebug(
					$"UpdateRollUpEstimationsForRecordMoveAsync: Moving record found. Type: {movingRecord.RecordType}, " +
					$"RolledUpEstimation: {movingRecord.RolledUpEstimation}");

				// STEP 0: Early exit if moving to same parent (Scenario 1)
				if (previousParentId == currentParentId)
				{
					_logger.LogInformation(
						$"UpdateRollUpEstimationsForRecordMoveAsync: Record {movingRecordId} moving to same parent {currentParentId}. " +
						$"No roll-up estimation updates required (Scenario 1 - Same Parent).");
					return true;
				}

				// Check execution time
				if ((DateTime.UtcNow - startTime).TotalMilliseconds > MaxExecutionTimeMs)
				{
					_logger.LogWarning(
						$"UpdateRollUpEstimationsForRecordMoveAsync: Operation exceeded maximum execution time of {MaxExecutionTimeMs}ms. " +
						$"Operation cancelled to prevent blocking.");
					return false;
				}

				// STEP 1: Get the moving record and obtain its rolled-up estimation value
				decimal rolledUpEstimationDelta = movingRecord.RolledUpEstimation;

				_logger.LogDebug(
					$"UpdateRollUpEstimationsForRecordMoveAsync: Step 1 - Retrieved moving record {movingRecordId} " +
					$"with rolled-up estimation: {rolledUpEstimationDelta}");

				// Early exit if no estimation to move
				if (rolledUpEstimationDelta == 0)
				{
					_logger.LogInformation(
						$"UpdateRollUpEstimationsForRecordMoveAsync: Moving record {movingRecordId} has zero estimation. " +
						$"No roll-up updates required.");
					return true;
				}

				// STEP 2: Get current (old) parent record
				var previousParent = await _context.ItemzHierarchy!
					.FirstOrDefaultAsync(ih => ih.Id == previousParentId);

				if (previousParent == null)
				{
					_logger.LogWarning(
						$"UpdateRollUpEstimationsForRecordMoveAsync: Previous parent record not found for ID: {previousParentId}. " +
						$"Skipping deduction phase.");
					// Non-fatal: continue to addition phase
				}
				else
				{
					_logger.LogDebug(
						$"UpdateRollUpEstimationsForRecordMoveAsync: Step 2 - Previous parent: Id={previousParentId}, " +
						$"Type={previousParent.RecordType}, Level={previousParent.ItemzHierarchyId!.GetLevel()}");

					// STEP 3: Deduct from previous parent and its ancestry chain up to Project level
					bool deductionSuccess = await DeductRollUpFromAncestryChainAsync(
						previousParent,
						rolledUpEstimationDelta,
						startTime);

					if (!deductionSuccess)
					{
						_logger.LogWarning(
							$"UpdateRollUpEstimationsForRecordMoveAsync: Failed to deduct roll-up estimation from previous parent ancestry chain. " +
							$"Continuing with addition phase...");
						// Non-fatal error - continue to addition phase
					}
					else
					{
						_logger.LogDebug(
							$"UpdateRollUpEstimationsForRecordMoveAsync: Step 3 - Successfully deducted {rolledUpEstimationDelta} " +
							$"from previous parent ancestry chain");
					}
				}

				// Check execution time
				if ((DateTime.UtcNow - startTime).TotalMilliseconds > MaxExecutionTimeMs)
				{
					_logger.LogWarning(
						$"UpdateRollUpEstimationsForRecordMoveAsync: Operation exceeded maximum execution time during deduction phase.");
					return false;
				}

				// STEP 4: Get current (new) parent record
				var currentParent = await _context.ItemzHierarchy!
					.FirstOrDefaultAsync(ih => ih.Id == currentParentId);

				if (currentParent == null)
				{
					_logger.LogWarning(
						$"UpdateRollUpEstimationsForRecordMoveAsync: Current parent record not found for ID: {currentParentId}. " +
						$"Skipping addition phase.");
					// Non-fatal: at least deduction was performed
					return true;
				}

				_logger.LogDebug(
					$"UpdateRollUpEstimationsForRecordMoveAsync: Step 4 - Current parent: Id={currentParentId}, " +
					$"Type={currentParent.RecordType}, Level={currentParent.ItemzHierarchyId!.GetLevel()}");

				// Add to current parent and its ancestry chain up to Project level
				bool additionSuccess = await AddRollUpToAncestryChainAsync(
					currentParent,
					rolledUpEstimationDelta,
					startTime);

				if (!additionSuccess)
				{
					_logger.LogWarning(
						$"UpdateRollUpEstimationsForRecordMoveAsync: Failed to add roll-up estimation to current parent ancestry chain.");
					// Non-fatal error - operation still considered successful
				}
				else
				{
					_logger.LogDebug(
						$"UpdateRollUpEstimationsForRecordMoveAsync: Step 4 - Successfully added {rolledUpEstimationDelta} " +
						$"to current parent ancestry chain");
				}

				_logger.LogInformation(
					$"UpdateRollUpEstimationsForRecordMoveAsync: Successfully completed roll-up estimation update for record move. " +
					$"RecordId: {movingRecordId} (Type: {movingRecord.RecordType}), Delta: {rolledUpEstimationDelta}, " +
					$"Duration: {(DateTime.UtcNow - startTime).TotalMilliseconds}ms " +
					$"(Scenario 2/3 - Different parents)");

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(
					$"UpdateRollUpEstimationsForRecordMoveAsync: Exception occurred while updating roll-up estimations for record move. " +
					$"MovingRecordId: {movingRecordId}, PreviousParentId: {previousParentId}, " +
					$"CurrentParentId: {currentParentId}. Exception: {ex.Message}",
					ex);
				// PHASE 1: Gentle error handling - log error but don't crash
				return false;
			}
		}

		#endregion

		#region Record Copy Operations

		/// <summary>
		/// Updates roll-up estimations when a hierarchy record is copied.
		/// This method implements a delta-based addition approach to add the copied record's
		/// estimation to the entire parent ancestor chain. Must be called AFTER the record has been successfully copied
		/// in the hierarchy structure.
		/// 
		/// Enhanced with comprehensive validation and logging for direct controller calls.
		/// Supports all record types: Itemz, ItemzType, and any other hierarchy record type.
		/// 
		/// Flow:
		/// Step 1: Validate input and obtain the copied record and its rolled-up estimation value
		/// Step 2: Identify the copied record's parent
		/// Step 3: Add rolled-up estimation to parent and entire ancestor chain up to Project level
		/// 
		/// PHASE 1: Non-fatal error handling - logs errors but doesn't fail the operation
		/// </summary>
		/// <param name="copiedRecordId">The ID of the copied hierarchy record</param>
		/// <returns>True if successful, False only if critical validation fails</returns>
		public async Task<bool> UpdateRollUpEstimationsForRecordCopyAsync(Guid copiedRecordId)
		{
			try
			{
				var startTime = DateTime.UtcNow;

				// Validate input
				if (copiedRecordId == Guid.Empty)
				{
					_logger.LogWarning("UpdateRollUpEstimationsForRecordCopyAsync: Copied record ID is empty");
					return false;
				}

				_logger.LogInformation(
					$"UpdateRollUpEstimationsForRecordCopyAsync: Starting roll-up estimation update for record copy. " +
					$"CopiedRecordId: {copiedRecordId}");

				// Check execution time
				if ((DateTime.UtcNow - startTime).TotalMilliseconds > MaxExecutionTimeMs)
				{
					_logger.LogWarning(
						$"UpdateRollUpEstimationsForRecordCopyAsync: Operation exceeded maximum execution time of {MaxExecutionTimeMs}ms. " +
						$"Operation cancelled to prevent blocking.");
					return false;
				}

				// STEP 1: Get the copied record and obtain its rolled-up estimation value
				var copiedRecord = await _context.ItemzHierarchy!
					.FirstOrDefaultAsync(ih => ih.Id == copiedRecordId);

				if (copiedRecord == null)
				{
					_logger.LogWarning(
						$"UpdateRollUpEstimationsForRecordCopyAsync: Copied record not found for ID: {copiedRecordId}");
					return false;
				}

				_logger.LogDebug(
					$"UpdateRollUpEstimationsForRecordCopyAsync: Step 1 - Retrieved copied record {copiedRecordId} " +
					$"(Type: {copiedRecord.RecordType}) with rolled-up estimation: {copiedRecord.RolledUpEstimation}");

				// Early exit if no estimation to add
				if (copiedRecord.RolledUpEstimation == 0)
				{
					_logger.LogInformation(
						$"UpdateRollUpEstimationsForRecordCopyAsync: Copied record {copiedRecordId} has zero estimation. " +
						$"No roll-up estimation updates required.");
					return true;
				}

				decimal rolledUpEstimationDelta = copiedRecord.RolledUpEstimation;

				// Check if record is in hierarchy (has parent)
				if (copiedRecord.ItemzHierarchyId == null)
				{
					_logger.LogInformation(
						$"UpdateRollUpEstimationsForRecordCopyAsync: Copied record {copiedRecordId} is orphaned (not in hierarchy). " +
						$"No roll-up estimation updates required.");
					return true;
				}

				// STEP 2: Get parent record
				var parentHierarchyIdPath = copiedRecord.ItemzHierarchyId.GetAncestor(1);
				if (parentHierarchyIdPath == null)
				{
					_logger.LogWarning(
						$"UpdateRollUpEstimationsForRecordCopyAsync: Could not determine parent HierarchyId path for copied record {copiedRecordId}");
					return false;
				}

				var parentRecord = await _context.ItemzHierarchy!
					.FirstOrDefaultAsync(ih => ih.ItemzHierarchyId == parentHierarchyIdPath);

				if (parentRecord == null)
				{
					_logger.LogWarning(
						$"UpdateRollUpEstimationsForRecordCopyAsync: Parent record not found for HierarchyId path: {parentHierarchyIdPath}");
					return false;
				}

				_logger.LogDebug(
					$"UpdateRollUpEstimationsForRecordCopyAsync: Step 2 - Parent record: Id={parentRecord.Id}, " +
					$"Type={parentRecord.RecordType}, Level={parentRecord.ItemzHierarchyId!.GetLevel()}");

				// Check execution time
				if ((DateTime.UtcNow - startTime).TotalMilliseconds > MaxExecutionTimeMs)
				{
					_logger.LogWarning(
						$"UpdateRollUpEstimationsForRecordCopyAsync: Operation exceeded maximum execution time during parent retrieval.");
					return false;
				}

				// STEP 3: Add to parent and its ancestry chain up to Project level
				bool additionSuccess = await AddRollUpToAncestryChainAsync(
					parentRecord,
					rolledUpEstimationDelta,
					startTime);

				if (!additionSuccess)
				{
					_logger.LogWarning(
						$"UpdateRollUpEstimationsForRecordCopyAsync: Failed to add roll-up estimation to parent ancestry chain. " +
						$"Continuing...");
					// Non-fatal error
				}
				else
				{
					_logger.LogDebug(
						$"UpdateRollUpEstimationsForRecordCopyAsync: Step 3 - Successfully added {rolledUpEstimationDelta} " +
						$"to parent ancestry chain");
				}

				_logger.LogInformation(
					$"UpdateRollUpEstimationsForRecordCopyAsync: Successfully completed roll-up estimation update for record copy. " +
					$"CopiedRecordId: {copiedRecordId} (Type: {copiedRecord.RecordType}), Delta: {rolledUpEstimationDelta}, " +
					$"Duration: {(DateTime.UtcNow - startTime).TotalMilliseconds}ms");

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(
					$"UpdateRollUpEstimationsForRecordCopyAsync: Exception occurred while updating roll-up estimations for record copy. " +
					$"CopiedRecordId: {copiedRecordId}. Exception: {ex.Message}",
					ex);
				// PHASE 1: Gentle error handling - log error but don't crash
				return false;
			}
		}

		#endregion

		#region Record Deletion Operations

		/// <summary>
		/// Updates roll-up estimations when a hierarchy record is deleted.
		/// This method implements a delta-based deduction approach to remove the deleted record's
		/// estimation from the entire parent ancestor chain. Must be called AFTER the record has been successfully deleted.
		/// 
		/// Enhanced with comprehensive validation and logging for direct controller calls.
		/// Supports all record types: Itemz, ItemzType, and any other hierarchy record type.
		/// 
		/// Flow:
		/// Step 1: Validate inputs and obtain the parent record
		/// Step 2: Deduct the deleted record's estimation from the parent
		/// Step 3: Recursively deduct from entire ancestor chain up to Project level
		/// 
		/// PHASE 1: Non-fatal error handling - logs errors but doesn't fail the operation
		/// </summary>
		/// <param name="parentRecordId">The ID of the parent record of the deleted record</param>
		/// <param name="deletedRecordRolledUpEstimation">The rolled-up estimation value of the deleted record (the delta to deduct)</param>
		/// <returns>True if successful, False only if critical validation fails</returns>
		public async Task<bool> UpdateRollUpEstimationsForRecordDeletionAsync(
			Guid parentRecordId,
			decimal deletedRecordRolledUpEstimation)
		{
			try
			{
				var startTime = DateTime.UtcNow;

				// Validate inputs
				if (parentRecordId == Guid.Empty)
				{
					_logger.LogWarning(
						"UpdateRollUpEstimationsForRecordDeletionAsync: Parent record ID is empty. " +
						"Skipping roll-up updates.");
					return true; // Not an error - deleted record may have been orphaned
				}

				if (deletedRecordRolledUpEstimation == 0)
				{
					_logger.LogInformation(
						$"UpdateRollUpEstimationsForRecordDeletionAsync: Deleted record has zero estimation. " +
						$"No roll-up estimation updates required.");
					return true;
				}

				_logger.LogInformation(
					$"UpdateRollUpEstimationsForRecordDeletionAsync: Starting roll-up estimation update for record deletion. " +
					$"ParentRecordId: {parentRecordId}, DeletedRecordEstimation: {deletedRecordRolledUpEstimation}");

				// Check execution time
				if ((DateTime.UtcNow - startTime).TotalMilliseconds > MaxExecutionTimeMs)
				{
					_logger.LogWarning(
						$"UpdateRollUpEstimationsForRecordDeletionAsync: Operation exceeded maximum execution time of {MaxExecutionTimeMs}ms. " +
						$"Operation cancelled to prevent blocking.");
					return false;
				}

				// STEP 1: Get the parent record
				var parentRecord = await _context.ItemzHierarchy!
					.FirstOrDefaultAsync(ih => ih.Id == parentRecordId);

				if (parentRecord == null)
				{
					_logger.LogWarning(
						$"UpdateRollUpEstimationsForRecordDeletionAsync: Parent record not found for ID: {parentRecordId}");
					return false;
				}

				_logger.LogDebug(
					$"UpdateRollUpEstimationsForRecordDeletionAsync: Step 1 - Retrieved parent record {parentRecordId} " +
					$"(Type: {parentRecord.RecordType}) for deduction chain");

				// Verify parent has a hierarchy path
				if (parentRecord.ItemzHierarchyId == null)
				{
					_logger.LogWarning(
						$"UpdateRollUpEstimationsForRecordDeletionAsync: HierarchyId path is null for parent record ID: {parentRecordId}");
					return false;
				}

				// STEP 2 & 3: Deduct from parent and its ancestry chain up to Project level
				bool deductionSuccess = await DeductRollUpFromAncestryChainAsync(
					parentRecord,
					deletedRecordRolledUpEstimation,
					startTime);

				if (!deductionSuccess)
				{
					_logger.LogWarning(
						$"UpdateRollUpEstimationsForRecordDeletionAsync: Failed to deduct roll-up estimation from parent ancestry chain. " +
						$"Parent: {parentRecordId}");
					// Non-fatal error
				}
				else
				{
					_logger.LogDebug(
						$"UpdateRollUpEstimationsForRecordDeletionAsync: Successfully deducted {deletedRecordRolledUpEstimation} " +
						$"from parent ancestry chain");
				}

				_logger.LogInformation(
					$"UpdateRollUpEstimationsForRecordDeletionAsync: Successfully completed roll-up estimation update for record deletion. " +
					$"ParentRecordId: {parentRecordId}, Delta: {deletedRecordRolledUpEstimation}, " +
					$"Duration: {(DateTime.UtcNow - startTime).TotalMilliseconds}ms");

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(
					$"UpdateRollUpEstimationsForRecordDeletionAsync: Exception occurred while updating roll-up estimations for record deletion. " +
					$"ParentRecordId: {parentRecordId}, DeletedRecordEstimation: {deletedRecordRolledUpEstimation}. " +
					$"Exception: {ex.Message}",
					ex);
				// PHASE 1: Gentle error handling - log error but don't crash
				return false;
			}
		}

		#endregion

		#region Common helper methods for hierarchy traversal and roll-up updates

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
	}
}