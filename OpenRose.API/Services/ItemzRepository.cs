// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using ItemzApp.API.DbContexts;
using ItemzApp.API.Entities;
using ItemzApp.API.Helper;
using ItemzApp.API.Models;
using ItemzApp.API.ResourceParameters;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.CodeAnalysis;
using Microsoft.Build.Evaluation;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Win32.SafeHandles;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ItemzApp.API.DbContexts.SQLHelper;

namespace ItemzApp.API.Services
{

    public class ItemzRepository : IItemzRepository, IDisposable
    {
        private readonly ItemzContext _context;
        private readonly IPropertyMappingService _propertyMappingService;
        public ItemzRepository(ItemzContext context,
            IPropertyMappingService propertyMappingService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _propertyMappingService = propertyMappingService ??
                throw new ArgumentNullException(nameof(propertyMappingService));
        }

        public async Task<Itemz?> GetItemzAsync(Guid ItemzId)
        {

            if (ItemzId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(ItemzId));
            }

            return await _context.Itemzs!
                .Where(c => c.Id == ItemzId).AsNoTracking().FirstOrDefaultAsync();
            
            // EXPLANATION: It is possible to return Itemz data with details
            // about FromItemzJoinItemzTrace + ToItemzJoinItemzTrace + ItemzTypeJoinItemz together 
            // as per below option.
            
            //return await _context.Itemzs!
            //    .Include(i => i.FromItemzJoinItemzTrace)
            //    .Include(i => i.ToItemzJoinItemzTrace)
            //    .Include(i => i.ItemzTypeJoinItemz)
            //    .Where(c => c.Id == ItemzId).AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<Itemz?> GetItemzForUpdatingAsync(Guid ItemzId)
        {

            if (ItemzId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(ItemzId));
            }

            return await _context.Itemzs!
                .Where(c => c.Id == ItemzId).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Itemz>> GetItemzsAsync(IEnumerable<Guid> itemzIds)
        {
            if (itemzIds == null)
            {
                throw new ArgumentNullException(nameof(itemzIds));
            }

            return await _context.Itemzs.AsNoTracking().Where(a => itemzIds.Contains(a.Id))
                .OrderBy(a => a.Name)
                .ToListAsync();
        }
        public PagedList<Itemz>? GetItemzs(ItemzResourceParameter itemzResourceParameter)
        {
            // TODO: Should we check for itemzResourceParameter being null?
            // There are chances that we just want to get all the itemz and
            // consumer of the API might now pass in necessary values for pagging.

            if (itemzResourceParameter == null)
            {
                throw new ArgumentNullException(nameof(itemzResourceParameter));
            }
            try
            {
                if (_context.Itemzs!.Count<Itemz>() > 0) //TODO: await and use CountAsync
                {
                    var itemzCollection = _context.Itemzs!.AsQueryable<Itemz>(); // as IQueryable<Itemz>;

                    if (!string.IsNullOrWhiteSpace(itemzResourceParameter.OrderBy))
                    {
                        var itemzPropertyMappingDictionary =
                                               _propertyMappingService.GetPropertyMapping<Models.GetItemzDTO, Itemz>();

                        itemzCollection = itemzCollection.ApplySort(itemzResourceParameter.OrderBy,
                            itemzPropertyMappingDictionary).AsNoTracking();
                    }

                    // EXPLANATION: Pagging feature should be implemented at the end 
                    // just before calling ToList. This will make sure that any filtering,
                    // sorting, grouping, etc. that we implement on the data are 
                    // put in place before calling ToList. 

                    return PagedList<Itemz>.Create(itemzCollection,
                        itemzResourceParameter.PageNumber,
                        itemzResourceParameter.PageSize);
                }
                return null;
            }
            catch (Exception ex)
            {
                // TODO: It's not good that we capture Generic Exception and then 
                // return null here. Basically, I wanted to check if we have 
                // itemzs returned from the DB and if it does not then
                // it should simply return null back to the calling function.
                // One has to learn how to do this gracefully as part of Entity Framework 
                return null;
            }
        }

        public PagedList<GetItemzWithBasePropertiesDTO>? GetOrphanItemzs(ItemzResourceParameter itemzResourceParameter)
        {
            // TODO: Should we check for itemzResourceParameter being null?
            // There are chances that we just want to get all the itemz and
            // consumer of the API might now pass in necessary values for pagging.

            if (itemzResourceParameter == null)
            {
                throw new ArgumentNullException(nameof(itemzResourceParameter));
            }
            try
            {
                if (_context.Itemzs!.Count<Itemz>() > 0) //TODO: await and use CountAsync
                {
                    // TODO :: Instead of utilizing ItemzTypeJoinItemz for finding Orphaned Itemz,
                    // now that we have implemented support for Hierarchy, we should utilize ItemzHierarchy
                    // instead. 

                    ////               var itemzCollection = _context.Itemzs
                    ////                   .AsNoTracking()
                    ////				.Include(i => i.ItemzTypeJoinItemz)
                    ////                   .Where (i => i.ItemzTypeJoinItemz!.Count() == 0)
                    //// 			.AsQueryable<Itemz>(); // as IQueryable<Itemz>;

                    ////if (!string.IsNullOrWhiteSpace(itemzResourceParameter.OrderBy))
                    ////               {
                    ////                   var itemzPropertyMappingDictionary =
                    ////                                          _propertyMappingService.GetPropertyMapping<Models.GetItemzWithBasePropertiesDTO, Models.GetItemzWithBasePropertiesDTO>();

                    ////                   itemzCollection = itemzCollection.ApplySort(itemzResourceParameter.OrderBy,
                    ////                       itemzPropertyMappingDictionary).AsNoTracking();
                    ////               }

                    ////               // EXPLANATION: Pagging feature should be implemented at the end 
                    ////               // just before calling ToList. This will make sure that any filtering,
                    ////               // sorting, grouping, etc. that we implement on the data are 
                    ////               // put in place before calling ToList. 

                    ////               return PagedList<Itemz>.Create(itemzCollection,
                    ////                   itemzResourceParameter.PageNumber,
                    ////                   itemzResourceParameter.PageSize);


                    ////var itemsNotInHierarchy = from item in _context.Itemzs 
                    ////                          join itemHierarchy in _context.ItemzHierarchy 
                    ////                          on item.Id equals itemHierarchy.Id into itemHierarchyGroup 
                    ////                          from itemHierarchy 
                    ////                          in itemHierarchyGroup.DefaultIfEmpty() 
                    ////                          where itemHierarchy == null
                    ////                          select new GetItemzWithBasePropertiesDTO 
                    ////                          { 
                    ////                              Id = item.Id
                    ////                            , Name = item.Name
                    ////                            , Status = item.Status
                    ////                            , Priority = item.Priority.ToString()
                    ////                            , Severity = item.Severity.ToString()
                    ////                            , CreatedDate = item.CreatedDate 
                    ////                          }; 

                    var itemsNotInHierarchy = from item in _context.Itemzs
                                              join itemHierarchy in _context.ItemzHierarchy 
                                              on item.Id equals itemHierarchy.Id into itemHierarchyGroup
                                              from itemHierarchy in itemHierarchyGroup.DefaultIfEmpty()
                                              where itemHierarchy == null
                                              select new GetItemzWithBasePropertiesDTO
                                              {
                                                  Id = item.Id,
                                                  Name = item.Name,
                                                  Status = item.Status.ToString(), // Still Enum at this stage
                                                  Priority = item.Priority.ToString() , // Still Enum at this stage
                                                  Severity = item.Severity.ToString(), // Still Enum at this stage
                                                  CreatedDate = item.CreatedDate 
                                              };

                    if (!string.IsNullOrWhiteSpace(itemzResourceParameter.OrderBy))
                    {
                        if (itemzResourceParameter.OrderBy.ToLower() != "name"
                            && itemzResourceParameter.OrderBy.ToLower() != "name desc" 
                            && itemzResourceParameter.OrderBy.ToLower() != "createddate"
                            && itemzResourceParameter.OrderBy.ToLower() != "createddate desc" )
                        {
                            // TODO :: We currently allow ordering by only name and createddate. We have to support ENUMs in the future for
                            // Status, Priority and Severity. 
                            // Also, consider response code for not supported OrderBy value in the future so that we can
                            // show elegent error message to the users.
                            return null; 
                        }
                        var itemzPropertyMappingDictionary =
                                               _propertyMappingService.GetPropertyMapping<Models.GetItemzWithBasePropertiesDTO, Models.GetItemzWithBasePropertiesDTO>();

                        itemsNotInHierarchy = itemsNotInHierarchy.ApplySort(itemzResourceParameter.OrderBy,
                            itemzPropertyMappingDictionary).AsNoTracking();
                    }


                    //if (!string.IsNullOrWhiteSpace(itemzResourceParameter.OrderBy))
                    //{
                    //    var itemzPropertyMappingDictionary =
                    //                           _propertyMappingService.GetPropertyMapping<Models.GetItemzWithBasePropertiesDTO, Models.GetItemzWithBasePropertiesDTO>();

                    //    itemsNotInHierarchy = itemsNotInHierarchy.ApplySort(itemzResourceParameter.OrderBy,
                    //        itemzPropertyMappingDictionary).AsNoTracking()
                    //        .Select(i => new GetItemzWithBasePropertiesDTO
                    //        {
                    //            Id = i.Id,
                    //            Name = i.Name,
                    //            Status = i.Status.ToString(), // Convert to string in projection
                    //            Priority = i.Priority.ToString(), // Convert to string in projection
                    //            Severity = i.Severity.ToString(), // Convert to string in projection
                    //            CreatedDate = i.CreatedDate
                    //        }
                    //        ).ToList(); ;


                    // EXPLANATION: Pagging feature should be implemented at the end 
                    // just before calling ToList. This will make sure that any filtering,
                    // sorting, grouping, etc. that we implement on the data are 
                    // put in place before calling ToList. 

                    return PagedList<GetItemzWithBasePropertiesDTO>.Create(itemsNotInHierarchy,
                        itemzResourceParameter.PageNumber,
                        itemzResourceParameter.PageSize);
                    
                }
                return null;
            }
            catch (Exception ex)
            {
                // TODO: It's not good that we capture Generic Exception and then 
                // return null here. Basically, I wanted to check if we have 
                // itemzs returned from the DB and if it does not then
                // it should simply return null back to the calling function.
                // One has to learn how to do this gracefully as part of Entity Framework 
                return null;
            }
        }

