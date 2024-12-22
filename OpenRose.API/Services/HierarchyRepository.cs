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
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ItemzApp.API.Models.BetweenControllerAndRepository;
using Microsoft.SqlServer.Types;

namespace ItemzApp.API.Services
{
    public class HierarchyRepository : IHierarchyRepository, IDisposable
    {
        private readonly ItemzContext _context;

        public HierarchyRepository(ItemzContext context)
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

		//public async Task<HierarchyIdRecordDetailsDTO> GetNextSiblingHierarchyRecordDetailsByID(Guid recordId)
		//{
		//	if (recordId == Guid.Empty)
		//	{
		//		throw new ArgumentNullException(nameof(recordId));
		//	}

		//	var foundFirstHierarchyRecord = _context.ItemzHierarchy!.AsNoTracking()
		//		.Where(ih => ih.Id == recordId);

		//	if (foundFirstHierarchyRecord.Count() != 1)
		//	{
		//		throw new ApplicationException($"Expected 1 Hierarchy record to be found " +
		//			$"but instead found {foundFirstHierarchyRecord.Count()} records for ID {recordId}" +
		//			"Please contact your System Administrator.");
		//	}

		//	var foundSecondHierarchyRecord =

		//}



		//// GENERATED CODE STARTS
		public async Task<HierarchyIdRecordDetailsDTO?> GetNextSiblingHierarchyRecordDetailsByID(Guid recordId)
		{
			if (recordId == Guid.Empty)
			{
				throw new ArgumentNullException(nameof(recordId));
			}

			var foundFirstHierarchyRecord = _context.ItemzHierarchy!.AsNoTracking()
				.Where(ih => ih.Id == recordId)
				.FirstOrDefault();

			if (foundFirstHierarchyRecord == null)
			{
				throw new ApplicationException($"Expected 1 Hierarchy record to be found " +
					$"but instead found 0 records for ID {recordId}. " +
					"Please contact your System Administrator.");
			}

			//if (foundFirstHierarchyRecord.RecordType.ToLower() != "itemz") // TODO :: USE CONSTANTS
			//{
			//	throw new ApplicationException($"Provided ID should be for RecordType Itemz and not for {foundFirstHierarchyRecord.RecordType}");
			//}

			// EXPLANATION :: Get Immediate parent of the current record and it's RecordType does not matter.
			var parentHierarchyId = foundFirstHierarchyRecord.ItemzHierarchyId!.GetAncestor(1);

			if (parentHierarchyId == null)
			{
				return null;
			}

			// Get all child nodes of the parent
			var siblingNodes = await _context.ItemzHierarchy!
				.AsNoTracking()
				.Where(ih => ih.ItemzHierarchyId!.GetAncestor(1) == parentHierarchyId)
				.OrderBy(ih => ih.ItemzHierarchyId!)
				.ToListAsync();

			// Find the index of the current record and get the next sibling
			var currentIndex = siblingNodes.FindIndex(ih => ih.Id == recordId);

			if (currentIndex == -1 || currentIndex + 1 >= siblingNodes.Count)
			{
				// No next sibling found
				return null;
			}

			var nextSibling = siblingNodes[currentIndex + 1];

			//var nextSiblingDetails = new HierarchyIdRecordDetailsDTO
			//{
			//	RecordId = nextSibling.Id,
			//	Name = nextSibling.Name ?? "",
			//	HierarchyId = nextSibling.ItemzHierarchyId!.ToString(),
			//	RecordType = nextSibling.RecordType,
			//	Level = nextSibling.ItemzHierarchyId.GetLevel(),
			//	ParentRecordId = foundFirstHierarchyRecord.Id,
			//	ParentRecordType = foundFirstHierarchyRecord.RecordType,
			//	ParentHierarchyId = foundFirstHierarchyRecord.ItemzHierarchyId.ToString(),
			//	ParentLevel = foundFirstHierarchyRecord.ItemzHierarchyId.GetLevel(),
			//	ParentName = foundFirstHierarchyRecord.Name ?? ""
			//};

			var nextSiblingDetails = await GetHierarchyRecordDetailsByID(nextSibling.Id);

			return nextSiblingDetails;
		}
		//// GENERATED CODE ENDS





		public async Task<HierarchyIdRecordDetailsDTO?> GetHierarchyRecordDetailsByID(Guid recordId)
        {
            if (recordId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(recordId));
            }

