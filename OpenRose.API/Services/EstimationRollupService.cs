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
	}
}