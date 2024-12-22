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
using ItemzApp.API.Helper;
using ItemzApp.API.ResourceParameters;

namespace ItemzApp.API.Services
{
    public class BaselineItemzTypeRepository : IBaselineItemzTypeRepository, IDisposable
    {
        private readonly BaselineContext _baselineContext;
        private readonly IPropertyMappingService _propertyMappingService;
        public BaselineItemzTypeRepository(BaselineContext baselineContext,
            IPropertyMappingService propertyMappingService)
        {
            _baselineContext = baselineContext ?? throw new ArgumentNullException(nameof(baselineContext));
            _propertyMappingService = propertyMappingService ??
               throw new ArgumentNullException(nameof(propertyMappingService));
        }
        public async Task<BaselineItemzType?> GetBaselineItemzTypeAsync(Guid BaselineItemzTypeId)
        {
            if (BaselineItemzTypeId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(BaselineItemzTypeId));
            }

            return await _baselineContext.BaselineItemzType!
                .Where(bit => bit.Id == BaselineItemzTypeId).AsNoTracking().FirstOrDefaultAsync();
        } 

        //public async Task<Baseline?> GetBaselineForUpdateAsync(Guid BaselineId)
        //{
        //    if (BaselineId == Guid.Empty)
        //    {
        //        throw new ArgumentNullException(nameof(BaselineId));
        //    }

        //    return await _baselineContext.Baseline!
        //        .Where(b => b.Id == BaselineId).FirstOrDefaultAsync();
        //}

        public async Task<IEnumerable<BaselineItemzType>?> GetBaselineItemzTypesAsync()
        {
            try
            {
                if (await _baselineContext.BaselineItemzType.CountAsync<BaselineItemzType>() > 0)
                {
                    var baselineItemzTypeCollection = await _baselineContext.BaselineItemzType.AsNoTracking().AsQueryable<BaselineItemzType>().OrderBy(b => b.Name).ToListAsync();

                    // TODO: We have to create simple implementation of sort by BaselineItemzType Name here 
                    // baselineItemzTypeCollection = baselineItemzTypeCollection.ApplySort("Name", null).AsNoTracking();

                    return baselineItemzTypeCollection;
                }
                return null;
            }
            catch (Exception ex)
            {
                // TODO: It's not good that we capture Generic Exception and then 
                // return null here. Basically, I wanted to check if we have 
                // baselineItemzTypes returned from the DB and if it does not then
                // it should simply return null back to the calling function.
                // One has to learn how to do this gracefully as part of Entity Framework 
                return null;
            }
        }

        //public async Task<IEnumerable<BaselineItemzType>?> GetBaselineItemzTypesAsync(Guid BaselineId)
        //{
        //    if (BaselineId == Guid.Empty)
        //    {
        //        throw new ArgumentNullException(nameof(BaselineId));
        //    }

        //    if (await _baselineContext.BaselineItemzType!.Where(bit => bit.BaselineId == BaselineId).AnyAsync())
        //    {
        //        return await _baselineContext.BaselineItemzType
        //            .AsNoTracking()
        //            .Where(bit => bit.BaselineId == BaselineId)
        //            .AsQueryable<BaselineItemzType>()
        //            .OrderBy(bit => bit.Name)
        //            .ToListAsync();
        //    }
        //    return null;
        //}

        // TODO: decide if we need GetBaseline by passing in collection of baselineIds
        // if yes, then we need to implement action method in BaselineController for the same
        // so that Swagger docs shows GET method under Baselines section.
        public async Task<IEnumerable<BaselineItemzType>> GetBaselineItemzTypesAsync(IEnumerable<Guid> baselineItemzTypeIds)
        {
            if (baselineItemzTypeIds == null)
            {
                throw new ArgumentNullException(nameof(baselineItemzTypeIds));
            }

            return await _baselineContext.BaselineItemzType.AsNoTracking().Where(bit => baselineItemzTypeIds.Contains(bit.Id))
                .OrderBy(bit => bit.Name)
                .ToListAsync();
        }

        //public async Task AddBaselineAsync(Baseline baseline)
        //{
        //    if (baseline == null)
        //    {
        //        throw new ArgumentNullException(nameof(baseline));
        //    }

        //    if (baseline.Name is null || baseline.Name == string.Empty)
        //    {
        //        throw new ArgumentNullException(nameof(baseline.Name));
        //    }

        //    if (baseline.ProjectId == Guid.Empty)
        //    {
        //        throw new ArgumentNullException(nameof(baseline.ProjectId));
        //    }

        //    if (!_baselineContext.Projects!.Where(p => p.Id == baseline.ProjectId).Any())
        //    {
        //        throw new ArgumentException(nameof(baseline.ProjectId));
        //    }
        //    var sqlParameters = new[]
        //    {
        //        new SqlParameter
        //        {
        //            ParameterName = "ProjectId",
        //            Value = baseline.ProjectId,
        //            SqlDbType = System.Data.SqlDbType.UniqueIdentifier,
        //        },                new SqlParameter
        //        {
        //            ParameterName = "Name",
        //            Size = 128,
        //            Value = baseline.Name ?? Convert.DBNull,
        //            SqlDbType = System.Data.SqlDbType.NVarChar,
        //        },
        //        new SqlParameter
        //        {
        //            ParameterName = "Description",
        //            Size = 1028,
        //            Value = baseline.Description ?? Convert.DBNull,
        //            SqlDbType = System.Data.SqlDbType.NVarChar,
        //        }
        //    };
        //    var _ = await _baselineContext.Database.ExecuteSqlRawAsync(sql: "EXEC userProcCreateBaselineByProjectID  @ProjectId, @Name, @Description", parameters: sqlParameters);
        //}