            var foundHierarchyRecord = _context.ItemzHierarchy!.AsNoTracking()
                            .Where(ih => ih.Id == recordId);

            if (foundHierarchyRecord.Count() != 1)
            {
                throw new ApplicationException($"Expected 1 Hierarchy record to be found " +
                    $"but instead found {foundHierarchyRecord.Count()} records for ID {recordId}" +
                    "Please contact your System Administrator.");
            }
			var hierarchyIdRecordDetails = new HierarchyIdRecordDetailsDTO();
            hierarchyIdRecordDetails.RecordId = recordId;
            hierarchyIdRecordDetails.Name = foundHierarchyRecord.FirstOrDefault()!.Name ?? "";
			hierarchyIdRecordDetails.HierarchyId = foundHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId!.ToString();
            hierarchyIdRecordDetails.RecordType = foundHierarchyRecord.FirstOrDefault()!.RecordType;
            hierarchyIdRecordDetails.Level = foundHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId!.GetLevel();

            // EXPLANATION : We are using SQL Server HierarchyID field type. Now we can use EF Core special
            // methods to query for all Decendents as per below. We are actually finding all Decendents by saying
            // First find the ItemzHierarchy record where ID matches RootItemz ID. This is expected to be the
            // repository ID itself which is the root. then we find all desendents of Repository which is nothing but Project(s). 

            var itemzTypeHierarchyItemz = await _context.ItemzHierarchy!
                    .AsNoTracking()
                    .Where(ih => ih.ItemzHierarchyId!.GetAncestor(1) == foundHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId!)
                    .OrderBy(ih => ih.ItemzHierarchyId!)
                    .ToListAsync();

            if (itemzTypeHierarchyItemz.Count != 0)
            {
                hierarchyIdRecordDetails.TopChildHierarchyId = itemzTypeHierarchyItemz.FirstOrDefault()!.ItemzHierarchyId!.ToString();
                hierarchyIdRecordDetails.BottomChildHierarchyId = itemzTypeHierarchyItemz.LastOrDefault()!.ItemzHierarchyId!.ToString();
                hierarchyIdRecordDetails.NumberOfChildNodes = itemzTypeHierarchyItemz.Count;

            }

			// EXPLANATION : Here we are getting record where Hierarchy ID is equal to the Hierarchy Id of immediate parent. 
            // We find immediate parent by using GetAncestor(1) method on found hierarchy record.

			var parentHierarchyRecord = _context.ItemzHierarchy!
				.AsNoTracking()
				.Where(ih => ih.ItemzHierarchyId == foundHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId!.GetAncestor(1))
				.FirstOrDefault();

            if (parentHierarchyRecord != null)
            {
				hierarchyIdRecordDetails.ParentRecordId = parentHierarchyRecord.Id;
				hierarchyIdRecordDetails.ParentRecordType = parentHierarchyRecord.RecordType;
				hierarchyIdRecordDetails.ParentHierarchyId = parentHierarchyRecord.ItemzHierarchyId!.ToString();
                hierarchyIdRecordDetails.ParentLevel = parentHierarchyRecord.ItemzHierarchyId!.GetLevel();
                hierarchyIdRecordDetails.ParentName = parentHierarchyRecord.Name ?? "";
			}
			return hierarchyIdRecordDetails;
           
        }


