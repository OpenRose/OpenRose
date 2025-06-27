// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using ItemzApp.API.DbContexts;
using ItemzApp.API.Entities;
using ItemzApp.API.Helper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using ItemzApp.API.Models;
using ItemzApp.API.Models.BetweenControllerAndRepository;

namespace ItemzApp.API.Services
{
    public class BaselineHierarchyRepository : IBaselineHierarchyRepository, IDisposable
    {
        private readonly ItemzContext _context;

        public BaselineHierarchyRepository(ItemzContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
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

        public async Task<BaselineHierarchyIdRecordDetailsDTO?> GetBaselineHierarchyRecordDetailsByID(Guid recordId)
        {
            if (recordId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(recordId));
            }

            var foundBaselineHierarchyRecord = _context.BaselineItemzHierarchy!.AsNoTracking()
                            .Where(bih => bih.Id == recordId);

            if (foundBaselineHierarchyRecord.Count() != 1)
            {
                throw new ApplicationException($"Expected 1 Baseline Hierarchy record to be found " +
                    $"but instead found {foundBaselineHierarchyRecord.Count()} records for ID {recordId} " +
                    "Please contact your System Administrator.");
            }
            
            var baselineHierarchyIdRecordDetails = new BaselineHierarchyIdRecordDetailsDTO();
            baselineHierarchyIdRecordDetails.RecordId = recordId;
            baselineHierarchyIdRecordDetails.BaselineHierarchyId = foundBaselineHierarchyRecord.FirstOrDefault()!.BaselineItemzHierarchyId!.ToString();
            baselineHierarchyIdRecordDetails.SourceHierarchyId = (foundBaselineHierarchyRecord.FirstOrDefault()!.SourceItemzHierarchyId != null) 
                                                                    ? foundBaselineHierarchyRecord.FirstOrDefault()!.SourceItemzHierarchyId!.ToString() 
                                                                    : "";
            baselineHierarchyIdRecordDetails.RecordType = foundBaselineHierarchyRecord.FirstOrDefault()!.RecordType;
            baselineHierarchyIdRecordDetails.Name = foundBaselineHierarchyRecord.FirstOrDefault()!.Name ?? "";
            baselineHierarchyIdRecordDetails.Level = foundBaselineHierarchyRecord.FirstOrDefault()!.BaselineItemzHierarchyId!.GetLevel();
            baselineHierarchyIdRecordDetails.IsIncluded = foundBaselineHierarchyRecord.FirstOrDefault()!.isIncluded;

            // EXPLANATION : We are using SQL Server HierarchyID field type. Now we can use EF Core special
            // methods to query for all Decendents as per below. We are actually finding all Decendents by saying
            // First find the BaselineItemzHierarchy record where ID matches RootItemz ID. This is expected to be the
            // repository ID itself which is the root. then we find all desendents of Repository which is nothing but Project(s). 

            var parentBaselineItemzHierarchyRecord = await _context.BaselineItemzHierarchy!
                    .AsNoTracking()
                    .Where(bih => bih.BaselineItemzHierarchyId!.GetAncestor(1) == foundBaselineHierarchyRecord.FirstOrDefault()!.BaselineItemzHierarchyId!)
                    .OrderBy(bih => bih.BaselineItemzHierarchyId!)
                    .ToListAsync();

            if (parentBaselineItemzHierarchyRecord.Count != 0)
            {
                baselineHierarchyIdRecordDetails.TopChildBaselineHierarchyId = parentBaselineItemzHierarchyRecord.FirstOrDefault()!.BaselineItemzHierarchyId!.ToString();
                baselineHierarchyIdRecordDetails.BottomChildBaselineHierarchyId = parentBaselineItemzHierarchyRecord.LastOrDefault()!.BaselineItemzHierarchyId!.ToString();
                baselineHierarchyIdRecordDetails.NumberOfChildNodes = parentBaselineItemzHierarchyRecord.Count;

            }

			// EXPLANATION : Here we are getting record where Baseline Hierarchy ID is equal to the Baseline Hierarchy Id of immediate parent. 
			// We find immediate parent by using GetAncestor(1) method on found Baseline hierarchy record.

			var parentBaselineHierarchyRecord = _context.BaselineItemzHierarchy!
				.AsNoTracking()
				.Where(bih => bih.BaselineItemzHierarchyId == foundBaselineHierarchyRecord.FirstOrDefault()!.BaselineItemzHierarchyId!.GetAncestor(1))
				.FirstOrDefault();

			if (parentBaselineHierarchyRecord != null)
			{
				baselineHierarchyIdRecordDetails.ParentRecordId = parentBaselineHierarchyRecord.Id;
				baselineHierarchyIdRecordDetails.ParentRecordType = parentBaselineHierarchyRecord.RecordType;
				baselineHierarchyIdRecordDetails.ParentBaselineHierarchyId = parentBaselineHierarchyRecord.BaselineItemzHierarchyId!.ToString();
				baselineHierarchyIdRecordDetails.ParentLevel = parentBaselineHierarchyRecord.BaselineItemzHierarchyId!.GetLevel();
                baselineHierarchyIdRecordDetails.ParentName = parentBaselineHierarchyRecord.Name ?? "";
                baselineHierarchyIdRecordDetails.ParentIsIncluded = parentBaselineHierarchyRecord.isIncluded;
			}
			return baselineHierarchyIdRecordDetails;
        }