        //public async Task<bool> SaveAsync()
        //{
        //    return (await _baselineContext.SaveChangesAsync() >= 0);
        //}

        public async Task<bool> BaselineItemzTypeExistsAsync(Guid baselineItemzTypeId)
        {
            if (baselineItemzTypeId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(baselineItemzTypeId));
            }
            return await _baselineContext.BaselineItemzType.AsNoTracking().AnyAsync(bit => bit.Id == baselineItemzTypeId);
        }

        //public void UpdateBaseline(Baseline baseline)
        //{
        //    // Due to Repository Pattern implementation, 
        //    // there is no code in this implementation. 
        //}

        //public void DeleteBaseline(Baseline baseline)
        //{
        //    _baselineContext.Baseline!.Remove(baseline);
        //}

        public async Task<int> GetBaselineItemzCountByBaselineItemzTypeAsync(Guid BaselineItemzTypeId)
        {
            if (BaselineItemzTypeId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(BaselineItemzTypeId));
            }
            KeyValuePair<string, object>[] sqlArgs = new KeyValuePair<string, object>[]
            {
                new KeyValuePair<string, object>("@__BaselineItemzTypeId__", BaselineItemzTypeId.ToString()),
            };
            var foundItemzByBaselineItemzType = await _baselineContext.CountByRawSqlAsync(SQLStatements.SQLStatementFor_GetBaselineItemzCountByBaselineItemzType, sqlArgs);

            return foundItemzByBaselineItemzType;
        }

        public PagedList<BaselineItemz>? GetBaselineItemzsByBaselineItemzType(Guid baselineItemzTypeId, ItemzResourceParameter itemzResourceParameter)
        {
            // TODO: Should we check for itemzResourceParameter being null?
            // There are chances that we just want to get all the itemz and
            // consumer of the API might now pass in necessary values for pagging.

            // TODO: Make this method Async.

            if (itemzResourceParameter == null)
            {
                throw new ArgumentNullException(nameof(itemzResourceParameter));
            }

            if (baselineItemzTypeId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(baselineItemzTypeId));
            }
            try
            {
                if ( _baselineContext.BaselineItemz!.Count<BaselineItemz>() > 0 )
                {
                    var baselineItemzCollection = _baselineContext.BaselineItemz 
                        .Include(bi => bi.BaselineItemzTypeJoinBaselineItemz )
                        //                        .ThenInclude(PjI => PjI.ItemzType)
                        .Where(bi => bi.BaselineItemzTypeJoinBaselineItemz!.Any(bitjbi => bitjbi.BaselineItemzTypeId == baselineItemzTypeId));

                    //     .Where(i => i.  . AsQueryable<Itemz>(); // as IQueryable<Itemz>;

                    if (!string.IsNullOrWhiteSpace(itemzResourceParameter.OrderBy))
                    {
                        var itemzPropertyMappingDictionary =
                                               _propertyMappingService.GetPropertyMapping<Models.GetBaselineItemzDTO, BaselineItemz>();

                        baselineItemzCollection = baselineItemzCollection.ApplySort(itemzResourceParameter.OrderBy,
                            itemzPropertyMappingDictionary).AsNoTracking();
                    }

                    // EXPLANATION: Pagging feature should be implemented at the end 
                    // just before calling ToList. This will make sure that any filtering,
                    // sorting, grouping, etc. that we implement on the data are 
                    // put in place before calling ToList. 

                    return PagedList<BaselineItemz>.Create(baselineItemzCollection,
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

        //public async Task<bool> ProjectExistsAsync(Guid projectId)
        //{
        //    if (projectId == Guid.Empty)
        //    {
        //        throw new ArgumentNullException(nameof(projectId));
        //    }

        //    // EXPLANATION: We expect ProjectExists to be used independently on it's own without
        //    // expecting it to track the Project that was found in the database. That's why it's not
        //    // a good idea to use "!(_baselineContext.Projects.Find(projectId) == null)" option
        //    // to "Find()" Project. This is because Find is designed to track the Project in the memory.
        //    // This is going to be in the single scoped DBContext.
        //    // If we use "Find()" method then it will start tracking the Project and then we can't
        //    // get the Project once again from the DB as it's already being tracked. We have a choice here
        //    // to decide if we should always use Find via ProjectExists and then yet again in the subsequent
        //    // operations like Delete / Update or we use ProjectExists as independent method and not rely on 
        //    // it for subsequent operations like Delete / Update.

        //    return await _baselineContext.Projects.AsNoTracking().AnyAsync(p => p.Id == projectId);
        //}

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

        public async Task<bool> HasBaselineItemzTypeWithNameAsync(Guid baselineId, string baselineItemzTypeName)
        {
            return await _baselineContext.BaselineItemzType.AsNoTracking().AnyAsync(bit=> bit.BaselineId.ToString().ToLower() == baselineId.ToString().ToLower() && bit.Name!.ToLower() == baselineItemzTypeName.ToLower());
        }
    }
}