		public async Task<IEnumerable<HierarchyIdRecordDetailsDTO?>> GetImmediateChildrenOfItemzHierarchy(Guid recordId)
        {
			if (recordId == Guid.Empty)
			{
				throw new ArgumentNullException(nameof(recordId));
			}

			var foundHierarchyRecord =  _context.ItemzHierarchy!.AsNoTracking()
							.Where(ih => ih.Id == recordId);

			if (foundHierarchyRecord.Count() != 1)
			{
				throw new ApplicationException($"Expected 1 Hierarchy record to be found " +
					$"but instead found {foundHierarchyRecord.Count()} records for ID {recordId}" +
					"Please contact your System Administrator.");
			}

            var foundHierarchyRecordLevel = foundHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId.GetLevel();

			// EXPLANATION : We are using SQL Server HierarchyID field type. Now we can use EF Core special
			// methods to query for all Decendents as per below. By adding clause to check for GetLevel which is less then
			// CurrentHierarchyRecordLevel + three, we get Hierarchy record itself plus two more deeper level of hierarchy records.
			// The 1st Level data of the record itself is ignored and then 2nd level data is the actual child records.
			// While third level data are used for calculating number of children for child records.

			var itemzTypeHierarchyItemzs = await _context.ItemzHierarchy!
					.AsNoTracking()
					.Where(ih => ih.ItemzHierarchyId!.IsDescendantOf(foundHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId!) 
						&& ih.ItemzHierarchyId.GetLevel() < (foundHierarchyRecordLevel+3))
					.OrderBy(ih => ih.ItemzHierarchyId!)
					.ToListAsync();

			List<HierarchyIdRecordDetailsDTO> returningRecords = [];
			HierarchyIdRecordDetailsDTO hierarchyIdRecordDetails = new();
			string? _localTopChildHierarchyId = null ;
			int _localNumerOfChildNodes = 0;

			if (itemzTypeHierarchyItemzs.Count() < 2) // Less then 2 because we may have project entry but no ItemzType below it.
			{
				return default;
			}

			for (var i = 0; i< itemzTypeHierarchyItemzs.Count(); i++ )
			{ 
				if (itemzTypeHierarchyItemzs[i].ItemzHierarchyId!.GetLevel() == (foundHierarchyRecordLevel + 1))
				{
					if (i == 1) // Because i = 0 is the hierarchy record for passed in recordId parameter itself. So we check for i == 1 as first child record.
					{
						hierarchyIdRecordDetails.RecordId = itemzTypeHierarchyItemzs[i].Id;
						hierarchyIdRecordDetails.Name = itemzTypeHierarchyItemzs[i].Name ?? "";
						hierarchyIdRecordDetails.HierarchyId = itemzTypeHierarchyItemzs[i].ItemzHierarchyId!.ToString();
						hierarchyIdRecordDetails.RecordType = itemzTypeHierarchyItemzs[i].RecordType;
						hierarchyIdRecordDetails.Level = itemzTypeHierarchyItemzs[i].ItemzHierarchyId!.GetLevel();

						// EXPLANATION :: Now add Parent Details which is nothing but foundHierarchyRecord
						hierarchyIdRecordDetails.ParentRecordId = foundHierarchyRecord.FirstOrDefault()!.Id;
						hierarchyIdRecordDetails.ParentRecordType = foundHierarchyRecord.FirstOrDefault()!.RecordType;
						hierarchyIdRecordDetails.ParentHierarchyId = foundHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId!.ToString();
						hierarchyIdRecordDetails.ParentLevel = foundHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId!.GetLevel();
						hierarchyIdRecordDetails.ParentName = foundHierarchyRecord.FirstOrDefault()!.Name ?? "";
					}
					else
					{
						// WE HAVE TO FINISH WORKING ON PREVIOUS RECORD AND START PROCESSING NEXT ONE
						// IF NUMBER OF CHILD RECORDS ARE GREATER THEN ZERO i.e. ANY CHILD RECORDS FOUND THEN 
						// We have to capture BottomChildHierarchyId and NumberOfChildNodes values.
						if (_localNumerOfChildNodes > 0)
						{
							// hierarchyIdRecordDetails.TopChildHierarchyId = _localTopChildHierarchyId;
							hierarchyIdRecordDetails.BottomChildHierarchyId = itemzTypeHierarchyItemzs[i - 1].ItemzHierarchyId!.ToString();
							hierarchyIdRecordDetails.NumberOfChildNodes = _localNumerOfChildNodes;
							_localNumerOfChildNodes = 0; // RESET 
						}

						returningRecords.Add(hierarchyIdRecordDetails);

						// RESET hierarchyIdRecordDetails AND START CAPTURING DETAILS OF THE NEXT CHILD RECORD

						hierarchyIdRecordDetails = new();
						hierarchyIdRecordDetails.RecordId = itemzTypeHierarchyItemzs[i].Id;
						hierarchyIdRecordDetails.Name = itemzTypeHierarchyItemzs[i].Name ?? "";
						hierarchyIdRecordDetails.HierarchyId = itemzTypeHierarchyItemzs[i].ItemzHierarchyId!.ToString();
						hierarchyIdRecordDetails.RecordType = itemzTypeHierarchyItemzs[i].RecordType;
						hierarchyIdRecordDetails.Level = itemzTypeHierarchyItemzs[i].ItemzHierarchyId!.GetLevel();

						// EXPLANATION :: Now add Parent Details which is nothing but foundHierarchyRecord
						hierarchyIdRecordDetails.ParentRecordId = foundHierarchyRecord.FirstOrDefault()!.Id;
						hierarchyIdRecordDetails.ParentRecordType = foundHierarchyRecord.FirstOrDefault()!.RecordType;
						hierarchyIdRecordDetails.ParentHierarchyId = foundHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId!.ToString();
						hierarchyIdRecordDetails.ParentLevel = foundHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId!.GetLevel();
						hierarchyIdRecordDetails.ParentName = foundHierarchyRecord.FirstOrDefault()!.Name ?? "";
					}

				}
				else if (itemzTypeHierarchyItemzs[i].ItemzHierarchyId.GetLevel() == (foundHierarchyRecordLevel + 2))
				{
					if (_localNumerOfChildNodes == 0)
					{
						hierarchyIdRecordDetails.TopChildHierarchyId = itemzTypeHierarchyItemzs[i].ItemzHierarchyId!.ToString();
					}
					_localNumerOfChildNodes = _localNumerOfChildNodes + 1;
				}
			}

			// ADD FINAL hierarchyIdRecordDetails TO THE COLLECTION.
			if (_localNumerOfChildNodes > 0)
			{
				hierarchyIdRecordDetails.BottomChildHierarchyId = itemzTypeHierarchyItemzs[(itemzTypeHierarchyItemzs.Count()-1)].ItemzHierarchyId!.ToString();
				hierarchyIdRecordDetails.NumberOfChildNodes = _localNumerOfChildNodes;
			}
			returningRecords.Add(hierarchyIdRecordDetails);

			return returningRecords;
		}