		public async Task<IEnumerable<BaselineHierarchyIdRecordDetailsDTO?>> GetImmediateChildrenOfBaselineItemzHierarchy(Guid recordId)
		{
			if (recordId == Guid.Empty)
			{
				throw new ArgumentNullException(nameof(recordId));
			}

			var foundBaselineHierarchyRecord = _context.BaselineItemzHierarchy!.AsNoTracking()
							.Where(ih => ih.Id == recordId);


			if (foundBaselineHierarchyRecord.Count() != 1) 
			{
				//if (!foundBaselineHierarchyRecord.Any())
				//{
				//	return default;
				//}
				return default;
				// EXPLANATION:: WE COMMENTED OUT FOLLOWING EXPECTION THROWING CODE 
				//				 BECAUSE IT'S POSSIBLE THAT PROJECT MIGHT NOT HAVE ANY BASELINE IN IT. 

				//throw new ApplicationException($"Expected 1 Baseline Hierarchy record to be found " +
				//	$"but instead found {foundBaselineHierarchyRecord.Count()} records for ID {recordId} " +
				//	"Please contact your System Administrator.");
			}

			var foundBaselineHierarchyRecordLevel = foundBaselineHierarchyRecord.FirstOrDefault()!.BaselineItemzHierarchyId.GetLevel();


			// EXPLANATION : We are using SQL Server HierarchyID field type. Now we can use EF Core special
			// methods to query for all Decendents as per below. By adding clause to check for GetLevel which is less then
			// CurrentBaselineHierarchyRecordLevel + three, we get Baseline Hierarchy record itself
			// plus two more deeper level of baseline hierarchy records.
			// The 1st Level data of the record itself is ignored and then 2nd level data is the actual child records.
			// While third level data are used for calculating number of children for child records.

			var baselineItemzTypeHierarchyItemzs = await _context.BaselineItemzHierarchy!
					.AsNoTracking()
					.Where(bih => bih.BaselineItemzHierarchyId!.IsDescendantOf(foundBaselineHierarchyRecord.FirstOrDefault()!.BaselineItemzHierarchyId!)
						&& bih.BaselineItemzHierarchyId.GetLevel() < (foundBaselineHierarchyRecordLevel + 3))
					.OrderBy(bih => bih.BaselineItemzHierarchyId!)
					.ToListAsync();

			if (baselineItemzTypeHierarchyItemzs.Count() < 2) // Less then 2 because we may have project entry but no baseline below it.
			{
				return default;
			}

			List<BaselineHierarchyIdRecordDetailsDTO> returningRecords = [];
			BaselineHierarchyIdRecordDetailsDTO baselineHierarchyIdRecordDetails = new();
			string? _localTopChildBaselineHierarchyId = null;
			int _localNumerOfChildNodes = 0;


			for (var i = 0; i < baselineItemzTypeHierarchyItemzs.Count(); i++)
			{
				if (baselineItemzTypeHierarchyItemzs[i].BaselineItemzHierarchyId!.GetLevel() == (foundBaselineHierarchyRecordLevel + 1))
				{
					if (i == 1) // Because i = 0 is the baseline hierarchy record for passed in recordId parameter itself. So we check for i == 1 as first child record.
					{
						baselineHierarchyIdRecordDetails.RecordId = baselineItemzTypeHierarchyItemzs[i].Id;
						baselineHierarchyIdRecordDetails.Name = baselineItemzTypeHierarchyItemzs[i].Name ?? "";
						baselineHierarchyIdRecordDetails.BaselineHierarchyId = baselineItemzTypeHierarchyItemzs[i].BaselineItemzHierarchyId!.ToString();
						baselineHierarchyIdRecordDetails.RecordType = baselineItemzTypeHierarchyItemzs[i].RecordType;
						baselineHierarchyIdRecordDetails.Level = baselineItemzTypeHierarchyItemzs[i].BaselineItemzHierarchyId!.GetLevel();
						baselineHierarchyIdRecordDetails.SourceHierarchyId = (baselineItemzTypeHierarchyItemzs[i].SourceItemzHierarchyId != null)
																							? baselineItemzTypeHierarchyItemzs[i].SourceItemzHierarchyId!.ToString()
																							: "";
						baselineHierarchyIdRecordDetails.IsIncluded = baselineItemzTypeHierarchyItemzs[i].isIncluded;

						// EXPLANATION :: Now add Parent Details which is nothing but foundHierarchyRecord
						baselineHierarchyIdRecordDetails.ParentRecordId = foundBaselineHierarchyRecord.FirstOrDefault()!.Id;
						baselineHierarchyIdRecordDetails.ParentRecordType = foundBaselineHierarchyRecord.FirstOrDefault()!.RecordType;
						baselineHierarchyIdRecordDetails.ParentBaselineHierarchyId = foundBaselineHierarchyRecord.FirstOrDefault()!.BaselineItemzHierarchyId!.ToString();
						baselineHierarchyIdRecordDetails.ParentLevel = foundBaselineHierarchyRecord.FirstOrDefault()!.BaselineItemzHierarchyId!.GetLevel();
						baselineHierarchyIdRecordDetails.ParentName = foundBaselineHierarchyRecord.FirstOrDefault()!.Name ?? "";
                        baselineHierarchyIdRecordDetails.ParentIsIncluded = foundBaselineHierarchyRecord.FirstOrDefault()!.isIncluded;
					}
					else
					{
						// WE HAVE TO FINISH WORKING ON PREVIOUS RECORD AND START PROCESSING NEXT ONE
						// IF NUMBER OF CHILD RECORDS ARE GREATER THEN ZERO i.e. ANY CHILD RECORDS FOUND THEN 
						// We have to capture BottomChildHierarchyId and NumberOfChildNodes values.
						if (_localNumerOfChildNodes > 0)
						{
							// hierarchyIdRecordDetails.TopChildHierarchyId = _localTopChildHierarchyId;
							baselineHierarchyIdRecordDetails.BottomChildBaselineHierarchyId = baselineItemzTypeHierarchyItemzs[i - 1].BaselineItemzHierarchyId!.ToString();
							baselineHierarchyIdRecordDetails.NumberOfChildNodes = _localNumerOfChildNodes;
							_localNumerOfChildNodes = 0; // RESET 
						}

						returningRecords.Add(baselineHierarchyIdRecordDetails);

						// RESET hierarchyIdRecordDetails AND START CAPTURING DETAILS OF THE NEXT CHILD RECORD

						baselineHierarchyIdRecordDetails = new();
						baselineHierarchyIdRecordDetails.RecordId = baselineItemzTypeHierarchyItemzs[i].Id;
						baselineHierarchyIdRecordDetails.Name = baselineItemzTypeHierarchyItemzs[i].Name ?? "";
						baselineHierarchyIdRecordDetails.BaselineHierarchyId = baselineItemzTypeHierarchyItemzs[i].BaselineItemzHierarchyId!.ToString();
						baselineHierarchyIdRecordDetails.RecordType = baselineItemzTypeHierarchyItemzs[i].RecordType;
						baselineHierarchyIdRecordDetails.Level = baselineItemzTypeHierarchyItemzs[i].BaselineItemzHierarchyId!.GetLevel();
						baselineHierarchyIdRecordDetails.SourceHierarchyId = (baselineItemzTypeHierarchyItemzs[i].SourceItemzHierarchyId != null)
																							? baselineItemzTypeHierarchyItemzs[i].SourceItemzHierarchyId!.ToString()
																							: "";


						baselineHierarchyIdRecordDetails.IsIncluded = baselineItemzTypeHierarchyItemzs[i].isIncluded;


						// EXPLANATION :: Now add Parent Details which is nothing but foundHierarchyRecord
						baselineHierarchyIdRecordDetails.ParentRecordId = foundBaselineHierarchyRecord.FirstOrDefault()!.Id;
						baselineHierarchyIdRecordDetails.ParentRecordType = foundBaselineHierarchyRecord.FirstOrDefault()!.RecordType;
						baselineHierarchyIdRecordDetails.ParentBaselineHierarchyId = foundBaselineHierarchyRecord.FirstOrDefault()!.BaselineItemzHierarchyId!.ToString();
						baselineHierarchyIdRecordDetails.ParentLevel = foundBaselineHierarchyRecord.FirstOrDefault()!.BaselineItemzHierarchyId!.GetLevel();
						baselineHierarchyIdRecordDetails.ParentName = foundBaselineHierarchyRecord.FirstOrDefault()!.Name ?? "";
                        baselineHierarchyIdRecordDetails.ParentIsIncluded = foundBaselineHierarchyRecord.FirstOrDefault()!.isIncluded;
					}

				}
				else if (baselineItemzTypeHierarchyItemzs[i].BaselineItemzHierarchyId.GetLevel() == (foundBaselineHierarchyRecordLevel + 2))
				{
					if (_localNumerOfChildNodes == 0)
					{
						baselineHierarchyIdRecordDetails.TopChildBaselineHierarchyId = baselineItemzTypeHierarchyItemzs[i].BaselineItemzHierarchyId!.ToString();
					}
					_localNumerOfChildNodes = _localNumerOfChildNodes + 1;
				}
			}

			// ADD FINAL hierarchyIdRecordDetails TO THE COLLECTION.
			if (_localNumerOfChildNodes > 0)
			{
				baselineHierarchyIdRecordDetails.BottomChildBaselineHierarchyId = baselineItemzTypeHierarchyItemzs[(baselineItemzTypeHierarchyItemzs.Count() - 1)].BaselineItemzHierarchyId!.ToString();
				baselineHierarchyIdRecordDetails.NumberOfChildNodes = _localNumerOfChildNodes;
			}
			returningRecords.Add(baselineHierarchyIdRecordDetails);

			return returningRecords;
		}


