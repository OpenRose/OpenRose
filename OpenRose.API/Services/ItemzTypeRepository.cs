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
using System.Text.RegularExpressions;
using System.Linq.Dynamic.Core;

namespace ItemzApp.API.Services
{
    public class ItemzTypeRepository : IItemzTypeRepository, IDisposable
    {
        private readonly ItemzContext _context;

        public ItemzTypeRepository(ItemzContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ItemzType> GetItemzTypeAsync(Guid ItemzTypeId)
        {
            if (ItemzTypeId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(ItemzTypeId));
            }

            return await _context.ItemzTypes!
                .Where(c => c.Id == ItemzTypeId).AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<ItemzType> GetItemzTypeForUpdateAsync(Guid ItemzTypeId)
        {
            if (ItemzTypeId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(ItemzTypeId));
            }

            return await _context.ItemzTypes!
                .Where(c => c.Id == ItemzTypeId).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ItemzType>?> GetItemzTypesAsync()
        {
            try
            {
                if (await _context.ItemzTypes.CountAsync<ItemzType>() > 0)
                {
                    var itemzTypeCollection = await _context.ItemzTypes.AsNoTracking().AsQueryable<ItemzType>().OrderBy(p => p.Name).ToListAsync();

                    // TODO: We have to create simple implementation of sort by ItemzType Name here 
                    // ItemzTypeCollection = ItemzTypeCollection.ApplySort("Name", null).AsNoTracking();

                    return itemzTypeCollection;
                }
                return null;
            }
            catch (Exception ex)
            {
                // TODO: It's not good that we capture Generic Exception and then 
                // return null here. Basically, I wanted to check if we have 
                // itemzTypes returned from the DB and if it does not then
                // it should simply return null back to the calling function.
                // One has to learn how to do this gracefully as part of Entity Framework 
                return null;
            }
        }

        public async Task<IEnumerable<ItemzType>> GetItemzTypesAsync(IEnumerable<Guid> itemzTypeIds)
        {
            if (itemzTypeIds == null)
            {
                throw new ArgumentNullException(nameof(itemzTypeIds));
            }

            return await _context.ItemzTypes.AsNoTracking().Where(a => itemzTypeIds.Contains(a.Id))
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public void AddItemzType(ItemzType itemzType)
        {
            if (itemzType ==null)
            {
                throw new ArgumentNullException(nameof(itemzType));
            }

            _context.ItemzTypes!.Add(itemzType);
        }

        public async Task AddNewItemzTypeHierarchyAsync(ItemzType itemzTypeEntity)
        {
            if (itemzTypeEntity == null)
            {
                throw new ArgumentNullException(nameof(itemzTypeEntity));
            }

            var rootProjectItemz = _context.ItemzHierarchy!.AsNoTracking()
                            .Where(ih => ih.Id == itemzTypeEntity.ProjectId);

            if (rootProjectItemz.Count() != 1)
            {
                throw new ApplicationException("Either no Root Project Repository Hierarchy record " +
                    "found OR multiple Root Project Repository Hierarchy records found in the system");
            }

            // EXPLANATION : We are using SQL Server HierarchyID field type. Now we can use EF Core special
            // methods to query for all Decendents as per below. We are actually finding all Decendents by saying
            // First find the ItemzHierarchy record where ID matches RootProject ID. This is expected to be the
            // Project ID itself which is the root OR parent to newly ItemzType.
            // Then we find all desendents of Repository which is nothing but existing ItemzType(s). 

            var itemzTypeHierarchyRecords = await _context.ItemzHierarchy!
                    .AsNoTracking()
                    .Where(ih => ih.ItemzHierarchyId!.GetAncestor(1) == rootProjectItemz.FirstOrDefault()!.ItemzHierarchyId!)
                    .OrderByDescending(ih => ih.ItemzHierarchyId!)
                    .ToListAsync();

            var tempItemzTypeHierarchy = new Entities.ItemzHierarchy
            {
                Id = itemzTypeEntity.Id,
                RecordType = "ItemzType",
                Name = itemzTypeEntity.Name,
                ItemzHierarchyId = rootProjectItemz.FirstOrDefault()!.ItemzHierarchyId!
                                    .GetDescendant(itemzTypeHierarchyRecords.Count() > 0
                                                        ? itemzTypeHierarchyRecords.FirstOrDefault()!.ItemzHierarchyId
                                                        : null
                                                   , null),
            };

            _context.ItemzHierarchy!.Add(tempItemzTypeHierarchy);
        }

        public async Task<bool> SaveAsync()
        {
            return (await _context.SaveChangesAsync() >= 0);
        }

        public async Task<bool> ItemzTypeExistsAsync(Guid itemzTypeId)
        {
            if (itemzTypeId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(itemzTypeId));
            }

            // EXPLANATION: Using ".Any()" instead of ".Find" as explained in method
            // public bool ItemzExists(Guid itemzId)
            // Above method is found in ItemzRepository.cs

            return await _context.ItemzTypes.AsNoTracking().AnyAsync(p => p.Id == itemzTypeId);

        }

        public void UpdateItemzType(ItemzType itemzType)
        {
            // Due to Repository Pattern implementation, 
            // there is no code in this implementation. 
        }

        public void DeleteItemzType(ItemzType itemzType)
        {
            _context.ItemzTypes!.Remove(itemzType);
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

        public async Task<bool> HasItemzTypeWithNameAsync(Guid projectId, string itemzTypeName)
        {
            return await _context.ItemzTypes.AsNoTracking().AnyAsync(it => it.ProjectId.ToString().ToLower() == projectId.ToString().ToLower() && it.Name!.ToLower() == itemzTypeName.ToLower());
        }

        public async Task<bool> DeleteItemzTypeItemzHierarchyAsync(Guid itemzTypeId)
        {
            bool returnValue;
            var OUTPUT_Success = new SqlParameter
            {
                ParameterName = "OUTPUT_Success",
                Direction = System.Data.ParameterDirection.Output,
                SqlDbType = System.Data.SqlDbType.Bit,
            };

            var sqlParameters = new[]
            {
                new SqlParameter
                {
                    ParameterName = "ItemzTypeId",
                    Value = itemzTypeId,
                    SqlDbType = System.Data.SqlDbType.UniqueIdentifier,
                }
            };

            sqlParameters = sqlParameters.Append(OUTPUT_Success).ToArray();

            var _ = await _context.Database.ExecuteSqlRawAsync(sql: "EXEC userProcDeleteItemzHierarchyRecordsByItemzTypeId  @ItemzTypeId, @OUTPUT_Success  = @OUTPUT_Success OUT", parameters: sqlParameters);
            returnValue = (bool)OUTPUT_Success.Value;
            return returnValue;
        }

        //public async Task<string?> GetTopItemzHierarchyID(Guid parentItemzTypeId)
        //{
        //    if (parentItemzTypeId == Guid.Empty)
        //    {
        //        throw new ArgumentNullException(nameof(parentItemzTypeId));
        //    }

        //    var rootItemzType = _context.ItemzHierarchy!.AsNoTracking()
        //                    .Where(ih => ih.Id == parentItemzTypeId);

        //    if (rootItemzType.Count() != 1)
        //    {
        //        throw new ApplicationException("Either no Root ItemzType Hierarchy record " +
        //            "found OR multiple Root ItemzType Hierarchy records found in the system. " +
        //            "Please contact your System Administrator.");
        //    }

        //    // EXPLANATION : We are using SQL Server HierarchyID field type. Now we can use EF Core special
        //    // methods to query for all Decendents as per below. We are actually finding all Decendents by saying
        //    // First find the ItemzHierarchy record where ID matches RootItemz ID. This is expected to be the
        //    // repository ID itself which is the root. then we find all desendents of Repository which is nothing but Project(s). 

        //    var itemzTypeHierarchyItemz = await _context.ItemzHierarchy!
        //            .AsNoTracking()
        //            .Where(ih => ih.ItemzHierarchyId!.GetAncestor(1) == rootItemzType.FirstOrDefault()!.ItemzHierarchyId!)
        //            .OrderBy(ih => ih.ItemzHierarchyId!)
        //            .ToListAsync();

        //    //return itemzTypeHierarchyItemz.Count > 0 ?
        //    //                HierarchyIdStringHelper.ManuallyGenerateHierarchyIdNumberString(
        //    //                itemzTypeHierarchyItemz.FirstOrDefault()!.ItemzHierarchyId!.ToString()
        //    //                , diffValue: -1
        //    //                , addDecimal: false)
        //    //             : null;

        //    return itemzTypeHierarchyItemz.Count > 0 ?
        //                    itemzTypeHierarchyItemz.FirstOrDefault()!.ItemzHierarchyId!.ToString()
        //                    : null;
        //}
        //public async Task<string?> GetLastItemzHierarchyID(Guid parentItemzTypeId)
        //{

        //    if (parentItemzTypeId == Guid.Empty)
        //    {
        //        throw new ArgumentNullException(nameof(parentItemzTypeId));
        //    }

        //    var rootItemzType = _context.ItemzHierarchy!.AsNoTracking()
        //                    .Where(ih => ih.Id == parentItemzTypeId);


        //    if (rootItemzType.Count() != 1)
        //    {
        //        throw new ApplicationException("Either no Root ItemzType Hierarchy record " +
        //            "found OR multiple Root ItemzType Hierarchy records found in the system. " +
        //            "Please contact your System Administrator.");
        //    }

        //    // EXPLANATION : We are using SQL Server HierarchyID field type. Now we can use EF Core special
        //    // methods to query for all Decendents as per below. We are actually finding all Decendents by saying
        //    // First find the ItemzHierarchy record where ID matches RootItemz ID. This is expected to be the
        //    // repository ID itself which is the root. then we find all desendents of Repository which is nothing but Project(s). 

        //    var itemzTypeHierarchyItemz = await _context.ItemzHierarchy!
        //            .AsNoTracking()
        //            .Where(ih => ih.ItemzHierarchyId!.GetAncestor(1) == rootItemzType.FirstOrDefault()!.ItemzHierarchyId!)
        //            .OrderByDescending(ih => ih.ItemzHierarchyId!)
        //            .ToListAsync();

        //    return itemzTypeHierarchyItemz.Count > 0 ? itemzTypeHierarchyItemz.FirstOrDefault()!.ItemzHierarchyId!.ToString() : null;
        //}


        public async Task MoveItemzTypeToAnotherProjectAsync(Guid movingItemzTypeId, Guid targetProjectId, bool atBottomOfChildNodes = true)
        {
            if (movingItemzTypeId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(movingItemzTypeId));
            }

            if (targetProjectId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(targetProjectId));
            }

            var movingItemzTypeHierarchyRecordList = _context.ItemzHierarchy!
                .Where(ih => ih.Id == movingItemzTypeId);
            var foundMovingItemzHierarchyRecordCount = movingItemzTypeHierarchyRecordList.Count();
            if (foundMovingItemzHierarchyRecordCount != 1)
            {
                throw new ApplicationException($"{movingItemzTypeHierarchyRecordList.Count()} records found for the " +
                    $"moving Itemz Id {movingItemzTypeId} in the system. " +
                    $"Expected 1 record but instead found {movingItemzTypeHierarchyRecordList.Count()}");
            }

            var movingItemzHierarchyRecord = new ItemzHierarchy();
            string originalItemzHierarchyIdString = "";
            List<ItemzHierarchy> allDescendentItemzHierarchyRecord = new List<ItemzHierarchy>();
            var found_ItemzType = await _context.ItemzTypes!.Where(it => it.Id == movingItemzTypeId).ToListAsync();


            if (found_ItemzType.FirstOrDefault()!.IsSystem == true)
            {
                if (found_ItemzType.FirstOrDefault()!.ProjectId != targetProjectId)
                {
                    throw new ApplicationException($"System does not support moving 'Parking Lot' system ItemzType " +
                        $"from source Project with ID {found_ItemzType.FirstOrDefault()!.ProjectId} to target Project with Id {targetProjectId}");
                }
            }

            movingItemzHierarchyRecord = movingItemzTypeHierarchyRecordList.FirstOrDefault();
            if (movingItemzHierarchyRecord!.ItemzHierarchyId!.GetLevel() != 2)
            {
                throw new ApplicationException($"Expected {movingItemzHierarchyRecord.Id} " +
                    $"to be 'ItemzType' but instead it's found in Itemz Hierarchy Record " +
                    $"as '{movingItemzHierarchyRecord.RecordType}' ");
            }

            //// EXPLANATION :: WE DO NOT HAVE TO BREAK EXECUTION IF TARGET PROJECT IS THE SAME AS CURRENT PROJECT.
            //// THIS IS BECAUSE WE NEED TO SUPPORT MOVING ITEMZ TYPE TO THE TOP OF THE CURRENT PROJECT'S 
            //// CHILD ITEMZ NODE OR AT THE BOTTOM. WE CAN'T USE MOVE ITEMZ TYPE BETWEEN TWO EXISTING ITEMZTYPE FOR THIS 
            //// PURPOSE BECAUSE TOP AND BOTTOM IS NOT BETWEEN TWO EXISTING ITEMZTYPES. SO WE ARE MARKING FOLLOWING
            //// LOGIC TO CHECK FOR SAME PROJECT AS COMMENTED.

            //var currentProjectHierarchyRecord = _context.ItemzHierarchy!.AsNoTracking()  
            //    .Where(ih => ih.ItemzHierarchyId == movingItemzHierarchyRecord.ItemzHierarchyId!.GetAncestor(1));

            //if (currentProjectHierarchyRecord.Any())
            //{
            //    if (currentProjectHierarchyRecord.FirstOrDefault()!.Id == targetProjectId)
            //    {
            //        throw new ApplicationException($"Current parent Project ID is {currentProjectHierarchyRecord.FirstOrDefault().Id} for ItemzType with ID {movingItemzTypeId}. " +
            //            $"ItemzType can not be moved under its current parent project!");
            //    }
            //}

            originalItemzHierarchyIdString = movingItemzHierarchyRecord!.ItemzHierarchyId!.ToString();
            allDescendentItemzHierarchyRecord = await _context.ItemzHierarchy!
                .Where(ih => ih.ItemzHierarchyId!.IsDescendantOf(movingItemzHierarchyRecord!.ItemzHierarchyId)).ToListAsync();

            // RemoveItemzTypeJoinItemzRecord(movingItemzTypeId);


            var newRootHierarchyRecord = _context.ItemzHierarchy!.AsNoTracking()
                            .Where(ih => ih.Id == targetProjectId);
            var newRootHierarchyRecordLevel = newRootHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId!.GetLevel();
            if (newRootHierarchyRecord.Count() != 1)
            {
                throw new ApplicationException($"{newRootHierarchyRecord.Count()} records found for the " +
                    $"New Root Hierarchy Id {targetProjectId} in the system. " +
                    $"Expected 1 record but instead found {newRootHierarchyRecord.Count()}");
            }

            if (newRootHierarchyRecordLevel != 1)
            {
                throw new ApplicationException($"New Root Hierarchy record has to be of type 'Project'");
            }

            // EXPLANATION : We are using SQL Server HierarchyID field type. Now we can use EF Core special
            // methods to query for all Decendents as per below. We are actually finding all Decendents by saying
            // First find the ItemzHierarchy record where ID matches RootItemzType ID. This is expected to be the
            // ItemzType ID itself which is the root OR parent to newly added Itemz.
            // Then we find all desendents of Repository which is nothing but existing Itemz(s). 

            var childItemzHierarchyRecords = await _context.ItemzHierarchy!
                    .AsNoTracking()
                    .Where(ih => ih.ItemzHierarchyId!.GetAncestor(1) == newRootHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId!)
                    .OrderBy(ih => ih.ItemzHierarchyId!)
                    .ToListAsync();

            if (childItemzHierarchyRecords.Count() == 0)
            {
                movingItemzHierarchyRecord!.ItemzHierarchyId = newRootHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId!
                    .GetDescendant(null, null);
            }
            else
            {
                if (atBottomOfChildNodes)
                {

                    movingItemzHierarchyRecord!.ItemzHierarchyId = newRootHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId!
                                        .GetDescendant(childItemzHierarchyRecords.LastOrDefault()!.ItemzHierarchyId
                                                       , null);
                }
                else
                {
                    movingItemzHierarchyRecord!.ItemzHierarchyId = newRootHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId!
                    .GetDescendant(null
                                    , childItemzHierarchyRecords.FirstOrDefault()!.ItemzHierarchyId);
                }
            }

            var newItemzHierarchyIdString = movingItemzHierarchyRecord!.ItemzHierarchyId!.ToString();

            foreach (var descendentItemzHierarchyRecord in allDescendentItemzHierarchyRecord)
            {
                Regex oldValueRegEx = new Regex(originalItemzHierarchyIdString);
                descendentItemzHierarchyRecord.ItemzHierarchyId = HierarchyId.Parse(
                    (oldValueRegEx.Replace((descendentItemzHierarchyRecord!
                                            .ItemzHierarchyId!.ToString())
                                            , newItemzHierarchyIdString
                                            , 1)
                    )
                );
            }

            // EXPLANATION :: Now that ItemzType has been moved to another project,
            // we should update ItemzType to Project One to One relationship data as well.

            // var found_ItemzType = await _context.ItemzTypes!.Where(it => it.Id == movingItemzTypeId).ToListAsync();
            if (found_ItemzType.Any())
            {
                // TODO :: FOLLOWING LOOP IS ACTUALLY NOT NEEDED. WE SHOULD ONLY UPDATE ONE RECORD WHICH 
                // IS GOING TO BE FirstOrDefault() ONCE. LOOK INTO VALIDATING THAT THIS LOOPING IS REUQIRED
                // AND WE SHOULD ONLY MAKE ONE SINGLE UPDATE RATHER THEN LOOPING THROUGH MANY IF MULTIPLE
                // RECORDS ARE RETURNED FOR SOME REASON.

                //foreach (var it in found_ItemzType)
                //{
                    found_ItemzType.FirstOrDefault()!.ProjectId = targetProjectId;
                //}
            }
        }













