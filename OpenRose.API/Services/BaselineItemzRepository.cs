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
