// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using ItemzApp.API.DbContexts;
using ItemzApp.API.DbContexts.Extensions;
using ItemzApp.API.DbContexts.SQLHelper;
using ItemzApp.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

namespace ItemzApp.API.Services
{
    public class BaselineItemzRepository : IBaselineItemzRepository, IDisposable
    {
        private readonly BaselineContext _baselineContext;

        public BaselineItemzRepository(BaselineContext baselineContext)
        {
            _baselineContext = baselineContext ?? throw new ArgumentNullException(nameof(baselineContext));
        }
        public async Task<BaselineItemz?> GetBaselineItemzAsync(Guid BaselineItemzId)
        {
            if (BaselineItemzId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(BaselineItemzId));
            }

            return await _baselineContext.BaselineItemz!
                .Where(bi => bi.Id == BaselineItemzId).AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<BaselineItemz>?> GetBaselineItemzByItemzIdAsync(Guid ItemzId)
        {
            if (ItemzId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(ItemzId));
            }

            var SqlParam_ItemzId = new SqlParameter("@__ItemzID__", ItemzId.ToString());

            return await _baselineContext.BaselineItemz.FromSqlRaw(SQLStatements.SQLStatementFor_GetBaselineItemzByItemzIdOrderByCreatedDate, SqlParam_ItemzId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> BaselineItemzExistsAsync(Guid baselineItemzId)
        {
            if (baselineItemzId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(baselineItemzId));
            }
            return await _baselineContext.BaselineItemz.AsNoTracking().AnyAsync(bi => bi.Id == baselineItemzId);
        }

        public async Task<int> GetBaselineItemzCountByItemzIdAsync(Guid ItemzId)
        {
            if (ItemzId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(ItemzId));
            }
            KeyValuePair<string, object>[] sqlArgs = new KeyValuePair<string, object>[]
            {
                new KeyValuePair<string, object>("@__ItemzId__", ItemzId.ToString()),
            };
            var foundBaselineItemzByItemzId = await _baselineContext.CountByRawSqlAsync(SQLStatements.SQLStatementFor_GetBaselineItemzByItemzId, sqlArgs);

            return foundBaselineItemzByItemzId;
        }

        public async Task<IEnumerable<BaselineItemz>> GetBaselineItemzsAsync(IEnumerable<Guid> baselineItemzIds)
        {
            if (baselineItemzIds == null)
            {
                throw new ArgumentNullException(nameof(baselineItemzIds));
            }

            return await _baselineContext.BaselineItemz.AsNoTracking().Where(bi => baselineItemzIds.Contains(bi.Id))
                .OrderBy(bi => bi.Name)
                .ToListAsync();
        }

        public async Task<bool> NOT_IN_USE_CheckBaselineitemzForInclusionBeforeImplementingAsync(UpdateBaselineItemz updateBaselineItemz)
        {
            // TODO :: We should implement all the checks that we are doing in this method within the 
            // Stored Procedure userProcUpdateBaselineItemz. 
            // Reason to include this same logic in the Stored Procedure is to makes sure that even by mistake we
            // do not get into situation where by we have child itemz included while it's parent itemz are not included. 

            // TODO :: Figure out how to truely implement this method as async. Currently we do not have await statement within this method. 

            var overallCheckPassedStatus = true;
            foreach (var baselineItemId in updateBaselineItemz.BaselineItemzIds!)
            {
                var immediateParentBaselineItemz = _baselineContext.BaselineItemzHierarchy!.AsNoTracking()
                    .Where(bih => bih.BaselineItemzHierarchyId ==
                                    _baselineContext.BaselineItemzHierarchy!.AsNoTracking()
                                    .Where(bih => bih.Id == baselineItemId)
                                    .FirstOrDefault()!.BaselineItemzHierarchyId!.GetAncestor(1)
                                  &&
                                      bih.BaselineItemzHierarchyId!.IsDescendantOf(
                                      _baselineContext.BaselineItemzHierarchy!.AsNoTracking()
                                        .Where(bih => bih.Id == updateBaselineItemz.BaselineId).FirstOrDefault()!.BaselineItemzHierarchyId
                                  ) == true
                                  &&
                                  bih.BaselineItemzHierarchyId!.GetLevel() > 2  // Above Baseline to allow BaselineItemzType
                    );
                    //.Where(bih => bih.BaselineItemzHierarchyId!.IsDescendantOf(
                    //                  _baselineContext.BaselineItemzHierarchy!.AsNoTracking()
                    //                    .Where(bih => bih.Id == updateBaselineItemz.BaselineId).FirstOrDefault()!.BaselineItemzHierarchyId
                    //              ) == true
                    //);
               
                if (immediateParentBaselineItemz != null)
                {
                    if(immediateParentBaselineItemz.FirstOrDefault()!.isIncluded ==  false)
                    {
                        overallCheckPassedStatus = false;
                        break;
                    }
                }
                else
                { 
                    overallCheckPassedStatus = false;
                    break;
                }
            }
            return overallCheckPassedStatus;  
        }