        public async Task<int> GetOrphanItemzsCount()
        {

			// TODO :: Instead of utilizing ItemzTypeJoinItemz for finding Orphaned Itemz,
			// now that we have implemented support for Hierarchy, we should utilize ItemzHierarchy
			// instead. 

			var foundOrphanItemzsCount = -1;
            //foundOrphanItemzsCount = await _context.Itemzs
            //            .AsNoTracking()
            //            .Include(i => i.ItemzTypeJoinItemz)
            //            .Where(i => i.ItemzTypeJoinItemz!.Count() == 0)
            //            .CountAsync();

            foundOrphanItemzsCount = (from item in _context.Itemzs
                                join itemHierarchy in _context.ItemzHierarchy
                                on item.Id equals itemHierarchy.Id into itemHierarchyGroup
                                from itemHierarchy in itemHierarchyGroup.DefaultIfEmpty()
                                where itemHierarchy == null
                                select item)
                                .Count();

            return foundOrphanItemzsCount > 0 ? foundOrphanItemzsCount : -1;
        }

        public async Task<int> GetItemzsCountByItemzType(Guid itemzTypeId)
        {
            if (itemzTypeId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(itemzTypeId));
            }

            var rootItemzType = _context.ItemzHierarchy!.AsNoTracking()
                .Where(ih => ih.Id == itemzTypeId).FirstOrDefault();

             return (await _context.ItemzHierarchy!
                    .AsNoTracking()
                    .Where(ih => ih.ItemzHierarchyId!.IsDescendantOf(rootItemzType!.ItemzHierarchyId))
                    .CountAsync())
                    - 1; // Minus One becaues Count includes ItemzType itself along with it's SubItemz
        }

        public PagedList<Itemz>? GetItemzsByItemzType(Guid itemzTypeId, ItemzResourceParameter itemzResourceParameter)
        {
            // TODO: Should we check for itemzResourceParameter being null?
            // There are chances that we just want to get all the itemz and
            // consumer of the API might now pass in necessary values for pagging.

            if (itemzResourceParameter == null)
            {
                throw new ArgumentNullException(nameof(itemzResourceParameter));
            }

            if (itemzTypeId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(itemzTypeId));
            }
            try
            {
                    // TODO: This only returns Itemz which are associated with ItemzType via ItemzTypeJoinItemz
                    // NOW THAT WE HAVE IMPLEMENTED HIERARCHY, WE NEED TO MAKE SURE THAT WE RETURN ITEMZ
                    // FOR ENTIRE ITEMZ TYPE. OTHERWISE WE CAN ALSO JUST RETURN ITEMZ WHICH ARE ASSOCIATED WITH
                    // ITEMZ TYPE BUT INSTEAD USE HIERARCHY TABLE INSTEAD OF ItemzTypeJoinItemz TABLE.
                    // WE WILL HAVE TO TAKE THIS DECISION SOON.

                    var itemzCollection = _context.Itemzs
                        .Include(i => i.ItemzTypeJoinItemz)
                        //                        .ThenInclude(PjI => PjI.ItemzType)
                        .Where(i => i.ItemzTypeJoinItemz!.Any(itji => itji.ItemzTypeId == itemzTypeId));
                    if (!(itemzCollection.Any()))
                    {
						return null;
					}
                    //     .Where(i => i.  . AsQueryable<Itemz>(); // as IQueryable<Itemz>;

                    if (!string.IsNullOrWhiteSpace(itemzResourceParameter.OrderBy))
                    {
                        var itemzPropertyMappingDictionary =
                                               _propertyMappingService.GetPropertyMapping<Models.GetItemzDTO, Itemz>();

                        itemzCollection = itemzCollection.ApplySort(itemzResourceParameter.OrderBy,
                            itemzPropertyMappingDictionary).AsNoTracking();
                    }

                    // EXPLANATION: Pagging feature should be implemented at the end 
                    // just before calling ToList. This will make sure that any filtering,
                    // sorting, grouping, etc. that we implement on the data are 
                    // put in place before calling ToList. 

                    return PagedList<Itemz>.Create(itemzCollection,
                        itemzResourceParameter.PageNumber,
                        itemzResourceParameter.PageSize);

            }
            catch (Exception ex)
            {
                // TODO: It's not good that we capture Generic Exception and then 
                // return null here. Basically, I wanted to check if we have 
                // itemzs returned from the DB and if it does not then
                // it should simply return null back to the calling function.
                // One has to learn how to do this gracefully as part of Entity Framework 
                return null;
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

        public void AddItemz(Itemz itemz)
        {
            if (itemz == null)
            {
                throw new ArgumentNullException(nameof(itemz));
            }
            _context.Itemzs!.Add(itemz);
        }

        public async Task AddOrMoveItemzBetweenTwoHierarchyRecordsAsync(Guid between1stItemzId, Guid between2ndItemzId, Guid addingOrMovingItemzId, string? itemzName)
        {
            if (between1stItemzId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(between1stItemzId));
            }
            if (between2ndItemzId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(between2ndItemzId));
            }

