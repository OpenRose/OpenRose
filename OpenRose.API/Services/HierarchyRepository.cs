// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using ItemzApp.API.DbContexts;
using ItemzApp.API.Entities;
using ItemzApp.API.Helper;
using ItemzApp.API.Models;
using ItemzApp.API.Models.BetweenControllerAndRepository;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace ItemzApp.API.Services
{
	public class HierarchyRepository : IHierarchyRepository
	{
		private readonly ItemzContext _context;
		private readonly ILogger<HierarchyRepository> _logger;
		private readonly EstimationRollupService _estimationRollupService;  // ADD THIS

		public HierarchyRepository(
			ItemzContext context,
			ILogger<HierarchyRepository> logger,
			EstimationRollupService estimationRollupService)  // ADD THIS
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_estimationRollupService = estimationRollupService ?? throw new ArgumentNullException(nameof(estimationRollupService));  // ADD THIS
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



		// PHASE 1: Get hierarchy record details by HierarchyId path value
		public async Task<HierarchyIdRecordDetailsDTO?> GetHierarchyRecordDetailsByHierarchyIdPath(HierarchyId hierarchyIdPath)
		{
			if (hierarchyIdPath == null)
			{
				throw new ArgumentNullException(nameof(hierarchyIdPath));
			}

			// Find the record with this HierarchyId path
			var foundHierarchyRecord = await _context.ItemzHierarchy!
				.AsNoTracking()
				.Where(ih => ih.ItemzHierarchyId == hierarchyIdPath)
				.ToListAsync();

			if (foundHierarchyRecord.Count() != 1)
			{
				return null; // Record not found or multiple records found
			}

			// Use existing GetHierarchyRecordDetailsByID method to return DTO
			return await GetHierarchyRecordDetailsByID(foundHierarchyRecord.FirstOrDefault()!.Id);
		}


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
					$"but instead found {foundHierarchyRecord.Count()} records for ID {recordId} " +
					"Please contact your System Administrator.");
			}
			var hierarchyIdRecordDetails = new HierarchyIdRecordDetailsDTO();
			hierarchyIdRecordDetails.RecordId = recordId;
			hierarchyIdRecordDetails.Name = foundHierarchyRecord.FirstOrDefault()!.Name ?? "";
			hierarchyIdRecordDetails.HierarchyId = foundHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId!.ToString();
			hierarchyIdRecordDetails.RecordType = foundHierarchyRecord.FirstOrDefault()!.RecordType;
			hierarchyIdRecordDetails.Level = foundHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId!.GetLevel();

			// Adding estimation fields
			hierarchyIdRecordDetails.EstimationUnit = foundHierarchyRecord.FirstOrDefault()!.EstimationUnit;
			hierarchyIdRecordDetails.OwnEstimation = foundHierarchyRecord.FirstOrDefault()!.OwnEstimation;
			hierarchyIdRecordDetails.RolledUpEstimation = foundHierarchyRecord.FirstOrDefault()!.RolledUpEstimation;

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

			var foundHierarchyRecord = _context.ItemzHierarchy!.AsNoTracking()
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
						&& ih.ItemzHierarchyId.GetLevel() < (foundHierarchyRecordLevel + 3))
					.OrderBy(ih => ih.ItemzHierarchyId!)
					.ToListAsync();

			List<HierarchyIdRecordDetailsDTO> returningRecords = [];
			HierarchyIdRecordDetailsDTO hierarchyIdRecordDetails = new();
			string? _localTopChildHierarchyId = null;
			int _localNumerOfChildNodes = 0;

			if (itemzTypeHierarchyItemzs.Count() < 2) // Less then 2 because we may have project entry but no ItemzType below it.
			{
				return default;
			}

			for (var i = 0; i < itemzTypeHierarchyItemzs.Count(); i++)
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
				hierarchyIdRecordDetails.BottomChildHierarchyId = itemzTypeHierarchyItemzs[(itemzTypeHierarchyItemzs.Count() - 1)].ItemzHierarchyId!.ToString();
				hierarchyIdRecordDetails.NumberOfChildNodes = _localNumerOfChildNodes;
			}
			returningRecords.Add(hierarchyIdRecordDetails);

			return returningRecords;
		}


		public async Task<NestedHierarchyIdRecordDetailsDTO?> GetRepositoryHierarchyRecord()
		{
			// Find the hierarchy record where Level == 0 (repository root)
			var foundRepositoryRecord = await _context.ItemzHierarchy!
				.AsNoTracking()
				.Where(ih => ih.ItemzHierarchyId!.GetLevel() == 0)
				.FirstOrDefaultAsync();

			if (foundRepositoryRecord == null)
			{
				return null; // Or throw an exception if this should never happen
			}

			// Map to NestedHierarchyIdRecordDetailsDTO
			var returningRepositoryRecordDTO = new NestedHierarchyIdRecordDetailsDTO
			{
				RecordId = foundRepositoryRecord.Id,
				HierarchyId = foundRepositoryRecord.ItemzHierarchyId!.ToString(),
				Level = foundRepositoryRecord.ItemzHierarchyId.GetLevel(),
				RecordType = foundRepositoryRecord.RecordType,
				Name = foundRepositoryRecord.Name ?? "",
				Children = new List<NestedHierarchyIdRecordDetailsDTO>()
			};

			return returningRepositoryRecordDTO;
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

			if (foundHierarchyRecord.Count() != 1)
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
				if ((record.Level == (childRecordToBeInserted.ItemzHierarchyId!.GetLevel() - 1))
					&& (childRecordToBeInserted.ItemzHierarchyId.ToString().StartsWith(record.HierarchyId!.ToString())))
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


		public async Task<ItemzHierarchy?> GetHierarchyRecordForUpdateAsync(Guid recordId)
		{
			if (recordId == Guid.Empty)
			{
				throw new ArgumentNullException(nameof(recordId));
			}

			// EXPLANATION :: We use SingleOrDefaultAsync to ensure exactly one record is returned.
			// This avoids running Count() + FirstOrDefault() which executes two SQL queries.
			var hierarchyRecord = await _context.ItemzHierarchy!
				.SingleOrDefaultAsync(ih => ih.Id == recordId);

			if (hierarchyRecord == null)
			{
				return null; // Controller can handle not found
			}

			// Prevent updates to repository root records
			if (hierarchyRecord.RecordType.Equals("repository", StringComparison.OrdinalIgnoreCase)) // TODO :: Use Constants instead of Text
			{
				throw new ApplicationException($"Can not update name of the Repository Root Hierarchy Record with ID {recordId}");
			}

			if (hierarchyRecord.ItemzHierarchyId!.GetLevel() == 0) // TODO :: Use Constants instead of Text
			{
				throw new ApplicationException($"Can not update name of the Repository Root Hierarchy Record with ID {recordId}");
			}

			return hierarchyRecord;
		}



		public async Task<bool> UpdateHierarchyRecordNameByID(Guid recordId, string newItemzName)
		{
			if (recordId == Guid.Empty)
			{
				throw new ArgumentNullException(nameof(recordId));
			}

			if (string.IsNullOrWhiteSpace(newItemzName))
			{
				throw new ArgumentNullException(nameof(newItemzName));
			}

			var foundHierarchyRecord = _context.ItemzHierarchy!
							.Where(ih => ih.Id == recordId);

			if (foundHierarchyRecord.Count() != 1)
			{
				throw new ApplicationException($"Expected 1 Hierarchy record to be found " +
					$"but instead found {foundHierarchyRecord.Count()} records for ID {recordId}" +
					"Please contact your System Administrator.");
			}

			//if (foundHierarchyRecord.FirstOrDefault()!.RecordType.ToLower() == "repository") // TODO :: Use Constants instead of Text
			//{
			//	throw new ApplicationException($"Can not update name of the Repository Root Hierarchy Record with ID {recordId}");
			//}
			//if (foundHierarchyRecord.FirstOrDefault()!.ItemzHierarchyId!.GetLevel() == 0) // TODO :: Use Constants instead of Text
			//{
			//	throw new ApplicationException($"Can not update name of the Repository Root Hierarchy Record with ID {recordId}");
			//}

			foundHierarchyRecord.FirstOrDefault()!.Name = newItemzName; // TODO :: Remove special characters from name variable before saving it to DB. Security Reason.

			return (await _context.SaveChangesAsync() >= 0);
		}


		/// <summary>
		/// Updates estimation fields (EstimationUnit and/or OwnEstimation) for a hierarchy record
		/// PHASE 1: Triggers roll-up recalculation after update
		/// 
		/// When EstimationUnit is updated for a Project record, automatically synchronizes
		/// the new value to all descendant records (ItemzType, Itemz, etc.) via stored procedure.
		/// Supports setting EstimationUnit to NULL or empty string as valid updates.
		/// </summary>
		/// <param name="recordId">The ID of the hierarchy record to update</param>
		/// <param name="estimationUnit">New estimation unit (optional, can be NULL or empty)</param>
		/// <param name="ownEstimation">New own estimation value (optional)</param>
		/// <returns>True if update successful, False otherwise</returns>
		public async Task<bool> UpdateHierarchyEstimationFieldsAsync(
			Guid recordId,
			string? estimationUnit = null,
			decimal? ownEstimation = null)
		{
			if (recordId == Guid.Empty)
			{
				throw new ArgumentNullException(nameof(recordId));
			}

			var hierarchyRecord = await _context.ItemzHierarchy!
				.FirstOrDefaultAsync(ih => ih.Id == recordId);

			if (hierarchyRecord == null)
			{
				return false;
			}

			bool hasChanged = false;
			bool estimationUnitChanged = false;
			decimal estimationDelta = 0;
			string? oldEstimationUnit = null;

			// EXPLANATION: Check if EstimationUnit has been provided (even if NULL or empty)
			// We use a sentinel value approach: Only process if the parameter was explicitly provided
			// However, since we can't distinguish between "not provided" and "provided as null" in optional params,
			// we check: if estimationUnit parameter is not null OR if it's explicitly null (user wants to clear it)
			// 
			// Key change: We now allow NULL/empty string as valid updates for Project EstimationUnit
			// This enables users to clear the EstimationUnit field via the UI

			if (string.Equals(hierarchyRecord.RecordType, "Project", StringComparison.OrdinalIgnoreCase)
				&& !(string.IsNullOrEmpty(estimationUnit)))
			{
				// EXPLANATION: For Project records, we need to detect if EstimationUnit should be updated
				// We check for actual changes by comparing:
				// 1. If provided value (estimationUnit param) is not null: trim and compare (case-sensitive)
				// 2. If provided value is null: check if current value is not null (clearing operation)

				string? trimmedEstimationUnit = estimationUnit?.Trim();

				// Case-sensitive comparison to detect changes
				// This handles three scenarios:
				// A) User provides a non-empty value different from current → Update
				// B) User provides empty/null when current has value → Clear (update to null)
				// C) User provides same value as current (case-sensitive) → No change
				if (!string.Equals(hierarchyRecord.EstimationUnit ?? "", trimmedEstimationUnit ?? "", StringComparison.Ordinal))
				{
					oldEstimationUnit = hierarchyRecord.EstimationUnit;
					hierarchyRecord.EstimationUnit = trimmedEstimationUnit; // Store trimmed value (which could be null)
					estimationUnitChanged = true;
					hasChanged = true;

					_logger.LogInformation(
						$"UpdateHierarchyEstimationFieldsAsync: EstimationUnit changed for Project {recordId} " +
						$"from '{oldEstimationUnit ?? "NULL"}' to '{trimmedEstimationUnit ?? "NULL"}'");
				}
			}

			// Update own estimation and calculate delta
			if (ownEstimation.HasValue && hierarchyRecord.OwnEstimation != ownEstimation.Value)
			{
				var oldEstimationValue = hierarchyRecord.OwnEstimation;
				estimationDelta = ownEstimation.Value - oldEstimationValue;

				hierarchyRecord.OwnEstimation = ownEstimation.Value;
				hierarchyRecord.RolledUpEstimation += estimationDelta; // Update current node

				hasChanged = true;

				// Log change to ItemzChangeHistory
				if (string.Equals(hierarchyRecord.RecordType, "Itemz", StringComparison.OrdinalIgnoreCase))
				{
					try
					{
						var changeHistoryEntry = new ItemzChangeHistory
						{
							ItemzId = recordId,
							CreatedDate = DateTimeOffset.Now,
							OldValues = oldEstimationValue.ToString(),
							NewValues = ownEstimation.Value.ToString(),
							ChangeEvent = "OwnEstimationChanged"
						};
						_context.ItemzChangeHistory!.Add(changeHistoryEntry);
					}
					catch (Exception ex)
					{
						_logger.LogWarning($"Could not log estimation change for record {recordId}: {ex.Message}");
					}
				}
			}

			if (hasChanged)
			{
				await _context.SaveChangesAsync();

				// PHASE 1: Trigger optimized roll-up recalculation for ancestors using delta
				if (hierarchyRecord.ItemzHierarchyId != null && estimationDelta != 0)
				{
					_logger.LogInformation(
						$"UpdateHierarchyEstimationFieldsAsync: Triggered optimized recalculation for record {recordId} with delta {estimationDelta}");

					// Get parent and trigger optimized recalculation
					var parentHierarchyId = hierarchyRecord.ItemzHierarchyId.GetAncestor(1);
					if (parentHierarchyId != null)
					{
						var parentRecord = await _context.ItemzHierarchy!
							.FirstOrDefaultAsync(ih => ih.ItemzHierarchyId == parentHierarchyId);

						if (parentRecord != null)
						{
							_logger.LogInformation(
								$"UpdateHierarchyEstimationFieldsAsync: Propagating delta {estimationDelta} to ancestors starting from {parentRecord.Id}");
							await _estimationRollupService.RecalculateSingleRecordRollUpOptimizedAsync(parentRecord.Id, estimationDelta);
						}
					}
				}

				// PHASE 1: Synchronize EstimationUnit to all child records if it changed for Project
				// EXPLANATION: Only trigger if:
				// 1. EstimationUnit was actually updated (not just provided but unchanged)
				// 2. The record type is 'Project' (business rule: only Projects can update EstimationUnit)
				// 3. The old value differs from new value (case-sensitive comparison already done above)
				// 4. This includes updates to NULL/empty string (clearing operation)
				if (estimationUnitChanged && string.Equals(hierarchyRecord.RecordType, "Project", StringComparison.OrdinalIgnoreCase))
				{
					_logger.LogInformation(
						$"UpdateHierarchyEstimationFieldsAsync: Synchronizing EstimationUnit to all descendants for Project {recordId}");

					try
					{
						// Call stored procedure to update all child records with new EstimationUnit
						// The stored procedure handles both setting values and clearing (NULL) values
						var syncResult = await _estimationRollupService.SetEstimationUnitForProjectAsync(recordId);

						if (syncResult)
						{
							_logger.LogInformation(
								$"UpdateHierarchyEstimationFieldsAsync: Successfully synchronized EstimationUnit " +
								$"to all descendants of Project {recordId}");
						}
						else
						{
							_logger.LogWarning(
								$"UpdateHierarchyEstimationFieldsAsync: Failed to synchronize EstimationUnit " +
								$"to descendants of Project {recordId}");
							// Non-fatal: Project record was updated, but sync failed
							// Log the failure but continue
						}
					}
					catch (Exception ex)
					{
						_logger.LogError(
							$"UpdateHierarchyEstimationFieldsAsync: Exception occurred while synchronizing EstimationUnit " +
							$"for Project {recordId}: {ex.Message}", ex);
						// Non-fatal error: Project was updated but sync failed
					}
				}

				return true;
			}

			return true;
		}

		/// <summary>
		/// Adds EstimationUnit to a hierarchy record by copying it from its immediate parent.
		/// This ensures new records (Itemz, ItemzType) inherit the EstimationUnit from their parent.
		/// Used during record creation to maintain EstimationUnit integrity across the hierarchy.
		/// 
		/// Ensures all child records have consistent EstimationUnit with their parent
		/// </summary>
		/// <param name="recordId">The ID of the hierarchy record to update (newly created Itemz or ItemzType)</param>
		/// <returns>True if update successful, False if record or parent not found</returns>
		public async Task<bool> AddHierarchyRecordEstimationUnitAsync(Guid recordId)
		{
			if (recordId == Guid.Empty)
			{
				throw new ArgumentNullException(nameof(recordId));
			}

			try
			{
				// STEP 1: Get the newly created record
				var hierarchyRecord = await _context.ItemzHierarchy!
					.FirstOrDefaultAsync(ih => ih.Id == recordId);

				if (hierarchyRecord == null)
				{
					_logger.LogWarning(
						$"AddHierarchyRecordEstimationUnitAsync: Hierarchy record not found for ID: {recordId}");
					return false;
				}

				// STEP 2: Verify the record has a valid HierarchyId path
				if (hierarchyRecord.ItemzHierarchyId == null)
				{
					_logger.LogWarning(
						$"AddHierarchyRecordEstimationUnitAsync: Record {recordId} has no ItemzHierarchyId path");
					return false;
				}

				_logger.LogDebug(
					$"AddHierarchyRecordEstimationUnitAsync: Processing record {recordId} " +
					$"(Type: {hierarchyRecord.RecordType}, Current EstimationUnit: {hierarchyRecord.EstimationUnit ?? "NULL"})");

				// STEP 3: Get the immediate parent HierarchyId path
				var parentHierarchyIdPath = hierarchyRecord.ItemzHierarchyId.GetAncestor(1);

				if (parentHierarchyIdPath == null)
				{
					_logger.LogWarning(
						$"AddHierarchyRecordEstimationUnitAsync: Could not determine parent HierarchyId path for record {recordId}");
					return false;
				}

				// STEP 4: Find the parent record
				var parentRecord = await _context.ItemzHierarchy!
					.FirstOrDefaultAsync(ih => ih.ItemzHierarchyId == parentHierarchyIdPath);

				if (parentRecord == null)
				{
					_logger.LogWarning(
						$"AddHierarchyRecordEstimationUnitAsync: Parent record not found for HierarchyId path: {parentHierarchyIdPath}");
					return false;
				}

				_logger.LogDebug(
					$"AddHierarchyRecordEstimationUnitAsync: Found parent record {parentRecord.Id} " +
					$"(Type: {parentRecord.RecordType}, EstimationUnit: {parentRecord.EstimationUnit ?? "NULL"})");

				// STEP 5: Copy EstimationUnit from parent to child record
				// No null/empty checks needed - just copy whatever value parent has
				string? parentEstimationUnit = parentRecord.EstimationUnit;

				// Only update if the child's EstimationUnit differs from parent's
				if (!string.Equals(hierarchyRecord.EstimationUnit ?? "", parentEstimationUnit ?? "", StringComparison.Ordinal))
				{
					hierarchyRecord.EstimationUnit = parentEstimationUnit;
					_context.ItemzHierarchy!.Update(hierarchyRecord);
					await _context.SaveChangesAsync();

					_logger.LogInformation(
						$"AddHierarchyRecordEstimationUnitAsync: Updated EstimationUnit for record {recordId} " +
						$"from '{hierarchyRecord.EstimationUnit ?? "NULL"}' to '{parentEstimationUnit ?? "NULL"}' " +
						$"(inherited from parent {parentRecord.Id})");

					return true;
				}
				else
				{
					_logger.LogDebug(
						$"AddHierarchyRecordEstimationUnitAsync: Record {recordId} already has matching EstimationUnit from parent. " +
						$"No update needed.");
					return true;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(
					$"AddHierarchyRecordEstimationUnitAsync: Exception occurred while adding EstimationUnit for record {recordId}: {ex.Message}",
					ex);
				return false;
			}
		}

		//// PHASE 1: Helper method to recalculate all ancestor records
		//private async Task<bool> RecalculateAncestorsAsync(Guid recordId)
		//{
		//	try
		//	{
		//		var currentRecord = await _context.ItemzHierarchy!
		//			.FirstOrDefaultAsync(ih => ih.Id == recordId);

		//		if (currentRecord == null || currentRecord.ItemzHierarchyId == null)
		//		{
		//			_logger.LogWarning($"RecalculateAncestorsAsync: Current record not found for ID: {recordId}");
		//			return false;
		//		}

		//		var currentLevel = currentRecord.ItemzHierarchyId.GetLevel();
		//		_logger.LogInformation($"RecalculateAncestorsAsync: Starting ancestor recalculation from level {currentLevel} for record {recordId}");

		//		// Traverse up the hierarchy and recalculate each level
		//		for (int i = currentLevel - 1; i > 0; i--)
		//		{
		//			var ancestorHierarchyId = currentRecord.ItemzHierarchyId.GetAncestor(currentLevel - i);
		//			if (ancestorHierarchyId == null)
		//			{
		//				_logger.LogWarning($"RecalculateAncestorsAsync: Ancestor HierarchyId is null at level {i}");
		//				break;
		//			}

		//			var ancestorRecord = await _context.ItemzHierarchy!
		//				.FirstOrDefaultAsync(ih => ih.ItemzHierarchyId == ancestorHierarchyId);

		//			if (ancestorRecord != null)
		//			{
		//				_logger.LogInformation($"RecalculateAncestorsAsync: Recalculating ancestor at level {i}, ID: {ancestorRecord.Id}");
		//				bool recalcResult = await RecalculateSingleRecordRollUpAsync(ancestorRecord.Id);
		//				_logger.LogInformation($"RecalculateAncestorsAsync: Recalculation result for {ancestorRecord.Id}: {recalcResult}");
		//			}
		//			else
		//			{
		//				_logger.LogWarning($"RecalculateAncestorsAsync: Ancestor record not found at level {i}");
		//			}
		//		}

		//		_logger.LogInformation($"RecalculateAncestorsAsync: Completed ancestor recalculation for record {recordId}");
		//		return true;
		//	}
		//	catch (Exception ex)
		//	{
		//		_logger.LogError($"Exception in RecalculateAncestorsAsync: {ex.Message}", ex);
		//		return false;
		//	}
		//}

		//// Helper method for recursive ancestor recalculation
		//// TODO :: WE NEED TO REMOVE THIS METHOD AND REPLACE CALLS TO THIS METHOD WITH CALLS TO
		//// RecalculateSingleRecordRollUpOptimizedAsync WHICH USES DELTA BASED CALCULATION
		//// AND ALSO STOPS PROPAGATION AT PROJECT LEVEL (LEVEL 1) TO OPTIMIZE PERFORMANCE.

		//private async Task<bool> RecalculateSingleRecordRollUpAsync(Guid hierarchyRecordId)
		//{
		//	try
		//	{
		//		var hierarchyRecord = await _context.ItemzHierarchy!
		//			.FirstOrDefaultAsync(ih => ih.Id == hierarchyRecordId);

		//		if (hierarchyRecord == null)
		//		{
		//			_logger.LogWarning($"RecalculateSingleRecordRollUpAsync: Hierarchy record not found for ID: {hierarchyRecordId}");
		//			return false;
		//		}

		//		_logger.LogInformation($"RecalculateSingleRecordRollUpAsync: Calculating roll-up for record ID {hierarchyRecordId}");

		//		// Calculate new roll-up value: own estimation + sum of all immediate children's rolled-up estimations
		//		decimal newRolledUpEstimation = hierarchyRecord.OwnEstimation;
		//		_logger.LogInformation($"RecalculateSingleRecordRollUpAsync: Starting with own estimation: {newRolledUpEstimation}");

		//		// Get all immediate children
		//		var immediateChildren = await _context.ItemzHierarchy!
		//			.Where(ih => ih.ItemzHierarchyId!.GetAncestor(1) == hierarchyRecord.ItemzHierarchyId!)
		//			.ToListAsync();

		//		_logger.LogInformation($"RecalculateSingleRecordRollUpAsync: Found {immediateChildren.Count} immediate children");

		//		// Add all children's rolled-up estimations to this record's roll-up
		//		foreach (var child in immediateChildren)
		//		{
		//			_logger.LogInformation($"RecalculateSingleRecordRollUpAsync: Adding child {child.Id} rolled-up estimation: {child.RolledUpEstimation}");
		//			newRolledUpEstimation += child.RolledUpEstimation;
		//		}

		//		_logger.LogInformation($"RecalculateSingleRecordRollUpAsync: New roll-up estimation calculated: {newRolledUpEstimation} (was: {hierarchyRecord.RolledUpEstimation})");

		//		// Update the record if roll-up value changed
		//		if (hierarchyRecord.RolledUpEstimation != newRolledUpEstimation)
		//		{
		//			hierarchyRecord.RolledUpEstimation = newRolledUpEstimation;
		//			_context.ItemzHierarchy!.Update(hierarchyRecord);
		//			await _context.SaveChangesAsync();

		//			_logger.LogInformation($"RecalculateSingleRecordRollUpAsync: UPDATED roll-up estimation for hierarchy record ID {hierarchyRecordId}. New value: {newRolledUpEstimation}");
		//			return true;
		//		}
		//		else
		//		{
		//			_logger.LogInformation($"RecalculateSingleRecordRollUpAsync: Roll-up value unchanged for {hierarchyRecordId}");
		//			return true;
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		_logger.LogError($"Exception occurred while recalculating roll-up for record ID {hierarchyRecordId}: {ex.Message}", ex);
		//		return false;
		//	}
		//}


		//// IMPROVED METHOD: Simple arithmetic-based roll-up calculation
		//private async Task<bool> RecalculateSingleRecordRollUpOptimizedAsync(
		//			Guid hierarchyRecordId,
		//			decimal estimationDelta)
		//{
		//	try
		//	{
		//		var currentRecord = await _context.ItemzHierarchy!
		//			.FirstOrDefaultAsync(ih => ih.Id == hierarchyRecordId);

		//		if (currentRecord == null || currentRecord.ItemzHierarchyId == null)
		//		{
		//			_logger.LogWarning($"Hierarchy record not found for ID: {hierarchyRecordId}");
		//			return false;
		//		}

		//		// Check if we've reached the Project level (Level 1) BEFORE updating
		//		int currentLevel = currentRecord.ItemzHierarchyId.GetLevel();

		//		if (currentLevel == 1)
		//		{
		//			_logger.LogInformation(
		//				$"Current record {hierarchyRecordId} is at Project level (Level 1). " +
		//				$"Applying delta and STOPPING propagation.");

		//			currentRecord.RolledUpEstimation += estimationDelta;
		//			_context.ItemzHierarchy!.Update(currentRecord);
		//			await _context.SaveChangesAsync();

		//			_logger.LogInformation(
		//				$"Updated rolled-up estimation for Project {hierarchyRecordId} by delta {estimationDelta}. " +
		//				$"New value: {currentRecord.RolledUpEstimation}");

		//			return true; // STOP - don't propagate beyond Project level
		//		}

		//		// Apply the delta (difference) to rolled-up estimation
		//		currentRecord.RolledUpEstimation += estimationDelta;
		//		_context.ItemzHierarchy!.Update(currentRecord);
		//		await _context.SaveChangesAsync();

		//		_logger.LogInformation(
		//			$"Updated rolled-up estimation for {hierarchyRecordId} by delta {estimationDelta}. " +
		//			$"New value: {currentRecord.RolledUpEstimation}");

		//		// Get immediate parent
		//		var parentHierarchyId = currentRecord.ItemzHierarchyId.GetAncestor(1);
		//		if (parentHierarchyId != null)
		//		{
		//			var parentRecord = await _context.ItemzHierarchy!
		//				.FirstOrDefaultAsync(ih => ih.ItemzHierarchyId == parentHierarchyId);

		//			if (parentRecord != null)
		//			{
		//				// Check if parent is a Project (Level 1) BEFORE recursing
		//				if (parentRecord.ItemzHierarchyId!.GetLevel() == 1)
		//				{
		//					_logger.LogInformation(
		//						$"Next parent {parentRecord.Id} is a Project (Level 1). Will apply delta and stop.");

		//					// Recurse ONE MORE TIME to update the Project, then stop
		//					await RecalculateSingleRecordRollUpOptimizedAsync(parentRecord.Id, estimationDelta);
		//					return true;
		//				}

		//				// Continue propagating to ancestors (for non-Project levels)
		//				await RecalculateSingleRecordRollUpOptimizedAsync(parentRecord.Id, estimationDelta);
		//			}
		//		}

		//		return true;
		//	}
		//	catch (Exception ex)
		//	{
		//		_logger.LogError($"Exception in RecalculateSingleRecordRollUpOptimizedAsync: {ex.Message}", ex);
		//		return false;
		//	}
		//}

	}
}