		public async Task<bool> CheckIfPartOfSingleBaselineHierarchyBreakdownStructureAsync(Guid parentId, Guid childId) 
        {
            var foundParentId = await _context.BaselineItemzHierarchy!.AsNoTracking()
                .Where(bih => bih.Id == parentId)
                .Where(bih => bih.BaselineItemzHierarchyId!.GetLevel() > 1 ) 
                .FirstOrDefaultAsync();

            if (foundParentId != null)
            {
                var foundChildId =  await _context.BaselineItemzHierarchy!.AsNoTracking()
                    .Where(bih => bih.Id == childId)
                    .Where(bih => bih.BaselineItemzHierarchyId!.IsDescendantOf( foundParentId!.BaselineItemzHierarchyId))
                    .ToListAsync();
                
                if (foundChildId.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }


		public async Task<RecordCountAndEnumerable<NestedBaselineHierarchyIdRecordDetailsDTO>> GetAllParentsOfBaselineItemzHierarchy(Guid recordId)
		{
			if (recordId == Guid.Empty)
			{
				throw new ArgumentNullException(nameof(recordId));
			}

			var foundBaselineHierarchyRecord = _context.BaselineItemzHierarchy!.AsNoTracking()
							.Where(bih => bih.Id == recordId);

			if (foundBaselineHierarchyRecord.Count() != 1)
			{
				throw new ApplicationException($"Expected 1 Baseline Itemz Hierarchy record to be found " +
					$"but instead found {foundBaselineHierarchyRecord.Count()} records for ID {recordId}" +
					"Please contact your System Administrator.");
			}

			var foundBaselineHierarchyRecordLevel = foundBaselineHierarchyRecord.FirstOrDefault()!.BaselineItemzHierarchyId!.GetLevel();
			int rootRepositoryLevel = 0;
			// EXPLANATION : We are using SQL Server HierarchyID field type. Now we can use EF Core special
			// methods to query for all Decendents as per below. 

			RecordCountAndEnumerable<NestedBaselineHierarchyIdRecordDetailsDTO> recordCountAndEnumerable = new RecordCountAndEnumerable<NestedBaselineHierarchyIdRecordDetailsDTO>();

			var baselineAllHierarchyItemzs = await _context.BaselineItemzHierarchy!
					.AsNoTracking()
					.Where(bih => foundBaselineHierarchyRecord.FirstOrDefault()!.BaselineItemzHierarchyId!.IsDescendantOf(bih.BaselineItemzHierarchyId!))
					.OrderBy(bih => bih.BaselineItemzHierarchyId!)
					.ToListAsync();

			List<NestedBaselineHierarchyIdRecordDetailsDTO> returningRecords = [];

			if (baselineAllHierarchyItemzs.Count() > 2) // We check more then 2 because 1st record is repository and last record is for recordId itself.
			{
				recordCountAndEnumerable.RecordCount = (baselineAllHierarchyItemzs.Count() - 2); // We are skipping repository and recordId records and so we reduce by 2 here.
			}
			else
			{
				recordCountAndEnumerable.RecordCount = 0;
				recordCountAndEnumerable.AllRecords = new List<NestedBaselineHierarchyIdRecordDetailsDTO>();
			}

			for (var i = 0; i < baselineAllHierarchyItemzs.Count(); i++)
			{
				if (i == rootRepositoryLevel) continue; // Skip first record as it's for the repository record
				if (i == (baselineAllHierarchyItemzs.Count() -1 )) continue; // Skip last record as it's for the supplied recordId
				if (baselineAllHierarchyItemzs[i].BaselineItemzHierarchyId!.GetLevel() == (rootRepositoryLevel + 1))
				{
					returningRecords.Add(new NestedBaselineHierarchyIdRecordDetailsDTO
					{
						RecordId = baselineAllHierarchyItemzs[i].Id,
						BaselineHierarchyId = baselineAllHierarchyItemzs[i].BaselineItemzHierarchyId!.ToString(),
						Level = baselineAllHierarchyItemzs[i].BaselineItemzHierarchyId!.GetLevel(),
						RecordType = baselineAllHierarchyItemzs[i].RecordType,
						Name = baselineAllHierarchyItemzs[i].Name ?? "",
						isIncluded = baselineAllHierarchyItemzs[i].isIncluded,
						Children = new List<NestedBaselineHierarchyIdRecordDetailsDTO>()
					});
				}
				else if (baselineAllHierarchyItemzs[i].BaselineItemzHierarchyId!.GetLevel() > (rootRepositoryLevel + 1))
				{


					// Find the last record at a specified level directly within returningRecords
					//var targetLevel = (baselineAllHierarchyItemzs[i].BaselineItemzHierarchyId!.GetLevel() - 1);
					//var lastRecordAtLevel = FindLastRecordAtLevel(returningRecords, targetLevel);
					var lastRecordAtLevel = FindParentRecord(returningRecords, baselineAllHierarchyItemzs[i]);

					if (lastRecordAtLevel != null)
					{
						lastRecordAtLevel.Children.Add(new NestedBaselineHierarchyIdRecordDetailsDTO
						{
							RecordId = baselineAllHierarchyItemzs[i].Id,
							BaselineHierarchyId = baselineAllHierarchyItemzs[i].BaselineItemzHierarchyId!.ToString(),
							Level = baselineAllHierarchyItemzs[i].BaselineItemzHierarchyId!.GetLevel(),
							RecordType = baselineAllHierarchyItemzs[i].RecordType,
							Name = baselineAllHierarchyItemzs[i].Name ?? "",
							isIncluded = baselineAllHierarchyItemzs[i].isIncluded,
							Children = new List<NestedBaselineHierarchyIdRecordDetailsDTO>()
						});
					}
					else
					{
						throw new ApplicationException($"Parent record could not be found for  " +
											$"RecordID {baselineAllHierarchyItemzs[i].Id} with " +
											$"BaselineHierarchyID  {baselineAllHierarchyItemzs[i].BaselineItemzHierarchyId!.ToString()} and " +
											$"Level as {baselineAllHierarchyItemzs[i].BaselineItemzHierarchyId!.GetLevel().ToString()} " +
											"Please contact your System Administrator.");
					}
				}
			}
			recordCountAndEnumerable.AllRecords = returningRecords;
			return recordCountAndEnumerable;
		}


		// TODO :: Baseline Itemz Hierarchy Record should include additional information which is related to Baseline Itemz only. 
		// For example, "IsIncluded" is a property found in BaselineHierarchyRecord but not in HierarchyRecord. So we need to
		// Make sure that we pass back those information as well.
		public async Task<RecordCountAndEnumerable<NestedBaselineHierarchyIdRecordDetailsDTO>> GetAllChildrenOfBaselineItemzHierarchy(
	Guid recordId,
	bool exportIncludedBaselineItemzOnly = false
)
		{
			if (recordId == Guid.Empty)
			{
				throw new ArgumentNullException(nameof(recordId));
			}

			var foundBaselineHierarchyRecord = _context.BaselineItemzHierarchy!.AsNoTracking()
				.Where(bih => bih.Id == recordId);

			if (foundBaselineHierarchyRecord.Count() != 1)
			{
				throw new ApplicationException($"Expected 1 Baseline Itemz Hierarchy record to be found " +
					$"but instead found {foundBaselineHierarchyRecord.Count()} records for ID {recordId}" +
					"Please contact your System Administrator.");
			}

			var foundBaselineHierarchyRecordLevel = foundBaselineHierarchyRecord.FirstOrDefault()!.BaselineItemzHierarchyId!.GetLevel();

			var baselineAllHierarchyItemzs = await _context.BaselineItemzHierarchy!
				.AsNoTracking()
				.Where(bih => bih.BaselineItemzHierarchyId!.IsDescendantOf(foundBaselineHierarchyRecord.FirstOrDefault()!.BaselineItemzHierarchyId!))
				.OrderBy(bih => bih.BaselineItemzHierarchyId!)
				.ToListAsync();

			List<NestedBaselineHierarchyIdRecordDetailsDTO> returningRecords = [];

			for (var i = 0; i < baselineAllHierarchyItemzs.Count(); i++)
			{
				if (i == 0) continue; // Skip first record as it's for the supplied recordId

				var current = baselineAllHierarchyItemzs[i];

				// Skip nodes that are excluded, if flag is set
				if (exportIncludedBaselineItemzOnly && current.isIncluded == false)
					continue;

				if (current.BaselineItemzHierarchyId!.GetLevel() == (foundBaselineHierarchyRecordLevel + 1))
				{
					returningRecords.Add(new NestedBaselineHierarchyIdRecordDetailsDTO
					{
						RecordId = current.Id,
						BaselineHierarchyId = current.BaselineItemzHierarchyId!.ToString(),
						Level = current.BaselineItemzHierarchyId!.GetLevel(),
						RecordType = current.RecordType,
						Name = current.Name ?? "",
						isIncluded = current.isIncluded,
						Children = new List<NestedBaselineHierarchyIdRecordDetailsDTO>()
					});
				}
				else if (current.BaselineItemzHierarchyId!.GetLevel() > (foundBaselineHierarchyRecordLevel + 1))
				{
					var foundParentRecordInReturningList = FindParentRecord(returningRecords, current);

					if (foundParentRecordInReturningList != null)
					{
						// If parent is not included (when flag is set), skip adding children to it
						if (exportIncludedBaselineItemzOnly && !foundParentRecordInReturningList.isIncluded)
							continue;

						foundParentRecordInReturningList.Children.Add(new NestedBaselineHierarchyIdRecordDetailsDTO
						{
							RecordId = current.Id,
							BaselineHierarchyId = current.BaselineItemzHierarchyId!.ToString(),
							Level = current.BaselineItemzHierarchyId!.GetLevel(),
							RecordType = current.RecordType,
							Name = current.Name ?? "",
							isIncluded = current.isIncluded,
							Children = new List<NestedBaselineHierarchyIdRecordDetailsDTO>()
						});
					}
					else
					{
						throw new ApplicationException($"Parent record could not be found for  " +
							$"RecordID {current.Id} with " +
							$"BaselineHierarchyID  {current.BaselineItemzHierarchyId!.ToString()} and " +
							$"Level as {current.BaselineItemzHierarchyId!.GetLevel().ToString()} " +
							"Please contact your System Administrator.");
					}
				}
			}

			// Optional: Recursively prune children of excluded parents
			if (exportIncludedBaselineItemzOnly)
			{
				void PruneExcludedChildren(List<NestedBaselineHierarchyIdRecordDetailsDTO> nodes)
				{
					foreach (var node in nodes.ToList())
					{
						if (!node.isIncluded)
						{
							nodes.Remove(node);
						}
						else if (node.Children != null && node.Children.Any())
						{
							PruneExcludedChildren(node.Children);
						}
					}
				}
				PruneExcludedChildren(returningRecords);
			}

			// Always set results and count at the end, based on final filtered collection
			RecordCountAndEnumerable<NestedBaselineHierarchyIdRecordDetailsDTO> recordCountAndEnumerable = new RecordCountAndEnumerable<NestedBaselineHierarchyIdRecordDetailsDTO>
			{
				AllRecords = returningRecords,
				RecordCount = returningRecords.Count
			};
			return recordCountAndEnumerable;
		}


		public async Task<int> GetAllChildrenCountOfBaselineItemzHierarchy(Guid recordId)
        {
            if (recordId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(recordId));
            }

            var foundBaselineHierarchyRecord = _context.BaselineItemzHierarchy!.AsNoTracking()
                            .Where(bih => bih.Id == recordId);

            if (foundBaselineHierarchyRecord.Count() != 1)
            {
                throw new ApplicationException($"Expected 1 Baseline Hierarchy record to be found " +
                    $"but instead found {foundBaselineHierarchyRecord.Count()} records for ID {recordId}. " +
                    "Please contact your System Administrator."); // We dont expect multiple records for a single recordId anyways.
            }

            // EXPLANATION : We are using SQL Server HierarchyID field type. Now we can use EF Core special
            // methods to query for all Decendents as per below. 

            return (
                        await _context.BaselineItemzHierarchy!
                        .AsNoTracking()
                        .Where(bih => bih.BaselineItemzHierarchyId!.IsDescendantOf(foundBaselineHierarchyRecord.FirstOrDefault()!.BaselineItemzHierarchyId!))
                        .OrderBy(bih => bih.BaselineItemzHierarchyId!)
                        .CountAsync()
                    ) - 1; // reducing it by 1 because it will include record for supplied recordId as well.
        }

        public static NestedBaselineHierarchyIdRecordDetailsDTO? FindParentRecord(List<NestedBaselineHierarchyIdRecordDetailsDTO> records, BaselineItemzHierarchy childRecordToBeInserted)
		{
			NestedBaselineHierarchyIdRecordDetailsDTO? lastRecord = null;

			foreach (var record in records)
			{
				//if (record.Level == childRecordToBeInserted)
				//{
				//	if (lastRecord == null || string.Compare(record.BaselineHierarchyId, lastRecord.BaselineHierarchyId, StringComparison.Ordinal) > 0)
				//	{
				//		lastRecord = record;
				//	}
				//}

				if ((record.Level == (childRecordToBeInserted.BaselineItemzHierarchyId!.GetLevel() - 1))
					&& (childRecordToBeInserted.BaselineItemzHierarchyId.ToString().StartsWith(record.BaselineHierarchyId!.ToString())))
				{
					lastRecord = record;
					break;
				}

				//var childRecord = FindLastRecordAtLevel(record.Children, childRecordToBeInserted);
				//if (childRecord != null)
				//{
				//	if (lastRecord == null || string.Compare(childRecord.BaselineHierarchyId, lastRecord.BaselineHierarchyId, StringComparison.Ordinal) > 0)
				//	{
				//		lastRecord = childRecord;
				//	}
				//}

				var childRecord = FindParentRecord(record.Children, childRecordToBeInserted);
				if (childRecord != null)
				{
					lastRecord = childRecord;
					break;
				}
			}
			return lastRecord;
		}

  ////		public static NestedBaselineHierarchyIdRecordDetailsDTO? FindLastRecordAtLevelOLD(List<NestedBaselineHierarchyIdRecordDetailsDTO> records, int targetLevel)
  ////      {
  ////          NestedBaselineHierarchyIdRecordDetailsDTO? lastRecord = null;

  ////          foreach (var record in records)
  ////          {
  ////              if (record.Level == targetLevel)
  ////              {
  ////                  if (lastRecord == null || string.Compare(record.BaselineHierarchyId, lastRecord.BaselineHierarchyId, StringComparison.Ordinal) > 0)
  ////                  {
  ////                      lastRecord = record;
  ////                  }
  ////              }

  ////              var childRecord = FindLastRecordAtLevel(record.Children, targetLevel);
  ////              if (childRecord != null)
  ////              {
  ////                  if (lastRecord == null || string.Compare(childRecord.BaselineHierarchyId, lastRecord.BaselineHierarchyId, StringComparison.Ordinal) > 0)
  ////                  {
  ////                      lastRecord = childRecord;
  ////                  }
  ////              }
  ////          }

  ////          return lastRecord;
  ////      }


        public async Task<bool> UpdateBaselineHierarchyRecordNameByID(Guid recordId, string name)
		{
			if (recordId == Guid.Empty)
			{
				throw new ArgumentNullException(nameof(recordId));
			}

			var foundHierarchyRecord = _context.BaselineItemzHierarchy!
							.Where(bih => bih.Id == recordId);

			if (foundHierarchyRecord.Count() != 1)
			{
				throw new ApplicationException($"Expected 1 Baseline Hierarchy record to be found " +
					$"but instead found {foundHierarchyRecord.Count()} records for ID {recordId}" +
					"Please contact your System Administrator.");
			}

			if (foundHierarchyRecord.FirstOrDefault()!.RecordType.ToLower() == "repository") // TODO :: Use Constants instead of Text
			{
				throw new ApplicationException($"Can not update name of the Repository Root Baseline Hierarchy Record with ID {recordId}");
			}
			if (foundHierarchyRecord.FirstOrDefault()!.BaselineItemzHierarchyId!.GetLevel() == 0) // TODO :: Use Constants instead of Text
			{
				throw new ApplicationException($"Can not update name of the Repository Root Baseline Hierarchy Record with ID {recordId}");
			}

			foundHierarchyRecord.FirstOrDefault()!.Name = name; // TODO :: Remove special characters from name variable before saving it to DB. Security Reason.

			return (await _context.SaveChangesAsync() >= 0);
		}


	}
}