            if (addingOrMovingItemzId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(addingOrMovingItemzId));
            }


            var tempFirstItemz = _context.ItemzHierarchy!.AsNoTracking()
                                        .Where(ih => ih.Id == between1stItemzId);
            if ((tempFirstItemz).Count() != 1)
            {
                throw new ApplicationException("For Between 1st Itemz, Either no hierarchy record was " +
                            "found OR more then one hierarchy record were found in the system");
            }

            if ((tempFirstItemz.FirstOrDefault()!.RecordType != "Itemz"))
            {
                throw new ApplicationException("Incorrect Record Type for First Itemz. " +
                    "Instead of 'Itemz' it is '" + tempFirstItemz.FirstOrDefault()!.RecordType + "'");
            }

            var tempSecondItemz = _context.ItemzHierarchy!.AsNoTracking()
                                        .Where(ih => ih.Id == between2ndItemzId);
            if ((tempSecondItemz).Count() != 1)
            {
                throw new ApplicationException("For Between 2nd Itemz, Either no hierarchy record was " +
                            "found OR more then one hierarchy record were found in the system");
            }

            if ((tempSecondItemz.FirstOrDefault()!.RecordType != "Itemz"))
            {
                throw new ApplicationException("Incorrect Record Type for Second Itemz. " +
                    "Instead of 'Itemz' it is '" + tempSecondItemz.FirstOrDefault()!.RecordType + "'");
            }

            if (tempSecondItemz.FirstOrDefault()!.ItemzHierarchyId < tempFirstItemz.FirstOrDefault()!.ItemzHierarchyId)
            {
                throw new ApplicationException($"1st Itemz Hierarchy ID is '{tempFirstItemz.FirstOrDefault()!.ItemzHierarchyId!.ToString()}' " +
                    $"which is greater then 2nd Itemz Hirarchy ID as '{tempSecondItemz.FirstOrDefault()!.ItemzHierarchyId!.ToString()}'. " +
                                   $"Provided 1st Itemz ID is '{tempFirstItemz.FirstOrDefault()!.Id}' and 2nd Itemz ID is '{tempSecondItemz.FirstOrDefault()!.Id}' ");
            }

            if (!(tempFirstItemz.FirstOrDefault()!.ItemzHierarchyId!.GetAncestor(1) ==
                    tempSecondItemz.FirstOrDefault()!.ItemzHierarchyId!.GetAncestor(1)))
            {
                throw new ApplicationException("Between Itemz do not belong to the same Parent. FirstItemz " +
                    "belongs to Hierarchy ID '" + tempFirstItemz.FirstOrDefault()!.ItemzHierarchyId!.GetAncestor(1)!.ToString()
                    + "' and SecondItemz belongs to HierarchyID '" + tempSecondItemz.FirstOrDefault()!.ItemzHierarchyId!.GetAncestor(1)!.ToString() + "'!"
                    );
            }

            var gapBetweenLowerAndUpper = _context.ItemzHierarchy!.AsNoTracking()
                .Where(ih => ih.ItemzHierarchyId >= tempFirstItemz.FirstOrDefault()!.ItemzHierarchyId
                && ih.ItemzHierarchyId <= tempSecondItemz.FirstOrDefault()!.ItemzHierarchyId
                && ih.ItemzHierarchyId!.GetLevel() == tempFirstItemz.FirstOrDefault()!.ItemzHierarchyId!.GetLevel());

            if ((gapBetweenLowerAndUpper).Count() != 2)
            {
                throw new ApplicationException("1st and 2nd Itemz are not next to each other. " +
                    "Please consider adding new Itemz between two Itemz which are next to each other. " +
                    "Total Itemz found in between 1st and 2nd Itemz are '" + (gapBetweenLowerAndUpper).Count() + "'");
            }


            var addingOrMovingItemzHierarchyRecordList = _context.ItemzHierarchy!
                    .Where(ih => ih.Id == addingOrMovingItemzId);
            var foundMovingItemzHierarchyRecordCount = addingOrMovingItemzHierarchyRecordList.Count();

            if (foundMovingItemzHierarchyRecordCount > 1)
            {
                throw new ApplicationException($"{addingOrMovingItemzHierarchyRecordList.Count()} records found for the " +
                    $"moving Itemz Id {addingOrMovingItemzId} in the system. " +
                    $"Expected 1 record but instead found {addingOrMovingItemzHierarchyRecordList.Count()}");
            }

            // DEFINE VARIABLES THAT WE USE IN AND OUT OF CODE BLOCKS
            var addingOrMovingItemzHierarchyRecord = new ItemzHierarchy();
            string originalItemzHierarchyIdString = "";
            List<ItemzHierarchy> allDescendentItemzHierarchyRecord = new List<ItemzHierarchy>();


            if (foundMovingItemzHierarchyRecordCount == 1)
            {
                addingOrMovingItemzHierarchyRecord = addingOrMovingItemzHierarchyRecordList.FirstOrDefault();
                if (addingOrMovingItemzHierarchyRecord!.ItemzHierarchyId!.GetLevel() < 3)
                {
                    throw new ApplicationException($"Expected {addingOrMovingItemzHierarchyRecord.Id} " +
                        $"to be 'Itemz' but instead it's found in Itemz Hierarchy Record " +
                        $"as '{addingOrMovingItemzHierarchyRecord.RecordType}' ");
                }

                originalItemzHierarchyIdString = addingOrMovingItemzHierarchyRecord!.ItemzHierarchyId!.ToString();
                allDescendentItemzHierarchyRecord = await _context.ItemzHierarchy!
                    .Where(ih => ih.ItemzHierarchyId!.IsDescendantOf(addingOrMovingItemzHierarchyRecord!.ItemzHierarchyId)).ToListAsync();

                if (allDescendentItemzHierarchyRecord.Any( ih => ih.Id == tempFirstItemz.FirstOrDefault().Id || ih.Id == tempSecondItemz.FirstOrDefault().Id))
                {
                    throw new ApplicationException($"System does not support moving parent Itemz under it's existing child Itemz. " +
                        $"Moving Itemz {addingOrMovingItemzHierarchyRecord.Id} (with hierarchy id '" +
                            $"{addingOrMovingItemzHierarchyRecord.ItemzHierarchyId.ToString()}') is a parent to either " +
                        $"{tempFirstItemz.FirstOrDefault().Id} (with hierarchy id '" +
                        $"{tempFirstItemz.FirstOrDefault().ItemzHierarchyId.ToString()}') " +
                        $"OR {tempSecondItemz.FirstOrDefault().Id}  (with hierarchy id '" +
                        $"{tempSecondItemz.FirstOrDefault().ItemzHierarchyId.ToString()}') " );
                }

                // EXPLANATION : This method is used not only to add new records but now also to move existing
                // Itemz record from one place to another place between two existing Itemz records.
                // So in this scenarios where we are moving from one location to another location,
                // we have to make sure that we Remove previous relationship between ItemzType and Itemz
                // if it's already existing.

                RemoveItemzTypeJoinItemzRecord(addingOrMovingItemzId);

                addingOrMovingItemzHierarchyRecord!.ItemzHierarchyId = tempFirstItemz.FirstOrDefault()!.ItemzHierarchyId!.GetAncestor(1)!
                            .GetDescendant(tempFirstItemz.FirstOrDefault()!.ItemzHierarchyId
                            , tempSecondItemz.FirstOrDefault()!.ItemzHierarchyId == tempFirstItemz.FirstOrDefault()!.ItemzHierarchyId
                             ? HierarchyId.Parse(
                                  HierarchyIdStringHelper.ManuallyGenerateHierarchyIdNumberString(
                                      tempFirstItemz.FirstOrDefault()!.ItemzHierarchyId!.ToString()
                                      , diffValue: 2
                                      , addDecimal: true
                                      )
                               )
                            : tempSecondItemz.FirstOrDefault()!.ItemzHierarchyId);


                var newItemzHierarchyIdString = addingOrMovingItemzHierarchyRecord!.ItemzHierarchyId!.ToString();

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

            }
            else // IF WE ARE ADDING NEW ITEMZ BETWEEN TWO EXISTING ITEMZ THEN
            {
                addingOrMovingItemzHierarchyRecord = new Entities.ItemzHierarchy
                {
                    Id = addingOrMovingItemzId,
                    RecordType = "Itemz",
                    Name = itemzName ?? "",
                    ItemzHierarchyId = tempFirstItemz.FirstOrDefault()!.ItemzHierarchyId!.GetAncestor(1)!
                            .GetDescendant(tempFirstItemz.FirstOrDefault()!.ItemzHierarchyId
                            , tempSecondItemz.FirstOrDefault()!.ItemzHierarchyId == tempFirstItemz.FirstOrDefault()!.ItemzHierarchyId
                             ? HierarchyId.Parse(
                                  HierarchyIdStringHelper.ManuallyGenerateHierarchyIdNumberString(
                                      tempFirstItemz.FirstOrDefault()!.ItemzHierarchyId!.ToString()
                                      , diffValue: 2
                                      , addDecimal: true
                                      )
                               )
                            : tempSecondItemz.FirstOrDefault()!.ItemzHierarchyId),
                };

                var checkItemzWithNewHierarchyIdExists = _context.ItemzHierarchy!.AsNoTracking()
                                            .Where(ih => ih.ItemzHierarchyId!.ToString() == addingOrMovingItemzHierarchyRecord.ItemzHierarchyId.ToString());

                if ((checkItemzWithNewHierarchyIdExists).Count() > 0)
                {
                    throw new ApplicationException($"Itemz with HierarchyID '{addingOrMovingItemzHierarchyRecord.ItemzHierarchyId.ToString()}' already existing in the repository.");
                }

                await _context.ItemzHierarchy!.AddAsync(addingOrMovingItemzHierarchyRecord);

            }

            // EXPLANATION :: In both the cases, whether we are adding new Itemz or Moving existing Itemz
            // between two Itemz in the hierarchy, we need to make sure that we check the newly added or moved Itemz
            // location. If new location is under ItemzType then we have to add entry in ItemzTypeJoinItemz table.
            // which is what following code is doing.

            var parentOfNewlyAddedtempItemzHierarchy = _context.ItemzHierarchy!.AsNoTracking()
                .Where(ih => ih.ItemzHierarchyId == addingOrMovingItemzHierarchyRecord.ItemzHierarchyId.GetAncestor(1)).FirstOrDefault();

            if (parentOfNewlyAddedtempItemzHierarchy!.ItemzHierarchyId!.GetLevel() == 2) // if it's ItemzType
            {
                AddItemzTypeJoinItemzRecord(parentOfNewlyAddedtempItemzHierarchy.Id, addingOrMovingItemzHierarchyRecord.Id);
            }
        }

        public async Task<bool> SaveAsync()
        {
            return (await _context.SaveChangesAsync() >= 0);
        }

        public void AddItemzByItemzType(Itemz itemz, Guid itemzTypeId)
        {
            if (itemz == null)
            {
                throw new ArgumentNullException(nameof(itemz));
            }

            if (itemzTypeId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(itemzTypeId));
            }

            _context.Itemzs!.Add(itemz);
            
            // TODO : ERROR HANDLING 
            AddItemzTypeJoinItemzRecord(itemzTypeId, itemz.Id);

        }

        public async Task MoveItemzHierarchyAsync(Guid movingItemzId, Guid targetId, bool atBottomOfChildNodes = true, string? movingItemzName = null)
        {
            if (movingItemzId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(movingItemzId));
            }

            if (targetId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(targetId));
            }

            var movingItemzHierarchyRecordList = _context.ItemzHierarchy!
                .Where(ih => ih.Id == movingItemzId);
            var foundMovingItemzHierarchyRecordCount = movingItemzHierarchyRecordList.Count();
            if (foundMovingItemzHierarchyRecordCount > 1)
            {
                throw new ApplicationException($"{movingItemzHierarchyRecordList.Count()} records found for the " +
                    $"moving Itemz Id {movingItemzId} in the system. " +
                    $"Expected 1 record but instead found {movingItemzHierarchyRecordList.Count()}");
            }

			// EXPLANATION :: We should check that targetId is not present as Node in
			// Child Tree Nodes below movingItemzId. Well targetId could be of type ItemzType
			// OR Itemz but any how it should not be a child node sitting under movingItemzId. 
            // Following code block is designed to check for the same. 

            if ((await CheckIfTargetIdIsUnderMovingItemzId(movingItemzId, targetId)))
            {
				throw new ApplicationException($"Moving Itemz with ID {movingItemzId.ToString()} is already " +
					$"parent of Target with ID {targetId} in the system. " +
					$"Application does not support moving Parent Itemz to one of it's Child Tree Node Itemz!");
			}

			// Variable Declarations
			var movingItemzHierarchyRecord = new ItemzHierarchy();
            string originalItemzHierarchyIdString = "";
            List<ItemzHierarchy> allDescendentItemzHierarchyRecord = new List<ItemzHierarchy>();

            if (foundMovingItemzHierarchyRecordCount == 1)
            {
                movingItemzHierarchyRecord = movingItemzHierarchyRecordList.FirstOrDefault();
                if(movingItemzHierarchyRecord!.ItemzHierarchyId!.GetLevel() < 3)
                {
                    throw new ApplicationException($"Expected {movingItemzHierarchyRecord.Id} " +
                        $"to be 'Itemz' but instead it's found in Itemz Hierarchy Record " +
                        $"as '{movingItemzHierarchyRecord.RecordType}' ");
                }

                originalItemzHierarchyIdString = movingItemzHierarchyRecord!.ItemzHierarchyId!.ToString();
                allDescendentItemzHierarchyRecord = await _context.ItemzHierarchy!
                    .Where(ih => ih.ItemzHierarchyId!.IsDescendantOf(movingItemzHierarchyRecord!.ItemzHierarchyId)).ToListAsync();

                //// TODO :: IN THE FUTURE WE SHOULD NOT CHECK FOR OLD ROOT ITEMZ AT ALL. THIS MIGHT
                //// NOT BE RELAVENT WHEN IT COMES TO ASSOCIATING ORPHAN ITEMZ.
                //// PERHAPS WE CAN USE THE SAME METHOD TO DO MOVE ITEMZ AS WELL AS ASSOCIATED ORPHAN ITEMZ

                //var oldRootHierarchyRecord = _context.ItemzHierarchy!.AsNoTracking()
                //        .Where(ih => ih.ItemzHierarchyId ==
                //                (movingItemzHierarchyRecord!.ItemzHierarchyId!.GetAncestor(1))
                //    );

                //if (oldRootHierarchyRecord.Count() != 1)
                //{
                //    // TODO: Following error can be improved by providing expected Vs actual found records.
                //    throw new ApplicationException("Either no old Root Itemz Type Hierarchy record " +
                //        "found OR multiple Root Itemz Type Hierarchy records found in the system");
                //}

                // TODO :: WE CAN INCLUDE CALL HERE TO REMOVE ITEMZTYPEJOINITEMZ RECORD HERE ITSELF.
                RemoveItemzTypeJoinItemzRecord(movingItemzId);
            }
            else
            {
                if (!(string.IsNullOrWhiteSpace(movingItemzName)))
                {
                    movingItemzHierarchyRecord = new Entities.ItemzHierarchy
                    {
                        Id = movingItemzId,
                        RecordType = "Itemz",
                        ItemzHierarchyId = null,
                        Name = movingItemzName
                    };
                }
                else
                {
                    movingItemzHierarchyRecord = new Entities.ItemzHierarchy
                    {
                        Id = movingItemzId,
                        RecordType = "Itemz",
                        ItemzHierarchyId = null
                    };
                    var _tempItemz = await _context.Itemzs!
                    .Where(c => c.Id == movingItemzId).AsNoTracking().FirstOrDefaultAsync();

                    if (_tempItemz != null)
                    {
                        movingItemzHierarchyRecord.Name = _tempItemz.Name;
                    }
                }
            }

            var newRootHierarchyRecord = _context.ItemzHierarchy!.AsNoTracking()
                            .Where(ih => ih.Id == targetId);
            if (newRootHierarchyRecord.Count() != 1)
            {
                throw new ApplicationException($"{newRootHierarchyRecord.Count()} records found for the " +
                    $"New Root Hierarchy Id {targetId} in the system. " +
                    $"Expected 1 record but instead found {newRootHierarchyRecord.Count()}");
            }

			var newRootHierarchyRecordLevel = newRootHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId!.GetLevel();
			if (newRootHierarchyRecordLevel < 2)
            {
                throw new ApplicationException($"New Root Hierarchy record has to be either 'Itemz Type' or 'Itemz'");
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

            if (newRootHierarchyRecordLevel == 2)
            {
                AddItemzTypeJoinItemzRecord(targetId, movingItemzId);
            }

            if (foundMovingItemzHierarchyRecordCount == 1)
            {
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
            }
            else
            {
                //_context.ItemzHierarchy.Add()
                _context.ItemzHierarchy!.Add(movingItemzHierarchyRecord);
            }
        }

        public async Task<bool> ItemzExistsAsync(Guid itemzId)
        {
            if (itemzId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(itemzId));
            }

            // EXPLANATION: We expect ItemzExists to be used independently on it's own without
            // expecting it to track the itemz that was found in the database. That's why it's not
            // a good idea to use "!(_context.Itemzs.Find(itemzId) == null)" option
            // to "Find()" Itemz. This is because Find is designed to track the itemz in the memory.
            // In "Itemz Delete controller method", we are first checking if ItemzExists and then 
            // we call Itemz Delete to actually remove it. This is going to be in the single scoped
            // DBContext. If we use "Find()" method then it will start tracking the itemz and then we can't
            // get the itemz once again from the DB as it's already being tracked. We have a choice here
            // to decide if we should always use Find via ItemzExists and then yet again in the subsequent
            // operations like Delete / Update or we use ItemzExists as independent method and not rely on 
            // it for subsequent operations like Delete / Update.

            return await _context.Itemzs.AsNoTracking().AnyAsync(a => a.Id == itemzId);
            // return  !(_context.Itemzs.Find(itemzId) == null);
        }

        public async Task<bool> ItemzTypeExistsAsync(Guid itemzTypeId)
        {
            if (itemzTypeId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(itemzTypeId));
            }

            // EXPLANATION: Using ".Any()" instead of ".Find" as explained in method
            // public bool ItemzExists(Guid itemzId)

            return await _context.ItemzTypes.AsNoTracking().AnyAsync(it => it.Id == itemzTypeId);
        }

        public async Task<bool> ItemzTypeItemzExistsAsync(ItemzTypeItemzDTO itemzTypeItemzDTO)
        {
            if (itemzTypeItemzDTO == null)
            {
                throw new ArgumentNullException(nameof(itemzTypeItemzDTO));
            }

            // EXPLANATION: Using ".Any()" instead of ".Find" as explained in method
            // public bool ItemzExists(Guid itemzId)

            var foundItemzNode = _context.ItemzHierarchy!.AsNoTracking()
                            .Where(ih => ih.Id == itemzTypeItemzDTO.ItemzId).FirstOrDefault();

            var foundItemzTypeNode = _context.ItemzHierarchy!.AsNoTracking()
                            .Where(ih => ih.Id == itemzTypeItemzDTO.ItemzTypeId).FirstOrDefault();

            if (foundItemzNode != null && foundItemzTypeNode != null)
            {
                if (foundItemzNode.ItemzHierarchyId!.GetAncestor(1) == foundItemzTypeNode.ItemzHierarchyId)
                {
                    return true;
                }
            }

            return false;   
        }

        public async Task<bool> IsOrphanedItemzAsync(Guid ItemzId)
        {
            if (ItemzId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(ItemzId));
            }

            // EXPLANATION: Using ".Any()" instead of ".Find" as explained in method
            // public bool ItemzExists(Guid itemzId)
            var isItemzFoundInItemzTypeJoinItemzAssociation = await _context.ItemzTypeJoinItemz.AsNoTracking()
                .AnyAsync(itji => itji.ItemzId == ItemzId);

            if (isItemzFoundInItemzTypeJoinItemzAssociation)
            {
                return false;
            }
            return true;

