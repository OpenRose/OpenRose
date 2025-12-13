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


namespace ItemzApp.API.Services
{
    public class BaselineRepository : IBaselineRepository, IDisposable
    {
        private readonly BaselineContext _baselineContext;
        private readonly ItemzContext _itemzContext;

        public BaselineRepository(BaselineContext baselineContext, ItemzContext itemzContext)
        {
            _baselineContext = baselineContext ?? throw new ArgumentNullException(nameof(baselineContext));
            _itemzContext = itemzContext ?? throw new ArgumentNullException(nameof(itemzContext));
        }
        public async Task<Baseline?> GetBaselineAsync(Guid BaselineId)
        {
            if (BaselineId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(BaselineId));
            }

            return await _baselineContext.Baseline!
                .Where(b => b.Id == BaselineId).AsNoTracking().FirstOrDefaultAsync();
        } 

        public async Task<Baseline?> GetBaselineForUpdateAsync(Guid BaselineId)
        {
            if (BaselineId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(BaselineId));
            }

            return await _baselineContext.Baseline!
                .Where(b => b.Id == BaselineId).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Baseline>?> GetBaselinesAsync()
        {
            try
            {
                if (await _baselineContext.Baseline.CountAsync<Baseline>() > 0)
                {
                    var baselineCollection = await _baselineContext.Baseline.AsNoTracking().AsQueryable<Baseline>().OrderBy(b => b.Name).ToListAsync();

                    // TODO: We have to create simple implementation of sort by Baseline Name here 
                    // baselineCollection = baselineCollection.ApplySort("Name", null).AsNoTracking();

                    return baselineCollection;
                }
                return null;
            }
            catch (Exception ex)
            {
                // TODO: It's not good that we capture Generic Exception and then 
                // return null here. Basically, I wanted to check if we have 
                // baselines returned from the DB and if it does not then
                // it should simply return null back to the calling function.
                // One has to learn how to do this gracefully as part of Entity Framework 
                return null;
            }
        }

        public async Task<IEnumerable<BaselineItemzType>?> GetBaselineItemzTypesAsync(Guid BaselineId)
        {
            if (BaselineId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(BaselineId));
            }