        public async Task<bool> UpdateBaselineItemzsAsync(UpdateBaselineItemz updateBaselineItemz)
        {
            if (updateBaselineItemz.BaselineId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(updateBaselineItemz.BaselineId));
            }

            if (updateBaselineItemz.BaselineItemzIds is null)
            {
                throw new ArgumentNullException(nameof(updateBaselineItemz.BaselineItemzIds));
            }

            if (!(updateBaselineItemz.BaselineItemzIds.Any()))
            {
                throw new ArgumentNullException(nameof(updateBaselineItemz.BaselineItemzIds));
            }

            if (!_baselineContext.Baseline!.Where(b => b.Id == updateBaselineItemz.BaselineId).Any())
            {
                throw new ArgumentException(nameof(updateBaselineItemz.BaselineId));
            }
            var tempListofIds = updateBaselineItemz.BaselineItemzIds.ToList();

            StringBuilder csvBaselineItemzIds = new StringBuilder("");
            for (int i = 0; i < tempListofIds.Count(); i++)
            {
                csvBaselineItemzIds.Append(tempListofIds[i].ToString());
                if (i != tempListofIds.Count()-1)
                { 
                    csvBaselineItemzIds.Append(",");
                }
            }

            var OUTPUT_isSuccessful = new SqlParameter
            {
                ParameterName = "OUTPUT_Success",
                Direction = System.Data.ParameterDirection.Output,
                SqlDbType = System.Data.SqlDbType.Bit,
            };

            var sqlParameters = new[]
            {
                new SqlParameter
                {
                    ParameterName = "BaselineId",
                    Value = updateBaselineItemz.BaselineId,
                    SqlDbType = System.Data.SqlDbType.UniqueIdentifier,
                },
                new SqlParameter
                {
                    ParameterName = "ShouldBeIncluded",
                    Value = updateBaselineItemz.ShouldBeIncluded,
                    SqlDbType = System.Data.SqlDbType.Bit,
                },
                new SqlParameter
                {
                    ParameterName = "SingleNodeInclusion",
                    Value = updateBaselineItemz.SingleNodeInclusion,
                    SqlDbType = System.Data.SqlDbType.Bit,
                },

		        // TODO :: For the following SqlParameter for type VARCHAR(MAX) supports ASCII and not unicode.
		        // To support unicode we have to change it to NVARCHAR(MAX) or NVARCHAR(n) with specified length. 

                new SqlParameter
                {
                    ParameterName = "BaselineItemzIds",
                    Value = csvBaselineItemzIds.ToString(),
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Size = -1, // size value of -1 indicates varchar(max)
                }
            };

            sqlParameters = sqlParameters.Append(OUTPUT_isSuccessful).ToArray();

            var tempResultOfExecution = await _baselineContext.Database.ExecuteSqlRawAsync(sql: "EXEC userProcUpdateBaselineItemz @BaselineId, @ShouldBeIncluded, @SingleNodeInclusion, @BaselineItemzIds, @OUTPUT_Success = @OUTPUT_Success OUT", parameters: sqlParameters);