//            return await _context.ItemzTypeJoinItemz.AsNoTracking().AnyAsync(itji => itji.ItemzId == ItemzId);
        }

        public void UpdateItemz(Itemz itemz)
        {
            // Due to Repository Pattern implementation, 
            // there is no code in this implementation.  
        }

        public async Task DeleteItemzAsync(Guid itemzId)
        {
            var sqlParameters = new[]
{
                new SqlParameter
                {
                    ParameterName = "ItemzId",
                    Value = itemzId,
                    SqlDbType = System.Data.SqlDbType.UniqueIdentifier,
                }
            };

            // Instead of using Itemzs.Remove we are now using Stored
            // procedure because we need to perform some cleanup of 
            // "non-cascade delete" data due to Entity Framework
            // SQL Server limitations when it comes to many-to-many 
            // relationship. 
            // _context.Itemzs!.Remove(itemz);

            var OUTPUT_Success = new SqlParameter
            {
                ParameterName = "OUTPUT_Success",
                Direction = System.Data.ParameterDirection.Output,
                SqlDbType = System.Data.SqlDbType.Bit,
            };

            sqlParameters = sqlParameters.Append(OUTPUT_Success).ToArray();

            var _ = await _context.Database.ExecuteSqlRawAsync(sql: "EXEC userProcDeleteSingleItemzByItemzID @ItemzId, @OUTPUT_Success = @OUTPUT_Success OUT", parameters: sqlParameters);
        }

		public async Task DeleteAllOrphanItemz()
		{
            try
            {
                // TODO :: Verify that it's safe to execute SQL Statement like this and protect
                // data from SQL Injections.
                await _context.Database.ExecuteSqlRawAsync(SQLStatements.SQLStatementFor_DeleteAllOrphanedItemz);
            }
			catch (Exception ex)
			{
				throw new ApplicationException("Issue encountered while deleting All Orphan Itemz via SQL Statement!");
			}
		}

		public void RemoveItemzFromItemzType(ItemzTypeItemzDTO itemzTypeItemzDTO)
        {
            RemoveItemzTypeJoinItemzRecord(itemzTypeItemzDTO.ItemzId);

            RemoveItemzHierarchyRecordsIncludingDescendents(itemzTypeItemzDTO.ItemzId);
           
        }

        private void RemoveItemzTypeJoinItemzRecord(Guid itemzId)
        {
            var fount_itji = _context.ItemzTypeJoinItemz!.Where(itji => itji.ItemzId == itemzId);
            if (fount_itji.Any())
            {
                foreach (var itji in fount_itji)
                {
                    _context.ItemzTypeJoinItemz!.Remove(itji);
                }
            }
        }

        private void RemoveItemzHierarchyRecordsIncludingDescendents(Guid itemzId)
        {
            var found_ih = _context!.ItemzHierarchy!
                                   .AsNoTracking()
                                   .Where(ih => ih.Id == itemzId
                                           && ih.ItemzHierarchyId!.GetLevel() > 2);

            if (found_ih.Any())
            {

                var allDescendentfound_ih = _context.ItemzHierarchy!
                                .Where(ih => ih.ItemzHierarchyId!.IsDescendantOf(found_ih!.FirstOrDefault()!.ItemzHierarchyId));

                if (allDescendentfound_ih.Any())
                {
                    foreach (var descendent_ih in allDescendentfound_ih)
                    {
                        _context.ItemzHierarchy!.Remove(descendent_ih);
                    }
                }
            }
        }

        private void AddItemzTypeJoinItemzRecord(Guid itemzTypeId, Guid itemzId)
        {
            var itji = _context.ItemzTypeJoinItemz!.Find(itemzTypeId, itemzId);
            if (itji == null)
            {
                var temp_itji = new ItemzTypeJoinItemz
                {
                    ItemzId = itemzId,
                    ItemzTypeId = itemzTypeId
                };
                _context.ItemzTypeJoinItemz.Add(temp_itji);
            }
        }


		public async Task<bool> CheckIfTargetIdIsUnderMovingItemzId(Guid movingItemzId, Guid targetId)
		{
			var foundMovingItemzHierarchyRecord = await _context.ItemzHierarchy!.AsNoTracking()
				.Where(ih => ih.Id == movingItemzId)
				.Where(ih => ih.ItemzHierarchyId!.GetLevel() > 2) // Greater then ItemzType which is Level 2
				.FirstOrDefaultAsync();

			if (foundMovingItemzHierarchyRecord != null)
			{
				var foundTargetItemzHierarchyRecord = await _context.ItemzHierarchy!.AsNoTracking()
					.Where(ih => ih.Id == targetId)
					.Where(ih => ih.ItemzHierarchyId!.IsDescendantOf(foundMovingItemzHierarchyRecord!.ItemzHierarchyId))
					.ToListAsync();

				if (foundTargetItemzHierarchyRecord.Count > 0)
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

		public async Task<Guid> CopyItemzAsync(Guid ItemzId)
		{
			if (ItemzId == Guid.Empty)
			{
				throw new ArgumentNullException(nameof(ItemzId));
			}

			// EXPLANATION : Check if Itemz exist in Itemz Table

			if (!_context.Itemzs!.Where(i => i.Id == ItemzId).Any())
			{
				throw new ArgumentException(nameof(ItemzId));
			}

			// EXPLANATION : Check if Itemz exist in ItemzHierarchy Table.
            // If it does not exist here then it must be an Orphand Itemz
            // OR ItemzID is wrongly provided by the user.

			if ( !_context.ItemzHierarchy!.AsNoTracking()
							.Where(ih => ih.Id == ItemzId).Any())
			{
				throw new ArgumentException(nameof(ItemzId));
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
					Value = ItemzId,
					SqlDbType = System.Data.SqlDbType.UniqueIdentifier,
				}
			};

			sqlParameters = sqlParameters.Append(OUTPUT_ID).ToArray();

			var _ = await _context.Database.ExecuteSqlRawAsync(sql: "EXEC userProcCopyRecordWithChildrenByRecordID @RecordID, @OUTPUT_Id = @OUTPUT_Id OUT", parameters: sqlParameters);
			returnValue = (Guid)OUTPUT_ID.Value;

			return returnValue;
		}

		#region NOT USED ANYMORE CODE 

		///// <summary>
		///// Purpose of this method is to add new Itemz under parent ItemzID which is passed in as parameter
		///// It adds new Itemz at the end of the existing list of child Itemz under supplied parent ItemzId
		///// </summary>
		///// <param name="parentItemzId"></param>
		///// <param name="newlyAddedItemzId"></param>
		///// <param name="atBottomOfChildNodes"></param>
		///// <returns></returns>
		///// <exception cref="ArgumentNullException"></exception>
		///// <exception cref="ApplicationException"></exception>

		//public async Task AddNewItemzHierarchyAsync(Guid parentItemzId, Guid newlyAddedItemzId , bool atBottomOfChildNodes = true)
		//{
		//    if (parentItemzId == Guid.Empty )
		//    {
		//        throw new ArgumentNullException(nameof(parentItemzId));
		//    }

		//    if ( newlyAddedItemzId == Guid.Empty)
		//    {
		//        throw new ArgumentNullException(nameof(newlyAddedItemzId));
		//    }

		//    var rootItemz = _context.ItemzHierarchy!.AsNoTracking()
		//                    .Where(ih => ih.Id == parentItemzId);

		//    if (rootItemz.Count() != 1)
		//    {
		//        throw new ApplicationException("Either no Parent Itemz Hierarchy record was " +
		//            "found OR multiple Parent Itemz Hierarchy records were found in the system");
		//    }

		//    // EXPLANATION : We are using SQL Server HierarchyID field type. Now we can use EF Core special
		//    // methods to query for all Decendents as per below. We are actually finding all Decendents by saying
		//    // First find the ItemzHierarchy record where ID matches Parent Itemz ID. This is expected to be the
		//    // Parent Itemz ID itself which is the root OR parent to newly added Itemz.
		//    // Then we find all desendents of Parent Itemz which is nothing but existing Itemz(s). 

		//    var parentItemzHierarchyChildRecords = await _context.ItemzHierarchy!
		//            .AsNoTracking()
		//            .Where(ih => ih.ItemzHierarchyId!.GetAncestor(1) == rootItemz.FirstOrDefault()!.ItemzHierarchyId!)
		//            .OrderBy(ih => ih.ItemzHierarchyId!)
		//            .ToListAsync();


		//    //var tempItemzHierarchy = new Entities.ItemzHierarchy
		//    //{
		//    //    Id = newlyAddedItemzId,
		//    //    RecordType = "Itemz",
		//    //    ItemzHierarchyId = rootItemz.FirstOrDefault()!.ItemzHierarchyId!
		//    //                        .GetDescendant(parentItemzHierarchyChildRecords.Count() > 0
		//    //                                            ? parentItemzHierarchyChildRecords.LastOrDefault()!.ItemzHierarchyId
		//    //                                            : null
		//    //                                       , null),
		//    //};
		//    //_context.ItemzHierarchy!.Add(tempItemzHierarchy);

		//    if (parentItemzHierarchyChildRecords.Count() > 0)
		//    {
		//        if (atBottomOfChildNodes)
		//        {
		//            var tempItemzHierarchy = new Entities.ItemzHierarchy
		//            {
		//                Id = newlyAddedItemzId,
		//                RecordType = "Itemz",
		//                ItemzHierarchyId = rootItemz.FirstOrDefault()!.ItemzHierarchyId!
		//                                .GetDescendant(parentItemzHierarchyChildRecords.LastOrDefault()!.ItemzHierarchyId
		//                                               , null),
		//            };

		//            _context.ItemzHierarchy!.Add(tempItemzHierarchy);
		//        }
		//        else
		//        {
		//            var tempItemzHierarchy = new Entities.ItemzHierarchy
		//            {
		//                Id = newlyAddedItemzId,
		//                RecordType = "Itemz",
		//                ItemzHierarchyId = rootItemz.FirstOrDefault()!.ItemzHierarchyId!
		//                                .GetDescendant(null
		//                                                , parentItemzHierarchyChildRecords.FirstOrDefault()!.ItemzHierarchyId
		//                                               ),
		//            };

		//            _context.ItemzHierarchy!.Add(tempItemzHierarchy);
		//        }
		//    }
		//    else
		//    {
		//        var tempItemzHierarchy = new Entities.ItemzHierarchy
		//        {
		//            Id = newlyAddedItemzId,
		//            RecordType = "Itemz",
		//            ItemzHierarchyId = rootItemz.FirstOrDefault()!.ItemzHierarchyId!
		//                            .GetDescendant(null, null),
		//        };

		//        _context.ItemzHierarchy!.Add(tempItemzHierarchy);
		//    }
		//}

		//private string? localHelperGetMeNextHierarchyIDNumber(string lowerBoundHierarchyId)
		//{
		//    var lastSlashPosition = lowerBoundHierarchyId.LastIndexOf("/");
		//    var convertedlowerBoundHierarchyId = lowerBoundHierarchyId.Remove(lastSlashPosition, 1).Insert(lastSlashPosition, ".2/");
		//    return convertedlowerBoundHierarchyId;
		//}

		//public async Task AddNewItemzHierarchyByItemzTypeIdAsync(Guid itemzId, Guid itemzTypeId, bool atBottomOfChildNodes = true)
		//{
		//    if (itemzId == Guid.Empty)
		//    {
		//        throw new ArgumentNullException(nameof(itemzId));
		//    }

		//    if (itemzTypeId == Guid.Empty)
		//    {
		//        throw new ArgumentNullException(nameof(itemzTypeId));
		//    }

		//    var rootItemzTypeHierarchyRecord = _context.ItemzHierarchy!.AsNoTracking()
		//                    .Where(ih => ih.Id == itemzTypeId);

		//    if (rootItemzTypeHierarchyRecord.Count() != 1)
		//    {
		//        // TODO: Following error can be improved by providing expected Vs actual found records.
		//        throw new ApplicationException("Either no Root Itemz Type Hierarchy record " +
		//            "found OR multiple Root Itemz Type Hierarchy records found in the system");
		//    }

		//    var rootItemzTypeHierarchyRecordLevel = rootItemzTypeHierarchyRecord!.FirstOrDefault()!.ItemzHierarchyId!.GetLevel();

		//    if (rootItemzTypeHierarchyRecordLevel != 2)
		//    {
		//        throw new ApplicationException($"Found root hierarchy record for ID {itemzTypeId} " +
		//            $"does not represent ItemzType. Instead it's " +
		//            $"{rootItemzTypeHierarchyRecord.FirstOrDefault()!.RecordType}");
		//    }

		//    // EXPLANATION : We are using SQL Server HierarchyID field type. Now we can use EF Core special
		//    // methods to query for all Decendents as per below. We are actually finding all Decendents by saying
		//    // First find the ItemzHierarchy record where ID matches RootItemzType ID. This is expected to be the
		//    // ItemzType ID itself which is the root OR parent to newly added Itemz.
		//    // Then we find all desendents of Repository which is nothing but existing Itemz(s). 

		//    var itemzHierarchyRecords = await _context.ItemzHierarchy!
		//            .AsNoTracking()
		//            .Where(ih => ih.ItemzHierarchyId!.GetAncestor(1) == rootItemzTypeHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId!)
		//            .OrderBy(ih => ih.ItemzHierarchyId!)
		//            .ToListAsync();

		//    if (itemzHierarchyRecords.Count() > 0)
		//    {
		//        if (atBottomOfChildNodes)
		//        {
		//            var tempItemzHierarchy = new Entities.ItemzHierarchy
		//            {
		//                Id = itemzId,
		//                RecordType = "Itemz",
		//                ItemzHierarchyId = rootItemzTypeHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId!
		//                                .GetDescendant(itemzHierarchyRecords.LastOrDefault()!.ItemzHierarchyId
		//                                               , null),
		//            };

		//            _context.ItemzHierarchy!.Add(tempItemzHierarchy);
		//        }
		//        else
		//        {
		//            var tempItemzHierarchy = new Entities.ItemzHierarchy
		//            {
		//                Id = itemzId,
		//                RecordType = "Itemz",
		//                ItemzHierarchyId = rootItemzTypeHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId!
		//                                .GetDescendant(null
		//                                                , itemzHierarchyRecords.FirstOrDefault()!.ItemzHierarchyId
		//                                               ),
		//            };

		//            _context.ItemzHierarchy!.Add(tempItemzHierarchy);
		//        }
		//    }
		//    else
		//    {
		//        var tempItemzHierarchy = new Entities.ItemzHierarchy
		//        {
		//            Id = itemzId,
		//            RecordType = "Itemz",
		//            ItemzHierarchyId = rootItemzTypeHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId!
		//                            .GetDescendant(null, null),
		//        };

		//        _context.ItemzHierarchy!.Add(tempItemzHierarchy);
		//    }

		//    if (rootItemzTypeHierarchyRecordLevel == 2)
		//    {
		//        AddItemzTypeJoinItemzRecord(itemzTypeId, itemzId);
		//    }
		//}

		//public async Task MoveItemzHierarchyByItemzTypeIdAsync(Guid itemzId, Guid itemzTypeId, bool atBottomOfChildNodes = true)
		//{
		//    if (itemzId == Guid.Empty)
		//    {
		//        throw new ArgumentNullException(nameof(itemzId));
		//    }

		//    if (itemzTypeId == Guid.Empty)
		//    {
		//        throw new ArgumentNullException(nameof(itemzTypeId));
		//    }

		//    var itemzHierarchyRecordList = _context.ItemzHierarchy!
		//        .Where(ih => ih.Id == itemzId);

		//    if (itemzHierarchyRecordList.Count() != 1)
		//    {
		//        // TODO: Following error can be improved by providing expected Vs actual found records.
		//        throw new ApplicationException("Either no Root Itemz Type Hierarchy record " +
		//            "found OR multiple Root Itemz Type Hierarchy records found in the system");
		//    }

		//    var itemzHierarchyRecord = itemzHierarchyRecordList.FirstOrDefault();
		//    var originalItemzHierarchyIdString = itemzHierarchyRecord!.ItemzHierarchyId!.ToString();
		//    var allDescendentItemzHierarchyRecord = await _context.ItemzHierarchy!
		//        .Where(ih => ih.ItemzHierarchyId!.IsDescendantOf(itemzHierarchyRecord!.ItemzHierarchyId)).ToListAsync();

		//    var oldRootItemzTypeHierarchyRecord = _context.ItemzHierarchy!.AsNoTracking()
		//            .Where(ih => ih.ItemzHierarchyId ==
		//                    (itemzHierarchyRecord!.ItemzHierarchyId!.GetAncestor(1))
		//        );

		//    if (oldRootItemzTypeHierarchyRecord.Count() != 1)
		//    {
		//        // TODO: Following error can be improved by providing expected Vs actual found records.
		//        throw new ApplicationException("Either no old Root Itemz Type Hierarchy record " +
		//            "found OR multiple Root Itemz Type Hierarchy records found in the system");
		//    }

		//    var newRootItemzTypeHierarchyRecord = _context.ItemzHierarchy!.AsNoTracking()
		//                    .Where(ih => ih.Id == itemzTypeId);

		//    if (newRootItemzTypeHierarchyRecord.Count() != 1)
		//    {
		//        // TODO: Following error can be improved by providing expected Vs actual found records.
		//        throw new ApplicationException("Either no Root Itemz Type Hierarchy record " +
		//            "found OR multiple Root Itemz Type Hierarchy records found in the system");
		//    }

		//    if( newRootItemzTypeHierarchyRecord.FirstOrDefault()!.RecordType != "ItemzType")
		//    {
		//        throw new ApplicationException($"New Root Hierarchy record is not of type 'ItemzType'");
		//    }

		//    // EXPLANATION : We are using SQL Server HierarchyID field type. Now we can use EF Core special
		//    // methods to query for all Decendents as per below. We are actually finding all Decendents by saying
		//    // First find the ItemzHierarchy record where ID matches RootItemzType ID. This is expected to be the
		//    // ItemzType ID itself which is the root OR parent to newly added Itemz.
		//    // Then we find all desendents of Repository which is nothing but existing Itemz(s). 

		//    var childItemzHierarchyRecords = await _context.ItemzHierarchy!
		//            .AsNoTracking()
		//            .Where(ih => ih.ItemzHierarchyId!.GetAncestor(1) == newRootItemzTypeHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId!)
		//            .OrderBy(ih => ih.ItemzHierarchyId!)
		//            .ToListAsync();

		//    //itemzHierarchyRecord!.ItemzHierarchyId = itemzHierarchyRecord.ItemzHierarchyId!
		//    //        .GetReparentedValue(oldRootItemzTypeHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId
		//    //        , newRootItemzTypeHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId
		//    //         );

		//    if (childItemzHierarchyRecords.Count() == 0)
		//    {
		//        itemzHierarchyRecord!.ItemzHierarchyId = newRootItemzTypeHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId!
		//            .GetDescendant(null, null);
		//    }
		//    else
		//    {
		//        if (atBottomOfChildNodes)
		//        {
		//            itemzHierarchyRecord!.ItemzHierarchyId = HierarchyId.Parse(
		//                HierarchyIdStringHelper.ManuallyGenerateHierarchyIdNumberString(
		//                    childItemzHierarchyRecords.LastOrDefault()!.ItemzHierarchyId!.ToString()
		//                    , diffValue: 1
		//                    , addDecimal: false)
		//                );
		//        }
		//        else
		//        {
		//            itemzHierarchyRecord!.ItemzHierarchyId = HierarchyId.Parse(
		//                HierarchyIdStringHelper.ManuallyGenerateHierarchyIdNumberString(
		//                    childItemzHierarchyRecords.FirstOrDefault()!.ItemzHierarchyId!.ToString()
		//                    , diffValue: -1
		//                    , addDecimal: false)
		//                );
		//        }
		//    }
		//    var newItemzHierarchyIdString = itemzHierarchyRecord!.ItemzHierarchyId!.ToString();
		//    // TODO :: I THINK I KNOW HOW TO DO ALL DESCENDENTS MOVE 
		//    // 1. NOTE ORIGINAL HIERARCHY ID OF THE MAIN ITEMZ WHICH IS MOVING
		//    // 2. MOVE THE FIRST ITEMZ TO THE NEW LOCATION BY GENERATING TOP OR BOTTOM NUMBER
		//    // 3. NOTE NEW HIERARCHY ID OF THE MAIN ITEM THAT WE JUST MOVED
		//    // 4. PERFORM STRING REPLACE AT THE BIGGINING OF THE STRING FOR EACH CHILD NODE FROM 
		//    //    ORIGINAL HIERARCHY ID NUMBER TO NEW HIERARCHY ID NUMBER OF THE MAIN ITEMZ
		//    // 5. PERFORM SAVE ALL.

		//    // Console.WriteLine($"Found {allDescendentItemzHierarchyRecord.Count()} allDescendentItemzHierarchyRecord");

		//    foreach (var descendentItemzHierarchyRecord in allDescendentItemzHierarchyRecord)
		//    {
		//        // Console.WriteLine($"Found descendent Itemz {descendentItemzHierarchyRecord.ItemzHierarchyId.ToString()}");

		//        Regex oldValueRegEx = new Regex(originalItemzHierarchyIdString);
		//        descendentItemzHierarchyRecord.ItemzHierarchyId = HierarchyId.Parse(
		//            (oldValueRegEx.Replace( (descendentItemzHierarchyRecord!
		//                                    .ItemzHierarchyId!.ToString())
		//                                    , newItemzHierarchyIdString
		//                                    , 1)
		//            ) 
		//        );
		//    }
		//}


		//public void AssociateItemzToItemzType(ItemzTypeItemzDTO itemzTypeItemzDTO, bool atBottomOfChildNodes = true)
		//{
		//    //var itji = _context.ItemzTypeJoinItemz!.Find(itemzTypeItemzDTO.ItemzTypeId, itemzTypeItemzDTO.ItemzId);
		//    //if (itji == null)
		//    //{
		//    //    var temp_itji = new ItemzTypeJoinItemz
		//    //    {
		//    //        ItemzId = itemzTypeItemzDTO.ItemzId,
		//    //        ItemzTypeId = itemzTypeItemzDTO.ItemzTypeId
		//    //    };
		//    //    _context.ItemzTypeJoinItemz.Add(temp_itji);
		//    //}

		//    // AddItemzTypeJoinItemzRecord(itemzTypeItemzDTO.ItemzTypeId, itemzTypeItemzDTO.ItemzId);

		//    ////var foundItemzHierarchy = _context.ItemzHierarchy!.AsNoTracking()
		//    ////    .Where(ih => ih.Id == itemzTypeItemzDTO.ItemzId);

		//    ////if (foundItemzHierarchy.Count() == 0)
		//    ////{
		//    ////    AddNewItemzHierarchyByItemzTypeIdAsync(itemzTypeItemzDTO.ItemzId,
		//    ////                                            itemzTypeItemzDTO.ItemzTypeId,
		//    ////                                            atBottomOfChildNodes).Wait();
		//    ////}
		//    ////else
		//    ////{
		//        MoveItemzHierarchyAsync(itemzTypeItemzDTO.ItemzId,
		//                                                itemzTypeItemzDTO.ItemzTypeId,
		//                                                atBottomOfChildNodes).Wait(); 
		//    ////}
		//}

		//public void MoveItemzFromOneItemzTypeToAnother(ItemzTypeItemzDTO sourceItemzTypeItemzDTO, ItemzTypeItemzDTO targetItemzTypeItemzDTO, bool atBottomOfChildNodes = true)
		//{
		//    // EXPLANATION: Fow now best thing to do would be to remove unwanted itemz and itemzType association 
		//    // and then find target  association and if not found then simply add it. 
		//    // This method should be used for moving one itemz at a time. If one would like to move
		//    // multiple items (i.e. 100s of them in bulk) then this method of updating one record at a time
		//    // is not very efficient. We will have to come-up with alternative option for 
		//    // Bulk updating multiple itemz and itemzType association. 


		//    // TODO :: NOW WE DON'T NEED TO EXPLICITELY CALL BELOW RemoveItemzTypeJoinItemzRecord 
		//    // RemoveItemzTypeJoinItemzRecord(sourceItemzTypeItemzDTO.ItemzId);

		//    //// EXPLANATION: Previously we were using following function but then we 
		//    /// stopped using it because we do not want to remove ItemzHierarchy records
		//    /// while we are suppose to query it and move it to another location. So now we 
		//    /// perform remove of ItemzTypeJoinItemz association manually above as part of 
		//    /// logic implementation for this specific method MoveItemzFromOneItemzTypeToAnother. 

		//    // RemoveItemzFromItemzType(sourceItemzTypeItemzDTO);

		//    // AssociateItemzToItemzType(targetItemzTypeItemzDTO, atBottomOfChildNodes);
		//    MoveItemzHierarchyAsync(targetItemzTypeItemzDTO.ItemzId,
		//                                targetItemzTypeItemzDTO.ItemzTypeId,
		//                                atBottomOfChildNodes).Wait();
		//}


		#endregion NOT IN USE


	}
}