		public async Task<RecordCountAndEnumerable<NestedHierarchyIdRecordDetailsDTO>> GetAllParentsOfItemzHierarchy(Guid recordId)
		{
			if (recordId == Guid.Empty)
			{
				throw new ArgumentNullException(nameof(recordId));
			}

			var foundHierarchyRecord = _context.ItemzHierarchy!.AsNoTracking()
							.Where(ih => ih.Id == recordId);

			if (foundHierarchyRecord.Count() == 0)
			{
				return new();
			}

			if (foundHierarchyRecord.Count() > 1)
			{
				throw new ApplicationException($"Expected 1 Hierarchy record to be found " +
					$"but instead found {foundHierarchyRecord.Count()} records for ID {recordId}" +
					"Please contact your System Administrator.");
			}

			var foundHierarchyRecordLevel = foundHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId!.GetLevel();
			int rootRepositoryLevel = 0;

			// EXPLANATION : We are using SQL Server HierarchyID field type. Now we can use EF Core special
			// methods to query for all Decendents as per below. 

			RecordCountAndEnumerable<NestedHierarchyIdRecordDetailsDTO> recordCountAndEnumerable = new RecordCountAndEnumerable<NestedHierarchyIdRecordDetailsDTO>();

			var allHierarchyItemzs = await _context.ItemzHierarchy!
					.AsNoTracking()
					.Where(ih => foundHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId!.IsDescendantOf(ih.ItemzHierarchyId!))
					.OrderBy(ih => ih.ItemzHierarchyId!)
					.ToListAsync();

			List<NestedHierarchyIdRecordDetailsDTO> returningRecords = [];


			if (allHierarchyItemzs.Count() > 2) // We check more then 2 because 1st record is repository and last record is for recordId itself.
			{
				recordCountAndEnumerable.RecordCount = (allHierarchyItemzs.Count() - 2);
			}
			else
			{
				recordCountAndEnumerable.RecordCount = 0;
				recordCountAndEnumerable.AllRecords = new List<NestedHierarchyIdRecordDetailsDTO>();
			}

			for (var i = 0; i < allHierarchyItemzs.Count(); i++)
			{
				if (i == rootRepositoryLevel) continue; // Skip first record as it's for the repository record
				if (i == (allHierarchyItemzs.Count() - 1)) continue; // Skip last record as it's for the supplied recordId

				if (allHierarchyItemzs[i].ItemzHierarchyId!.GetLevel() == (rootRepositoryLevel + 1))
				{
					returningRecords.Add(new NestedHierarchyIdRecordDetailsDTO
					{
						RecordId = allHierarchyItemzs[i].Id,
						HierarchyId = allHierarchyItemzs[i].ItemzHierarchyId!.ToString(),
						Level = allHierarchyItemzs[i].ItemzHierarchyId!.GetLevel(),
						RecordType = allHierarchyItemzs[i].RecordType,
						Name = allHierarchyItemzs[i].Name ?? "",
						Children = new List<NestedHierarchyIdRecordDetailsDTO>()
					});
				}
				else if (allHierarchyItemzs[i].ItemzHierarchyId!.GetLevel() > (rootRepositoryLevel + 1))
				{
					// Find the last record at a specified level directly within returningRecords
					//var targetLevel = (allHierarchyItemzs[i].ItemzHierarchyId!.GetLevel() - 1);
					//var lastRecordAtLevel = FindLastRecordAtLevel(returningRecords, targetLevel);
					var lastRecordAtLevel = FindParentRecord(returningRecords, allHierarchyItemzs[i]);


					if (lastRecordAtLevel != null)
					{
						// Add a child to the last record at the specified level
						lastRecordAtLevel.Children.Add(new NestedHierarchyIdRecordDetailsDTO
						{
							RecordId = allHierarchyItemzs[i].Id,
							HierarchyId = allHierarchyItemzs[i].ItemzHierarchyId!.ToString(),
							Level = allHierarchyItemzs[i].ItemzHierarchyId!.GetLevel(),
							RecordType = allHierarchyItemzs[i].RecordType,
							Name = allHierarchyItemzs[i].Name ?? "",
							Children = new List<NestedHierarchyIdRecordDetailsDTO>()
						});
					}
					else
					{
						throw new ApplicationException($"Parent record could not be found for  " +
											$"RecordID {allHierarchyItemzs[i].Id} with " +
											$"HierarchyID  {allHierarchyItemzs[i].ItemzHierarchyId!.ToString()} and " +
											$"Level as {allHierarchyItemzs[i].ItemzHierarchyId!.GetLevel().ToString()} " +
											"Please contact your System Administrator.");
					}
				}
			}

			recordCountAndEnumerable.AllRecords = returningRecords;
			return recordCountAndEnumerable;
		}




