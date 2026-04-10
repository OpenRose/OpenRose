// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using ItemzApp.API.Entities;
using ItemzApp.API.Models;
using ItemzApp.API.Models.BetweenControllerAndRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ItemzApp.API.Services
{
    public interface IHierarchyRepository
    {
        public Task<HierarchyIdRecordDetailsDTO?> GetHierarchyRecordDetailsByID(Guid recordId);

		// PHASE 1: Get hierarchy record details by HierarchyId path value
		public Task<HierarchyIdRecordDetailsDTO?> GetHierarchyRecordDetailsByHierarchyIdPath(HierarchyId hierarchyIdPath);

		public Task<HierarchyIdRecordDetailsDTO?> GetNextSiblingHierarchyRecordDetailsByID(Guid recordId);

		public Task<IEnumerable<HierarchyIdRecordDetailsDTO?>> GetImmediateChildrenOfItemzHierarchy(Guid recordId);

		public Task<NestedHierarchyIdRecordDetailsDTO?> GetRepositoryHierarchyRecord();

		public Task<RecordCountAndEnumerable<NestedHierarchyIdRecordDetailsDTO>> GetAllParentsOfItemzHierarchy(Guid recordId);

		public Task<RecordCountAndEnumerable<NestedHierarchyIdRecordDetailsDTO>> GetAllChildrenOfItemzHierarchy(Guid recordId);

		public Task<int> GetAllChildrenCountOfItemzHierarchy(Guid recordId);

		public Task<ItemzHierarchy?> GetHierarchyRecordForUpdateAsync(Guid recordId);

		public Task<bool> UpdateHierarchyRecordNameByID(Guid recordId, string name);

		// PHASE 1: Update estimation fields and trigger roll-up recalculation
		public Task<bool> UpdateHierarchyEstimationFieldsAsync(
			Guid recordId,
			string? estimationUnit = null,
			decimal? ownEstimation = null);

		public Task<bool> AddHierarchyRecordEstimationUnitAsync(Guid recordId);
	}
}