            return ((bool)OUTPUT_isSuccessful.Value);
        }




		/// <summary>
		/// Handles EXCLUSION scenario ONLY
		/// Single responsibility: Deduct a record's roll-up estimation from all ancestors
		/// 
		/// Prerequisites:
		/// - The target record has already been marked as isIncluded = false by the stored procedure
		/// - The target record MUST be of type 'BaselineItemz'
		/// 
		/// Process:
		/// 1. Validate that target record is of type 'BaselineItemz'
		/// 2. Capture the target record's current RolledUpEstimation value
		/// 3. Set target record's RolledUpEstimation = ZERO
		/// 4. Traverse hierarchy upward to find the parent Baseline record
		/// 5. Deduct the captured value from every ancestor up to (and including) that Baseline record
		/// 
		/// This method performs NO recalculations - it only propagates the deduction upward
		/// </summary>
		public async Task<bool> DeductRollUpFromAncestryChainAsync(
			Guid baselineItemzHierarchyRecordId)
		{
			try
			{
				if (baselineItemzHierarchyRecordId == Guid.Empty)
				{
					return false;
				}

				// Step 1: Get the target record
				var targetRecord = await _baselineContext.BaselineItemzHierarchy!
					.FirstOrDefaultAsync(bih => bih.Id == baselineItemzHierarchyRecordId);

				if (targetRecord == null || targetRecord.BaselineItemzHierarchyId == null)
				{
					return false;
				}

				// Step 2: Validate that the target record is of type 'BaselineItemz'
				if (targetRecord.RecordType != "BaselineItemz")
				{
					throw new ApplicationException(
						$"DeductRollUpFromAncestryChainAsync can only be called for 'BaselineItemz' record type. " +
						$"Provided record is of type '{targetRecord.RecordType}'");
				}

				// Step 3: Capture the RolledUpEstimation value BEFORE setting it to zero
				decimal deductionAmount = targetRecord.RolledUpEstimation;

				// Step 4: Set target record's RolledUpEstimation to ZERO
				targetRecord.RolledUpEstimation = 0;
				await _baselineContext.SaveChangesAsync();

				// Step 5: If deduction amount is zero, no need to propagate
				if (deductionAmount == 0)
				{
					return true;
				}

				// Step 6: Find the parent Baseline record by traversing the hierarchy upward
				HierarchyId? currentHierarchyId = targetRecord.BaselineItemzHierarchyId;
				BaselineItemzHierarchy? baselineRecord = null;

				// Traverse up the hierarchy to find the Baseline record
				while (currentHierarchyId != null && currentHierarchyId.GetLevel() > 0)
				{
					// Move to parent level
					currentHierarchyId = currentHierarchyId.GetAncestor(1);

					// Check if a record exists at this level with RecordType = 'Baseline'
					baselineRecord = await _baselineContext.BaselineItemzHierarchy!
						.FirstOrDefaultAsync(bih =>
							bih.BaselineItemzHierarchyId == currentHierarchyId
							&& bih.RecordType == "Baseline");

					if (baselineRecord != null)
					{
						break;
					}
				}

				// Step 7: If Baseline record not found, something is wrong with the hierarchy
				if (baselineRecord == null)
				{
					throw new ApplicationException(
						$"No parent Baseline record found in hierarchy for BaselineItemz {baselineItemzHierarchyRecordId}. " +
						"The hierarchy structure may be corrupted.");
				}

				// Step 8: Get all ancestor records from target's parent up to (and including) the Baseline
				var ancestorsToUpdate = await _baselineContext.BaselineItemzHierarchy!
					.Where(bih =>
						// Must be an ancestor of the target record
						targetRecord.BaselineItemzHierarchyId!.IsDescendantOf(bih.BaselineItemzHierarchyId!) == true
						// Must be a descendant of (or equal to) the baseline record
						&& bih.BaselineItemzHierarchyId!.IsDescendantOf(baselineRecord.BaselineItemzHierarchyId!) == true
						// Exclude the target record itself
						&& bih.BaselineItemzHierarchyId != targetRecord.BaselineItemzHierarchyId)
					.OrderByDescending(bih => bih.BaselineItemzHierarchyId!.GetLevel())
					.ToListAsync();

				// Step 9: Deduct the captured amount from each ancestor
				foreach (var ancestor in ancestorsToUpdate)
				{
					ancestor.RolledUpEstimation -= deductionAmount;
				}

				// Step 10: Save all changes
				await _baselineContext.SaveChangesAsync();

				return true;
			}
			catch (ApplicationException ex)
			{
				throw; // Re-throw application exceptions as-is
			}
			catch (Exception ex)
			{
				throw new ApplicationException(
					$"Error deducting roll-up estimation from ancestry chain: {ex.Message}",
					ex);
			}
		}


		/// <summary>
		/// Handles INCLUSION scenario ONLY
		/// 
		/// Single responsibility: Add a record's roll-up estimation to all ancestors
		/// 
		/// Prerequisites:
		/// - The target record has already been marked as isIncluded = true by the stored procedure
		/// - The target record MUST be of type 'BaselineItemz'
		/// 
		/// Process:
		/// 1. Validate that target record is of type 'BaselineItemz'
		/// 2. Recalculate target record's RolledUpEstimation = OwnEstimation + SUM(INCLUDED immediate children's RolledUpEstimation)
		/// 3. Traverse hierarchy upward to find the parent Baseline record
		/// 4. Add the calculated RolledUpEstimation to every ancestor up to (and including) that Baseline record
		/// 
		/// Note: EXCLUDED records are treated as having effective RolledUpEstimation = 0, regardless of their stored value
		/// This method recalculates only the target record, not its descendants
		/// </summary>
		public async Task<bool> AddRollUpToAncestryChainAsync(
			Guid baselineItemzHierarchyRecordId)
		{
			try
			{
				if (baselineItemzHierarchyRecordId == Guid.Empty)
				{
					return false;
				}

				// Step 1: Get the target record
				var targetRecord = await _baselineContext.BaselineItemzHierarchy!
					.FirstOrDefaultAsync(bih => bih.Id == baselineItemzHierarchyRecordId);

				if (targetRecord == null || targetRecord.BaselineItemzHierarchyId == null)
				{
					return false;
				}

				// Step 2: Validate that the target record is of type 'BaselineItemz'
				if (targetRecord.RecordType != "BaselineItemz")
				{
					throw new ApplicationException(
						$"AddRollUpToAncestryChainAsync can only be called for 'BaselineItemz' record type. " +
						$"Provided record is of type '{targetRecord.RecordType}'");
				}

				// Step 3: Treat EXCLUDED records as having effective RolledUpEstimation = 0
				// This handles cases where a record was EXCLUDED but retained a stale RolledUpEstimation value
				decimal oldRolledUpEstimation = targetRecord.isIncluded == false ? 0 : targetRecord.RolledUpEstimation;

				// Step 4: Recalculate RolledUpEstimation = OwnEstimation + SUM(INCLUDED immediate children's RolledUpEstimation)
				var childrenSum = await _baselineContext.BaselineItemzHierarchy!
					.Where(bih =>
						bih.BaselineItemzHierarchyId!.GetAncestor(1) == targetRecord.BaselineItemzHierarchyId
						&& bih.isIncluded == true)
					.SumAsync(bih => bih.RolledUpEstimation);

				decimal newRolledUpEstimation = targetRecord.OwnEstimation + childrenSum;

				// Step 5: Update target record with new RolledUpEstimation
				targetRecord.RolledUpEstimation = newRolledUpEstimation;
				await _baselineContext.SaveChangesAsync();

				// Step 6: Calculate the delta (change) from old to new value
				// Since we treated old excluded records as 0, this ensures correct propagation
				decimal additionAmount = newRolledUpEstimation - oldRolledUpEstimation;

				// Step 7: If addition amount is zero, no need to propagate
				if (additionAmount == 0)
				{
					return true;
				}

				// Step 8: Find the parent Baseline record by traversing the hierarchy upward
				HierarchyId? currentHierarchyId = targetRecord.BaselineItemzHierarchyId;
				BaselineItemzHierarchy? baselineRecord = null;

				// Traverse up the hierarchy to find the Baseline record
				while (currentHierarchyId != null && currentHierarchyId.GetLevel() > 0)
				{
					// Move to parent level
					currentHierarchyId = currentHierarchyId.GetAncestor(1);

					// Check if a record exists at this level with RecordType = 'Baseline'
					baselineRecord = await _baselineContext.BaselineItemzHierarchy!
						.FirstOrDefaultAsync(bih =>
							bih.BaselineItemzHierarchyId == currentHierarchyId
							&& bih.RecordType == "Baseline");

					if (baselineRecord != null)
					{
						break;
					}
				}

				// Step 9: If Baseline record not found, something is wrong with the hierarchy
				if (baselineRecord == null)
				{
					throw new ApplicationException(
						$"No parent Baseline record found in hierarchy for BaselineItemz {baselineItemzHierarchyRecordId}. " +
						"The hierarchy structure may be corrupted.");
				}

				// Step 10: Get all ancestor records from target's parent up to (and including) the Baseline
				var ancestorsToUpdate = await _baselineContext.BaselineItemzHierarchy!
					.Where(bih =>
						// Must be an ancestor of the target record
						targetRecord.BaselineItemzHierarchyId!.IsDescendantOf(bih.BaselineItemzHierarchyId!) == true
						// Must be a descendant of (or equal to) the baseline record
						&& bih.BaselineItemzHierarchyId!.IsDescendantOf(baselineRecord.BaselineItemzHierarchyId!) == true
						// Exclude the target record itself
						&& bih.BaselineItemzHierarchyId != targetRecord.BaselineItemzHierarchyId)
					.OrderByDescending(bih => bih.BaselineItemzHierarchyId!.GetLevel())
					.ToListAsync();

				// Step 11: Add the delta amount to each ancestor
				foreach (var ancestor in ancestorsToUpdate)
				{
					ancestor.RolledUpEstimation += additionAmount;
				}

				// Step 12: Save all changes
				await _baselineContext.SaveChangesAsync();

				return true;
			}
			catch (ApplicationException ex)
			{
				throw; // Re-throw application exceptions as-is
			}
			catch (Exception ex)
			{
				throw new ApplicationException(
					$"Error adding roll-up estimation to ancestry chain: {ex.Message}",
					ex);
			}
		}


		/// <summary>
		/// Handles INCLUSION ALL CHILDREN scenario
		/// 
		/// Single responsibility: Call stored procedure that handles entire scenario
		/// 
		/// Prerequisites:
		/// - The target record has already been marked as isIncluded = true by the stored procedure
		/// - ALL descendants have already been marked as isIncluded = true by the stored procedure
		/// - The target record MUST be of type 'BaselineItemz'
		/// 
		/// Process:
		/// 1. Validate that target record is of type 'BaselineItemz'
		/// 2. Call stored procedure which:
		///    - Recalculates target and all descendants (bottom-up from leaf nodes)
		///    - Updates all descendants with new roll-up values
		///    - Calculates and updates target record's roll-up
		///    - Propagates target's final value to all ancestors up to Baseline
		/// 3. Returns success/failure status
		/// 
		/// The entire operation is atomic - either all succeeds or all rolls back
		/// </summary>
		public async Task<bool> AddRollUpToAncestryChainForIncludeAllChildBaselineItemzAsync(
			Guid baselineItemzHierarchyRecordId)
		{
			try
			{
				if (baselineItemzHierarchyRecordId == Guid.Empty)
				{
					return false;
				}

				// Step 1: Get the target record for validation
				var targetRecord = await _baselineContext.BaselineItemzHierarchy!
					.FirstOrDefaultAsync(bih => bih.Id == baselineItemzHierarchyRecordId);

				if (targetRecord == null)
				{
					return false;
				}

				// Step 2: Validate that the target record is of type 'BaselineItemz'
				if (targetRecord.RecordType != "BaselineItemz")
				{
					throw new ApplicationException(
						$"AddRollUpToAncestryChainForScenarioThreeAsync can only be called for 'BaselineItemz' record type. " +
						$"Provided record is of type '{targetRecord.RecordType}'");
				}

				// Step 3: Create parameter for stored procedure
				var sqlParameters = new[]
				{
					new SqlParameter
					{
						ParameterName = "BaselineItemzHierarchyRecordId",
						Value = baselineItemzHierarchyRecordId,
						SqlDbType = System.Data.SqlDbType.UniqueIdentifier
					}
				};

				// Step 4: Execute stored procedure
				// EXPLANATION: The stored procedure handles:
				// - Bottom-up calculation of all descendants
				// - Update of all descendants in database
				// - Calculation and update of target record
				// - Propagation of target's value to all ancestors up to Baseline
				// - All within a single atomic transaction
				try
				{
					await _baselineContext.Database.ExecuteSqlRawAsync(
						"EXEC userProcRecalculateBaselineItemzDescendantsRollUpEstimations @BaselineItemzHierarchyRecordId",
						sqlParameters);

					return true;
				}
				catch (Exception ex)
				{
					throw new ApplicationException(
						$"Failed to execute Scenario 3 procedure for BaselineItemz {baselineItemzHierarchyRecordId}: {ex.Message}",
						ex);
				}
			}
			catch (ApplicationException ex)
			{
				throw; // Re-throw application exceptions as-is
			}
			catch (Exception ex)
			{
				throw new ApplicationException(
					$"Error in Scenario 3 processing: {ex.Message}",
					ex);
			}
		}


		public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose resources when needed
            }
        }
    }
}