		public async Task<RecordCountAndEnumerable<NestedHierarchyIdRecordDetailsDTO>> GetAllChildrenOfItemzHierarchy(Guid recordId)
		{
			if (recordId == Guid.Empty)
			{
				throw new ArgumentNullException(nameof(recordId));
			}

			var foundHierarchyRecord = _context.ItemzHierarchy!.AsNoTracking()
							.Where(ih => ih.Id == recordId);

			if (foundHierarchyRecord.Count() != 1)
			{
				throw new ApplicationException($"Expected 1 Hierarchy record to be found " +
					$"but instead found {foundHierarchyRecord.Count()} records for ID {recordId}" +
					"Please contact your System Administrator.");
			}

			var foundHierarchyRecordLevel = foundHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId!.GetLevel();

			// EXPLANATION : We are using SQL Server HierarchyID field type. Now we can use EF Core special
			// methods to query for all Decendents as per below. By adding clause to check for GetLevel which is less then
			// CurrentHierarchyRecordLevel + three, we get Hierarchy record itself plus two more deeper level of hierarchy records.
			// The 1st Level data of the record itself is ignored and then 2nd level data is the actual child records.
			// While third level data are used for calculating number of children for child records.

			RecordCountAndEnumerable<NestedHierarchyIdRecordDetailsDTO> recordCountAndEnumerable = new RecordCountAndEnumerable<NestedHierarchyIdRecordDetailsDTO>();

			var itemzHierarchyRecords = await _context.ItemzHierarchy!
					.AsNoTracking()
					.Where(ih => ih.ItemzHierarchyId!.IsDescendantOf(foundHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId!))
						// && ih.ItemzHierarchyId.GetLevel() < (foundHierarchyRecordLevel + 3))
					.OrderBy(ih => ih.ItemzHierarchyId!)
					.ToListAsync();

			List<NestedHierarchyIdRecordDetailsDTO> returningRecords = [];
			//NestedHierarchyIdRecordDetailsDTO hierarchyIdRecordDetails = new();
			//bool hasParent = false;
			//int previousRecordHierarchyLevel = 0;

			if (itemzHierarchyRecords.Count() > 1) // We check for 1 as 1st record returned is the same as recordId which we skip out.
			{
				recordCountAndEnumerable.RecordCount = (itemzHierarchyRecords.Count() - 1);
			}
			else
			{
				recordCountAndEnumerable.RecordCount = 0;
				recordCountAndEnumerable.AllRecords = new List<NestedHierarchyIdRecordDetailsDTO>();
			}


			for (var i = 0; i < itemzHierarchyRecords.Count(); i++)
			{
				if (i == 0) continue; // Skip first record as it's for the supplied recordId
				if (itemzHierarchyRecords[i].ItemzHierarchyId!.GetLevel() == (foundHierarchyRecordLevel + 1))
				{
					//hierarchyIdRecordDetails.RecordId = itemzTypeHierarchyItemzs[i].Id;
					//hierarchyIdRecordDetails.Name = itemzTypeHierarchyItemzs[i].Name ?? "";
					//hierarchyIdRecordDetails.HierarchyId = itemzTypeHierarchyItemzs[i].ItemzHierarchyId!.ToString();
					//hierarchyIdRecordDetails.RecordType = itemzTypeHierarchyItemzs[i].RecordType;
					//hierarchyIdRecordDetails.Level = itemzTypeHierarchyItemzs[i].ItemzHierarchyId!.GetLevel();
					returningRecords.Add(new NestedHierarchyIdRecordDetailsDTO
					{
						RecordId = itemzHierarchyRecords[i].Id,
						HierarchyId = itemzHierarchyRecords[i].ItemzHierarchyId!.ToString(),
						Level = itemzHierarchyRecords[i].ItemzHierarchyId!.GetLevel(),
						RecordType = itemzHierarchyRecords[i].RecordType,
						Name = itemzHierarchyRecords[i].Name ?? "",
						Children = new List<NestedHierarchyIdRecordDetailsDTO>()
					});
				}
				else if (itemzHierarchyRecords[i].ItemzHierarchyId!.GetLevel() > (foundHierarchyRecordLevel + 1))
				{
					// Console.WriteLine($"Now Processing {itemzTypeHierarchyItemzs[i].Name ?? ""}");

					// Find the last record at a specified level directly within returningRecords
					//var targetLevel = (itemzHierarchyRecords[i].ItemzHierarchyId!.GetLevel() - 1);
					var foundParentRecordInReturningList = FindParentRecord(returningRecords, itemzHierarchyRecords[i]);

					if (foundParentRecordInReturningList != null)
					{
						// Add a child to the last record at the specified level
						foundParentRecordInReturningList.Children.Add(new NestedHierarchyIdRecordDetailsDTO
						{
							RecordId = itemzHierarchyRecords[i].Id,
							HierarchyId = itemzHierarchyRecords[i].ItemzHierarchyId!.ToString(),
							Level = itemzHierarchyRecords[i].ItemzHierarchyId!.GetLevel(),
							RecordType = itemzHierarchyRecords[i].RecordType,
							Name = itemzHierarchyRecords[i].Name ?? "",
							Children = new List<NestedHierarchyIdRecordDetailsDTO>()
						});

						// Console.WriteLine("Child added to the last record at Level " + targetLevel);
					}
					else
					{
						throw new ApplicationException($"Parent record could not be found for  " +
											$"RecordID {itemzHierarchyRecords[i].Id} with " +
											$"HierarchyID  {itemzHierarchyRecords[i].ItemzHierarchyId!.ToString()} and " +
											$"Level as {itemzHierarchyRecords[i].ItemzHierarchyId!.GetLevel().ToString()} " +
											"Please contact your System Administrator.");
						// Console.WriteLine("No records found at Level " + targetLevel);
					}

					//returningRecords.Where<NestedHierarchyIdRecordDetailsDTO>(hierarchyIdRecordDetails => 
					//hierarchyIdRecordDetails.Level == (itemzTypeHierarchyItemzs[i].ItemzHierarchyId!.GetLevel() - 1 ))
					//	.OrderBy(hierarchyIdRecordDetails => hierarchyIdRecordDetails.HierarchyId)
					//	.Last()
					//	.Children.Add(new NestedHierarchyIdRecordDetailsDTO
					//	{
					//		RecordId = itemzTypeHierarchyItemzs[i].Id,
					//		HierarchyId = itemzTypeHierarchyItemzs[i].ItemzHierarchyId!.ToString(),
					//		Level = itemzTypeHierarchyItemzs[i].ItemzHierarchyId!.GetLevel(),
					//		RecordType = itemzTypeHierarchyItemzs[i].RecordType,
					//		Name = itemzTypeHierarchyItemzs[i].Name ?? "",
					//		Children = new()
					//	});
				}
			}

			recordCountAndEnumerable.AllRecords = returningRecords;
			return recordCountAndEnumerable;
		}