            if (await _baselineContext.BaselineItemzType!.Where(bit => bit.BaselineId == BaselineId).AnyAsync())
            {
                return await _baselineContext.BaselineItemzType
                    .AsNoTracking()
                    .Where(bit => bit.BaselineId == BaselineId)
                    .AsQueryable<BaselineItemzType>()
                    .OrderBy(bit => bit.Name)
                    .ToListAsync();
            }
            return null;
        }

        // TODO: decide if we need GetBaseline by passing in collection of baselineIds
        // if yes, then we need to implement action method in BaselineController for the same
        // so that Swagger docs shows GET method under Baselines section.
        public async Task<IEnumerable<Baseline>> GetBaselinesAsync(IEnumerable<Guid> baselineIds)
        {
            if (baselineIds == null)
            {
                throw new ArgumentNullException(nameof(baselineIds));
            }

            return await _baselineContext.Baseline.AsNoTracking().Where(b => baselineIds.Contains(b.Id))
                .OrderBy(b => b.Name)
                .ToListAsync();
        }

        public async Task<Guid> AddBaselineAsync(Baseline baseline)
        {
            if (baseline == null)
            {
                throw new ArgumentNullException(nameof(baseline));
            }

            if (baseline.Name is null || baseline.Name == string.Empty)
            {
                throw new ArgumentNullException(nameof(baseline.Name));
            }

            if (baseline.ProjectId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(baseline.ProjectId));
            }

            if (!_baselineContext.Projects!.Where(p => p.Id == baseline.ProjectId).Any())
            {
                throw new ArgumentException(nameof(baseline.ProjectId));
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
                    ParameterName = "ProjectId",
                    Value = baseline.ProjectId,
                    SqlDbType = System.Data.SqlDbType.UniqueIdentifier,
                }
            };

            if (baseline.ItemzTypeId != Guid.Empty)
            {
                sqlParameters = sqlParameters.Append(new SqlParameter
                {
                    ParameterName = "ItemzTypeId",
                    Value = baseline.ItemzTypeId,
                    SqlDbType = System.Data.SqlDbType.UniqueIdentifier,
                }).ToArray();
            }
            sqlParameters = sqlParameters.Append(
                new SqlParameter
                {
                    ParameterName = "Name",
                    Size = 128,
                    Value = baseline.Name ?? Convert.DBNull,
                    SqlDbType = System.Data.SqlDbType.NVarChar,
                }).ToArray();

            sqlParameters = sqlParameters.Append(
                new SqlParameter
                {
                    ParameterName = "Description",
                    Size = 10280,
                    Value = baseline.Description ?? Convert.DBNull,
                    SqlDbType = System.Data.SqlDbType.NVarChar,
                }).ToArray();

            sqlParameters = sqlParameters.Append(OUTPUT_ID).ToArray();

            if (baseline.ItemzTypeId == Guid.Empty)
            {
                var _ = await _baselineContext.Database.ExecuteSqlRawAsync(sql: "EXEC userProcCreateBaselineByProjectID  @ProjectId, @Name, @Description, @OUTPUT_Id  = @OUTPUT_Id OUT", parameters: sqlParameters);
                returnValue = (Guid)OUTPUT_ID.Value;
            }
            else
            {
                var _ = await _baselineContext.Database.ExecuteSqlRawAsync(sql: "EXEC userProcCreateBaselineByItemzTypeID  @ProjectId, @ItemzTypeID, @Name, @Description, @OUTPUT_Id  = @OUTPUT_Id OUT", parameters: sqlParameters);
                returnValue = (Guid)OUTPUT_ID.Value;
            }
            return returnValue;
        }

        public async Task<Guid> CloneBaselineAsync(NonEntity_CloneBaseline cloneBaseline)
        {
            if (cloneBaseline == null)
            {
                throw new ArgumentNullException(nameof(cloneBaseline));
            }

            if (cloneBaseline.Name is null || cloneBaseline.Name == string.Empty)
            {
                throw new ArgumentNullException(nameof(cloneBaseline.Name));
            }

            if (cloneBaseline.BaselineId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(cloneBaseline.BaselineId));
            }

            if (!_baselineContext.Baseline!.Where(b => b.Id == cloneBaseline.BaselineId).Any())
            {
                throw new ArgumentException(nameof(cloneBaseline.BaselineId));
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
                    ParameterName = "BaselineId",
                    Value = cloneBaseline.BaselineId,
                    SqlDbType = System.Data.SqlDbType.UniqueIdentifier,
                }
            };

            sqlParameters = sqlParameters.Append(
                new SqlParameter
                {
                    ParameterName = "Name",
                    Size = 128,
                    Value = cloneBaseline.Name ?? Convert.DBNull,
                    SqlDbType = System.Data.SqlDbType.NVarChar,
                }).ToArray();

            sqlParameters = sqlParameters.Append(
                new SqlParameter
                {
                    ParameterName = "Description",
                    Size = 10280,
                    Value = cloneBaseline.Description ?? Convert.DBNull,
                    SqlDbType = System.Data.SqlDbType.NVarChar,
                }).ToArray();

            sqlParameters = sqlParameters.Append(OUTPUT_ID).ToArray();

            var _ = await _baselineContext.Database.ExecuteSqlRawAsync(sql: "EXEC userProcCreateBaselineByExistingBaselineID  @BaselineId, @Name, @Description, @OUTPUT_Id = @OUTPUT_Id OUT", parameters: sqlParameters);
            returnValue = (Guid)OUTPUT_ID.Value;

            return returnValue;
        }

        public async Task DeleteOrphanedBaselineItemzAsync()
        {
            await _baselineContext.Database.ExecuteSqlRawAsync(sql: "EXEC userProcDeleteAllOrphanedBaselineItemz");
        }

        public async Task<bool> SaveAsync()
        {
            return (await _baselineContext.SaveChangesAsync() >= 0);
        }

        public async Task<bool> BaselineExistsAsync(Guid baselineId)
        {
            if (baselineId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(baselineId));
            }

            // EXPLANATION: We expect BaselineExists to be used independently on it's own without
            // expecting it to track the Baseline that was found in the database. That's why it's not
            // a good idea to use "!(_baselineContext.Baseline.Find(baselineId) == null)" option
            // to "Find()" Baseline. This is because Find is designed to track the Baseline in the memory.
            // In "Baseline Delete controller method", we are first checking if BaselineExists and then 
            // we call Baseline Delete to actually remove it. This is going to be in the single scoped
            // DBContext. If we use "Find()" method then it will start tracking the Baseline and then we can't
            // get the Baseine once again from the DB as it's already being tracked. We have a choice here
            // to decide if we should always use Find via BaselineExists and then yet again in the subsequent
            // operations like Delete / Update or we use BaselineExists as independent method and not rely on 
            // it for subsequent operations like Delete / Update.

            return await _baselineContext.Baseline.AsNoTracking().AnyAsync(b => b.Id == baselineId);
        }

        public void UpdateBaseline(Baseline baseline)
        {
            // Due to Repository Pattern implementation, 
            // there is no code in this implementation. 
        }

		public async Task<BaselineItemzHierarchy?> GetBaselineHierarchyRecordForUpdateAsync(Guid baselineId)
		{
			if (baselineId == Guid.Empty)
			{
				throw new ArgumentNullException(nameof(baselineId));
			}

			// Single query to fetch the hierarchy record
			var hierarchyRecord = await _baselineContext.BaselineItemzHierarchy!
				.SingleOrDefaultAsync(bih => bih.Id == baselineId);

			if (hierarchyRecord == null)
			{
				return null; // Controller can handle not found
			}

			// Prevent updates to repository root records
			if (hierarchyRecord.RecordType.Equals("repository", StringComparison.OrdinalIgnoreCase)) // TODO :: Use Constants instead of Text
			{
				throw new ApplicationException($"Can not update name of the Repository Root Baseline Hierarchy Record with ID {baselineId}");
			}

			if (hierarchyRecord.BaselineItemzHierarchyId!.GetLevel() == 0) // TODO :: Use Constants instead of Text
			{
				throw new ApplicationException($"Can not update name of the Repository Root Baseline Hierarchy Record with ID {baselineId}");
			}

			return hierarchyRecord;
		}


		public void DeleteBaseline(Baseline baseline)
        {
            _baselineContext.Baseline!.Remove(baseline);
  
            // EXPLANATION: We need to first get the BaselineItemzHierarchy record from the repository
            // then we use it within IsDescendantOf to find all the child nodes including Baseline node itself
            // and then we remove all those records using EF Core entity tracking feature.

            var baselineHierarchyRecord = _baselineContext.BaselineItemzHierarchy!.AsNoTracking()
                .Where(bih => bih.Id == baseline.Id).FirstOrDefault();

            if (baselineHierarchyRecord != null)
            {
                _baselineContext.BaselineItemzHierarchy!.RemoveRange
                    (
                        _baselineContext.BaselineItemzHierarchy.Where
                        (
                            bi => bi.BaselineItemzHierarchyId!.IsDescendantOf
                            (
                                baselineHierarchyRecord!.BaselineItemzHierarchyId
                            )
                        ).AsEnumerable()
                    );
            }
            // TODO: If we can't automatically remove Trace data using this method then it's better to remove all data using
            // stored procedure for deleting Baseline. This will be similar approach when it comes to removing Single Itemz like
            // userProcDeleteSingleItemzByItemzID STORED PROCEDURE.

        }

        public async Task<int> GetBaselineItemzCountByBaselineAsync(Guid BaselineId)
        {
            if (BaselineId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(BaselineId));
            }
            KeyValuePair<string, object>[] sqlArgs = new KeyValuePair<string, object>[]
            {
                new KeyValuePair<string, object>("@__BaselineId__", BaselineId.ToString()),
            };
            var foundItemzByBaseline = await _baselineContext.CountByRawSqlAsync(SQLStatements.SQLStatementFor_GetBaselineItemzCountByBaseline, sqlArgs);

            return foundItemzByBaseline;
        }

        public async Task<int> GetBaselineItemzTraceCountByBaselineAsync(Guid BaselineId)
        {
            if (BaselineId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(BaselineId));
            }
            KeyValuePair<string, object>[] sqlArgs = new KeyValuePair<string, object>[]
            {
                new KeyValuePair<string, object>("@__BaselineId__", BaselineId.ToString()),
                new KeyValuePair<string, object>("@__IsIncluded_IsTrue__", 1),
            };
            var foundBaselineItemzTraceByBaseline = await _baselineContext.CountByRawSqlAsync(SQLStatements.SQLStatementFor_GetBaselineItemzTraceCountByBaseline, sqlArgs);

            return foundBaselineItemzTraceByBaseline;
        }

        public async Task<int> GetIncludedBaselineItemzCountByBaselineAsync(Guid BaselineId)
        {
            if (BaselineId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(BaselineId));
            }
            KeyValuePair<string, object>[] sqlArgs = new KeyValuePair<string, object>[]
            {
                new KeyValuePair<string, object>("@__BaselineId__", BaselineId.ToString()),
            };
            var foundIncludedItemzByBaseline = await _baselineContext.CountByRawSqlAsync(SQLStatements.SQLStatementFor_GetIncludedBaselineItemzCountByBaseline, sqlArgs);

            return foundIncludedItemzByBaseline;
        }


        public async Task<int> GetExcludedBaselineItemzCountByBaselineAsync(Guid BaselineId)
        {
            if (BaselineId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(BaselineId));
            }
            KeyValuePair<string, object>[] sqlArgs = new KeyValuePair<string, object>[]
            {
                new KeyValuePair<string, object>("@__BaselineId__", BaselineId.ToString()),
            };
            var foundExcludedItemzByBaseline = await _baselineContext.CountByRawSqlAsync(SQLStatements.SQLStatementFor_GetExcludedBaselineItemzCountByBaseline, sqlArgs);

            return foundExcludedItemzByBaseline;
        }

        public async Task<int> GetOrphanedBaselineItemzCount()
        {
            KeyValuePair<string, object>[] sqlArgs = new KeyValuePair<string, object>[0];
            var foundTotalBaselineItemzInRepository = await _baselineContext.CountByRawSqlAsync(SQLStatements.SQLStatementFor_GetOrphanedBaselineItemzCount, sqlArgs);
            return foundTotalBaselineItemzInRepository;
        }

        public async Task<int> GetItemzCountByItemzTypeAsync(Guid ItemzTypeId)
        {
            if (ItemzTypeId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(ItemzTypeId));
            }
            //KeyValuePair<string, object>[] sqlArgs = new KeyValuePair<string, object>[]
            //{
            //    new KeyValuePair<string, object>("@__ItemzTypeId__", ItemzTypeId.ToString()),
            //};
            //var foundItemzByItemzType = await _baselineContext.CountByRawSqlAsync(SQLStatements.SQLStatementFor_GetItemzCountByItemzType, sqlArgs);

            //return foundItemzByItemzType;

            var rootItemzType = _itemzContext.ItemzHierarchy!.AsNoTracking()
                .Where(ih => ih.Id == ItemzTypeId).FirstOrDefault();

            return (await _itemzContext.ItemzHierarchy!
                   .AsNoTracking()
                   .Where(ih => ih.ItemzHierarchyId!.IsDescendantOf(rootItemzType!.ItemzHierarchyId))
                   .CountAsync())
                   - 1; // Minus One becaues Count includes ItemzType itself along with it's SubItemz
        }

        public async Task<int> GetItemzCountByProjectAsync(Guid ProjectId)
        {
            if (ProjectId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(ProjectId));
            }
            //KeyValuePair<string, object>[] sqlArgs = new KeyValuePair<string, object>[]
            //{
            //    new KeyValuePair<string, object>("@__ProjectID__", ProjectId.ToString()),
            //};
            //var foundItemzByProject = await _baselineContext.CountByRawSqlAsync(SQLStatements.SQLStatementFor_GetItemzCountByProject, sqlArgs);

            // return foundItemzByProject;

            var rootProject = _itemzContext.ItemzHierarchy!.AsNoTracking()
                            .Where(ih => ih.Id == ProjectId).FirstOrDefault();

            //var sumOfProjectAndSubItemzType = (await _itemzContext.ItemzHierarchy!
            //       .AsNoTracking()
            //       .Where(ih => ih.ItemzHierarchyId!.GetAncestor(1) == rootProject!.ItemzHierarchyId!)
            //       .CountAsync());

            //return (await _itemzContext.ItemzHierarchy!
            //       .AsNoTracking()
            //       .Where(ih => ih.ItemzHierarchyId!.IsDescendantOf(rootProject!.ItemzHierarchyId))
            //       .CountAsync())
            //       - sumOfProjectAndSubItemzType; // Minus sum of Project itself and it's sub ItemzType Hierarchy records

            return (await _itemzContext.ItemzHierarchy!
                   .AsNoTracking()
                   .Where(ih => ih.ItemzHierarchyId!.IsDescendantOf(rootProject!.ItemzHierarchyId) 
                            && ih.ItemzHierarchyId!.GetLevel() > 2) // This way we only count for Itemz Hierarchy records
                   .CountAsync());

		}

        public async Task<int> GetTotalBaselineItemzCountAsync()
        {
            KeyValuePair<string, object>[] sqlArgs = new KeyValuePair<string, object>[0];
            var foundTotalBaselineItemzInRepository = await _baselineContext.CountByRawSqlAsync(SQLStatements.SQLStatementFor_GetTotalBaselineItemzInRepository, sqlArgs);
            return foundTotalBaselineItemzInRepository;
        }

        public async Task<int> GetBaselineCountByProjectIdAsync(Guid projectID)
        {
            var foundBaselineCount = -1;
            foundBaselineCount = await _baselineContext.Baseline!
                .Where(b => b!.ProjectId == projectID)
                .TagWith("BaselineCountByProjectID")
                .AsNoTracking()
                .CountAsync();
            return foundBaselineCount > 0 ? foundBaselineCount : -1;
        }

        public async Task<IEnumerable<Baseline>?> GetBaselinesByProjectIdAsync(Guid ProjectId)
        {
            if (ProjectId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(ProjectId));
            }

            if (await _baselineContext.Baseline!.Where(b => b!.ProjectId == ProjectId).AnyAsync())
            {
                return await _baselineContext.Baseline
                    .AsNoTracking()
                    .Where(b => b!.ProjectId == ProjectId)
                    .AsQueryable<Baseline>()
                    .OrderBy(b => b.CreatedDate)
                    .ToListAsync();
            }
            return null;
        }

        public async Task<bool> ProjectExistsAsync(Guid projectId)
        {
            if (projectId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(projectId));
            }

            // EXPLANATION: We expect ProjectExists to be used independently on it's own without
            // expecting it to track the Project that was found in the database. That's why it's not
            // a good idea to use "!(_baselineContext.Projects.Find(projectId) == null)" option
            // to "Find()" Project. This is because Find is designed to track the Project in the memory.
            // This is going to be in the single scoped DBContext.
            // If we use "Find()" method then it will start tracking the Project and then we can't
            // get the Project once again from the DB as it's already being tracked. We have a choice here
            // to decide if we should always use Find via ProjectExists and then yet again in the subsequent
            // operations like Delete / Update or we use ProjectExists as independent method and not rely on 
            // it for subsequent operations like Delete / Update.

            return await _baselineContext.Projects.AsNoTracking().AnyAsync(p => p.Id == projectId);
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

        public async Task<bool> HasBaselineWithNameAsync(Guid projectId, string baselineName)
        {
            return await _baselineContext.Baseline.AsNoTracking().AnyAsync(b=> b.ProjectId.ToString().ToLower() == projectId.ToString().ToLower() && b.Name!.ToLower() == baselineName.ToLower());
        }
    }
}