        public async Task MoveItemzTypeBetweenTwoHierarchyRecordsAsync(Guid between1stItemzTypeId, Guid between2ndItemzTypeId, Guid movingItemzTypeId)
        {
            if (between1stItemzTypeId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(between1stItemzTypeId));
            }
            if (between2ndItemzTypeId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(between2ndItemzTypeId));
            }

            if (movingItemzTypeId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(movingItemzTypeId));
            }

            if ((movingItemzTypeId == between1stItemzTypeId) || (movingItemzTypeId == between2ndItemzTypeId ))
            {
                throw new ApplicationException($"Moving ItemzType ID can not be same " +
                    $"as Lower or Upper bound between ItemzTypeIDs.");
            }

            var tempFirstItemzType = _context.ItemzHierarchy!.AsNoTracking()
                                        .Where(ih => ih.Id == between1stItemzTypeId);
            if ((tempFirstItemzType).Count() != 1)
            {
                throw new ApplicationException("For Between 1st ItemzType, Either no hierarchy record was " +
                            "found OR more then one hierarchy record were found in the system");
            }

            if ((tempFirstItemzType.FirstOrDefault()!.RecordType != "ItemzType"))
            {
                throw new ApplicationException("Incorrect Record Type for First ItemzType. " +
                    "Instead of 'ItemzType' it is '" + tempFirstItemzType.FirstOrDefault()!.RecordType + "'");
            }

            var tempSecondItemzType = _context.ItemzHierarchy!.AsNoTracking()
                                        .Where(ih => ih.Id == between2ndItemzTypeId);
            if ((tempSecondItemzType).Count() != 1)
            {
                throw new ApplicationException("For Between 2nd ItemzType, Either no hierarchy record was " +
                            "found OR more then one hierarchy record were found in the system");
            }

            if ((tempSecondItemzType.FirstOrDefault()!.RecordType != "ItemzType"))
            {
                throw new ApplicationException("Incorrect Record Type for Second ItemzType. " +
                    "Instead of 'ItemzType' it is '" + tempSecondItemzType.FirstOrDefault()!.RecordType + "'");
            }

            if (tempSecondItemzType.FirstOrDefault()!.ItemzHierarchyId < tempFirstItemzType.FirstOrDefault()!.ItemzHierarchyId)
            {
                throw new ApplicationException($"1st ItemzType Hierarchy ID is '{tempFirstItemzType.FirstOrDefault()!.ItemzHierarchyId!.ToString()}' " +
                    $"which is greater then 2nd ItemzType Hirarchy ID as '{tempSecondItemzType.FirstOrDefault()!.ItemzHierarchyId!.ToString()}'. " +
                                   $"Provided 1st ItemzType ID is '{tempFirstItemzType.FirstOrDefault()!.Id}' and 2nd ItemzType ID is '{tempSecondItemzType.FirstOrDefault()!.Id}' ");
            }

            if (!(tempFirstItemzType.FirstOrDefault()!.ItemzHierarchyId!.GetAncestor(1) ==
                    tempSecondItemzType.FirstOrDefault()!.ItemzHierarchyId!.GetAncestor(1)))
            {
                throw new ApplicationException("Between ItemzType do not belong to the same Parent. FirstItemzType " +
                    "belongs to Hierarchy ID '" + tempFirstItemzType.FirstOrDefault()!.ItemzHierarchyId!.GetAncestor(1)!.ToString()
                    + "' and SecondItemzType belongs to HierarchyID '" + tempSecondItemzType.FirstOrDefault()!.ItemzHierarchyId!.GetAncestor(1)!.ToString() + "'!"
                    );
            }

            var gapBetweenLowerAndUpper = _context.ItemzHierarchy!.AsNoTracking()
                .Where(ih => ih.ItemzHierarchyId >= tempFirstItemzType.FirstOrDefault()!.ItemzHierarchyId
                && ih.ItemzHierarchyId <= tempSecondItemzType.FirstOrDefault()!.ItemzHierarchyId
                && ih.ItemzHierarchyId!.GetLevel() == tempFirstItemzType.FirstOrDefault()!.ItemzHierarchyId!.GetLevel());

            if ((gapBetweenLowerAndUpper).Count() != 2)
            {
                throw new ApplicationException("1st and 2nd ItemzType are not next to each other. " +
                    "Please consider moving ItemzType between two ItemzType which are next to each other. " +
                    "Total ItemzType found in between 1st and 2nd ItemzType are '" + (gapBetweenLowerAndUpper).Count() + "'");
            }


            var movingItemzTypeHierarchyRecordList = _context.ItemzHierarchy!
                    .Where(ih => ih.Id == movingItemzTypeId);
            var foundMovingItemzTypeHierarchyRecordCount = movingItemzTypeHierarchyRecordList.Count();

            if (foundMovingItemzTypeHierarchyRecordCount != 1)
            {
                throw new ApplicationException($"{movingItemzTypeHierarchyRecordList.Count()} records found for the " +
                    $"moving ItemzType Id {movingItemzTypeId} in the system. " +
                    $"Expected 1 record but instead found {movingItemzTypeHierarchyRecordList.Count()}");
            }

            // DEFINE VARIABLES THAT WE USE IN AND OUT OF CODE BLOCKS
            var movingItemzTypeHierarchyRecord = new ItemzHierarchy();
            string originalItemzTypeHierarchyIdString = "";
            List<ItemzHierarchy> allDescendentItemzHierarchyRecord = new List<ItemzHierarchy>();
            var found_ItemzType = await _context.ItemzTypes!.Where(it => it.Id == movingItemzTypeId).ToListAsync();
            var target_Project = _context.ItemzHierarchy!.AsNoTracking()
                                    .Where(ih => ih.ItemzHierarchyId ==
                                        tempFirstItemzType.FirstOrDefault()!.ItemzHierarchyId!
                                    .GetAncestor(1)).ToList();
            if (!(target_Project.Count != 0))
            {
                throw new ApplicationException($"Expected target Project for first ItemzType between two ItemzTypes " +
                    $"could not be found. Please contact your System Administrator.");
            }
            //var target_ProjectId = _context.ItemzHierarchy!.AsNoTracking()
            //            .Where(ih => ih.ItemzHierarchyId ==
            //                tempFirstItemzType.FirstOrDefault()!.ItemzHierarchyId!
            //                    .GetAncestor(1)).ToList().FirstOrDefault()!.Id;


            if (found_ItemzType.FirstOrDefault()!.IsSystem == true)
            {
                if (found_ItemzType.FirstOrDefault()!.ProjectId != target_Project.FirstOrDefault()!.Id)
                {
                    throw new ApplicationException($"System does not support moving 'Parking Lot' system ItemzType " +
                        $"from source Project with ID {found_ItemzType.FirstOrDefault()!.ProjectId}" +
                        $" to target Project with Id {target_Project.FirstOrDefault()!.Id}");
                }
            }

            if (foundMovingItemzTypeHierarchyRecordCount == 1)
            {
                movingItemzTypeHierarchyRecord = movingItemzTypeHierarchyRecordList.FirstOrDefault();
                if (! ((movingItemzTypeHierarchyRecord!.ItemzHierarchyId!.GetLevel() == 2) 
                    || (movingItemzTypeHierarchyRecord!.RecordType == "ItemzType")))
                {
                    throw new ApplicationException($"Expected {movingItemzTypeHierarchyRecord.Id} " +
                        $"to be 'ItemzType' but instead it's found moving ItemzType Hierarchy Record " +
                        $"as '{movingItemzTypeHierarchyRecord.RecordType}' ");
                }

                originalItemzTypeHierarchyIdString = movingItemzTypeHierarchyRecord!.ItemzHierarchyId!.ToString();
                allDescendentItemzHierarchyRecord = await _context.ItemzHierarchy!
                    .Where(ih => ih.ItemzHierarchyId!.IsDescendantOf(movingItemzTypeHierarchyRecord!.ItemzHierarchyId)).ToListAsync();

                // EXPLANATION: I BELIEVE THERE IS NOT NEED TO CHECK IF MOVING ITEMZTYPES ALL DECENDENT CHILD 
                // ITEMZ NODES CONTAINS TARGET FIRST AND SECOND BETWEEN ITEMZTYPE ID. 
                // WE MAY HAVE TO CHECK SOMETHING SIMILAR IN THE FUTURE IF WE SUPPORT SUB ITEMZTYPE RECORDS
                // IN THE FUTURE BUT WE WILL LOOK INTO IT WHEN WE THINK OF SUPPORTING THAT IN THE FUTURE.

                //if (allDescendentItemzHierarchyRecord.Any(ih => ih.Id == tempFirstItemzType.FirstOrDefault().Id || ih.Id == tempSecondItemzType.FirstOrDefault().Id))
                //{
                //    throw new ApplicationException($"System does not support moving parent ItemzType under it's existing child Itemz. " +
                //        $"Moving ItemzType {movingItemzTypeHierarchyRecord.Id} (with hierarchy id '" +
                //            $"{movingItemzTypeHierarchyRecord.ItemzHierarchyId.ToString()}') is a parent to either " +
                //        $"{tempFirstItemzType.FirstOrDefault().Id} (with hierarchy id '" +
                //        $"{tempFirstItemzType.FirstOrDefault().ItemzHierarchyId.ToString()}') " +
                //        $"OR {tempSecondItemzType.FirstOrDefault().Id}  (with hierarchy id '" +
                //        $"{tempSecondItemzType.FirstOrDefault().ItemzHierarchyId.ToString()}') ");
                //}

                movingItemzTypeHierarchyRecord!.ItemzHierarchyId = tempFirstItemzType.FirstOrDefault()!.ItemzHierarchyId!.GetAncestor(1)!
                            .GetDescendant(tempFirstItemzType.FirstOrDefault()!.ItemzHierarchyId
                            , tempSecondItemzType.FirstOrDefault()!.ItemzHierarchyId == tempFirstItemzType.FirstOrDefault()!.ItemzHierarchyId
                             ? HierarchyId.Parse(
                                  HierarchyIdStringHelper.ManuallyGenerateHierarchyIdNumberString(
                                      tempFirstItemzType.FirstOrDefault()!.ItemzHierarchyId!.ToString()
                                      , diffValue: 2
                                      , addDecimal: true
                                      )
                               )
                            : tempSecondItemzType.FirstOrDefault()!.ItemzHierarchyId);

                var newItemzTypeHierarchyIdString = movingItemzTypeHierarchyRecord!.ItemzHierarchyId!.ToString();

                foreach (var descendentItemzHierarchyRecord in allDescendentItemzHierarchyRecord)
                {
                    Regex oldValueRegEx = new Regex(originalItemzTypeHierarchyIdString);
                    descendentItemzHierarchyRecord.ItemzHierarchyId = HierarchyId.Parse(
                        (oldValueRegEx.Replace((descendentItemzHierarchyRecord!
                                                .ItemzHierarchyId!.ToString())
                                                , newItemzTypeHierarchyIdString
                                                , 1)
                        )
                    );
                }

            }
           
            // EXPLANATION :: In both the cases, whether we are adding new ItemzType or Moving existing ItemzType
            // between two ItemzType in the hierarchy, we need to make sure that we check the newly added or moved ItemzType
            // location. If new location is under Project then we have to add entry in ItemzTypeJoinItemz table.
            // which is what following code is doing.

            var parentOfNewlyAddedtempItemzTypeHierarchy = _context.ItemzHierarchy!.AsNoTracking()
                .Where(ih => ih.ItemzHierarchyId == movingItemzTypeHierarchyRecord.ItemzHierarchyId.GetAncestor(1)).FirstOrDefault();

            if (parentOfNewlyAddedtempItemzTypeHierarchy!.ItemzHierarchyId!.GetLevel() == 1) // if it's Project
            {
                // EXPLANATION :: Now that ItemzType has been moved to another project,
                // we should update ItemzType to Project One to One relationship data as well.

                if (found_ItemzType.Any())
                {
                    //foreach (var it in found_it)
                    //{
                        found_ItemzType.FirstOrDefault()!.ProjectId = parentOfNewlyAddedtempItemzTypeHierarchy.Id;
                    //}
                }
            }
        }

		public async Task<Guid> CopyItemzTypeAsync(Guid ItemzTypeId)
		{
			if (ItemzTypeId == Guid.Empty)
			{
				throw new ArgumentNullException(nameof(ItemzTypeId));
			}

			// EXPLANATION : Check if ItemzType exist in ItemzType Table

			if (!_context.ItemzTypes!.Where(it => it.Id == ItemzTypeId).Any())
			{
				throw new ArgumentException(nameof(ItemzTypeId));
			}

			// EXPLANATION : Check if ItemzType exist in ItemzHierarchy Table.
			// System is desinged to expect all ItemzType to be available in ItemzHierarhcy 
            // This way, we know who the Parent Project record is and also we create association
            // between ItemzType and immidate Child Itemz when we perform Copy of ItemzType

			if (!_context.ItemzHierarchy!.AsNoTracking()
							.Where(ih => ih.Id == ItemzTypeId).Any())
			{
				throw new ArgumentException(nameof(ItemzTypeId));
			}

			Guid returnValue = Guid.Empty;
			var OUTPUT_ID = new SqlParameter
			{
				ParameterName = "OUTPUT_Id",
				Direction = System.Data.ParameterDirection.Output,
				SqlDbType = System.Data.SqlDbType.UniqueIdentifier,
			};

			var sqlParameters = new[]
			{
				new SqlParameter
				{
					ParameterName = "RecordID",
					Value = ItemzTypeId,
					SqlDbType = System.Data.SqlDbType.UniqueIdentifier,
				}
			};

			sqlParameters = sqlParameters.Append(OUTPUT_ID).ToArray();

			var _ = await _context.Database.ExecuteSqlRawAsync(sql: "EXEC userProcCopyRecordWithChildrenByRecordID @RecordID, @OUTPUT_Id = @OUTPUT_Id OUT", parameters: sqlParameters);
			returnValue = (Guid)OUTPUT_ID.Value;

			return returnValue;
		}

	}
}