        public async Task<int> GetAllChildrenCountOfItemzHierarchy(Guid recordId)
        {
            if (recordId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(recordId));
            }

            var foundHierarchyRecord = _context.ItemzHierarchy!.AsNoTracking()
                            .Where(ih => ih.Id == recordId);

            if (foundHierarchyRecord.Count() != 1 )
            {
                throw new ApplicationException($"Expected 1 Hierarchy record to be found " +
                    $"but instead found {foundHierarchyRecord.Count()} records for ID {recordId}. " +
                    "Please contact your System Administrator."); // We dont expect multiple records for a single recordId anyways.
            }

            // EXPLANATION : We are using SQL Server HierarchyID field type. Now we can use EF Core special
            // methods to query for all Decendents as per below. 

			return (
						await _context.ItemzHierarchy!
						.AsNoTracking()
						.Where(ih => ih.ItemzHierarchyId!.IsDescendantOf(foundHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId!))
						.OrderBy(ih => ih.ItemzHierarchyId!)
						.CountAsync()
					) - 1; // reducing it by 1 because it will include record for supplied recordId as well.
        }

        public static NestedHierarchyIdRecordDetailsDTO? FindParentRecord(List<NestedHierarchyIdRecordDetailsDTO> records, ItemzHierarchy childRecordToBeInserted)
		{
			NestedHierarchyIdRecordDetailsDTO? lastRecord = null;

			foreach (var record in records)
			{
				if ((record.Level == (childRecordToBeInserted.ItemzHierarchyId!.GetLevel() -1) ) 
					&& (childRecordToBeInserted.ItemzHierarchyId.ToString().StartsWith(record.HierarchyId!.ToString()))  )
				{
					lastRecord = record; 
					break; 
				}

				var childRecord = FindParentRecord(record.Children, childRecordToBeInserted);
				if (childRecord != null)
				{
					lastRecord = childRecord;
					break;
				}
			}
			return lastRecord;
		}

		////public static NestedHierarchyIdRecordDetailsDTO? FindLastRecordAtLevelOLD(List<NestedHierarchyIdRecordDetailsDTO> records, int targetLevel)
		////{
		////	NestedHierarchyIdRecordDetailsDTO? lastRecord = null;

		////	foreach (var record in records)
		////	{
		////		if (record.Level == targetLevel)
		////		{
		////			if (lastRecord == null || string.Compare(record.HierarchyId, lastRecord.HierarchyId, StringComparison.Ordinal) > 0)
		////			{
		////				lastRecord = record;
		////			}
		////		}

		////		var childRecord = FindLastRecordAtLevelOLD(record.Children, targetLevel);
		////		if (childRecord != null)
		////		{
		////			if (lastRecord == null || string.Compare(childRecord.HierarchyId, lastRecord.HierarchyId, StringComparison.Ordinal) > 0)
		////			{
		////				lastRecord = childRecord;
		////			}
		////		}
		////	}

		////	return lastRecord;
		////}

		public async Task<bool> UpdateHierarchyRecordNameByID(Guid recordId, string name)
		{
			if (recordId == Guid.Empty)
			{
				throw new ArgumentNullException(nameof(recordId));
			}

			var foundHierarchyRecord = _context.ItemzHierarchy!
							.Where(ih => ih.Id == recordId);

			if (foundHierarchyRecord.Count() != 1)
			{
				throw new ApplicationException($"Expected 1 Hierarchy record to be found " +
					$"but instead found {foundHierarchyRecord.Count()} records for ID {recordId}" +
					"Please contact your System Administrator.");
			}

			if (foundHierarchyRecord.FirstOrDefault()!.RecordType.ToLower() == "repository") // TODO :: Use Constants instead of Text
			{
				throw new ApplicationException($"Can not update name of the Repository Root Hierarchy Record with ID {recordId}");
			}
			if (foundHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId!.GetLevel() == 0) // TODO :: Use Constants instead of Text
			{
				throw new ApplicationException($"Can not update name of the Repository Root Hierarchy Record with ID {recordId}");
			}

			foundHierarchyRecord.FirstOrDefault()!.Name = name; // TODO :: Remove special characters from name variable before saving it to DB. Security Reason.

			return (await _context.SaveChangesAsync() >= 0);
		}
	}
}
