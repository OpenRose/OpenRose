// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using ItemzApp.API.DbContexts;
using System;
using ItemzApp.API.DbContexts.Extensions;
using ItemzApp.API.DbContexts.SQLHelper;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ItemzApp.API.Services
{
    public class ItemzChangeHistoryByProjectRepository : IItemzChangeHistoryByProjectRepository, IDisposable
    {
        private readonly ItemzChangeHistoryContext _itemzChangeHistoryContext;
        private readonly ItemzContext _itemzContext;

        public ItemzChangeHistoryByProjectRepository(ItemzChangeHistoryContext itemzChangeHistoryContext,
            ItemzContext itemzContext)
        {
            _itemzChangeHistoryContext = itemzChangeHistoryContext ?? throw new ArgumentNullException(nameof(itemzChangeHistoryContext));
            _itemzContext = itemzContext ?? throw new ArgumentNullException(nameof(itemzContext));
        }

        /// <summary>
        /// Used for deleting Itemz Change History by Itemz Type based on ProjectId and provided Date and Time as Cut-off for deletion
        /// 
        /// If no records are deleted then we return ZERO integer value. This is to make this method behave as an idempotent method.
        /// </summary>
        /// <param name="ProjectId">GUID representing ProjectID for which all associated Itemz's Change History records should be deleted</param>
        /// <param name="deleteUptoDateTime">Date time value which is treated as Cut-Off for identifying records that should be deleted</param>
        /// <returns>Integer value indicating number of itemz change history records that are deleted within Project.</returns>
        public async Task<int> DeleteItemzChangeHistoryByProjectAsync(Guid ProjectId, DateTimeOffset? deleteUptoDateTime = null)
        {
            if (deleteUptoDateTime == null)
            {
                return 0;
            }
            var numberOfItemzChangeHistoryToBeRemoved = 0;

            var foundProjectItemzTypes = await _itemzContext.Projects
                .AsNoTracking()
                .Where(p => p.Id == ProjectId)
                .Select(p => p.ItemzTypes).FirstOrDefaultAsync();

            if (foundProjectItemzTypes is null)
            {
                return 0;
            }
            foreach (var foundProjectItemzType in foundProjectItemzTypes)
            {
                var foundItemzTypeItemzs = await _itemzContext.ItemzTypeJoinItemz
                            .AsNoTracking()
                            .Where(itji => itji.ItemzTypeId == foundProjectItemzType.Id)
                            .Select(itji => itji.ItemzId)
                            .ToListAsync();
                foreach (var foundItemzTypeItemz in foundItemzTypeItemzs)
                {
                    // TODO : Here we are iterating through all the associated Itemz that are part of given ProjectID
                    // and then sending queries to database for each itemz that is found. This could really be large number
                    // of SQL Queries as well as multiple back and forth between database. 
                    // Also consider that every Itemz that are associated with Project may not have 
                    // change history record associated with it. That means we might just dry run iteration
                    // with many SQL queries that checkes if Project has associated change history.
                    // We have opportunity to improve this process further.

                    var foundItemzChangeHistory = _itemzChangeHistoryContext.ItemzChangeHistory!.Where(ich => ich.ItemzId == foundItemzTypeItemz);
                    if (foundItemzChangeHistory.Count() > 0)
                    {
                        foreach (var each_fich in foundItemzChangeHistory)
                        {
                            if (each_fich.CreatedDate < deleteUptoDateTime)
                            {
                                _itemzChangeHistoryContext.Remove(each_fich);
                                numberOfItemzChangeHistoryToBeRemoved += 1;
                            }
                        }
                    }
                }
            }
            if (numberOfItemzChangeHistoryToBeRemoved > 0)
            {
                await _itemzChangeHistoryContext.SaveChangesAsync();
                return numberOfItemzChangeHistoryToBeRemoved;
            }
            return 0;
        }

        public async Task<int> TotalNumberOfItemzChangeHistoryByProjectAsync(Guid ProjectId)
        {
            if (ProjectId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(ProjectId));
            }
            KeyValuePair<string, object>[] sqlArgs = new KeyValuePair<string, object>[]
            {
                new KeyValuePair<string, object>("@__ProjectId__", ProjectId.ToString()),
            };
            var foundItemzChangeHistory = await _itemzChangeHistoryContext.CountByRawSqlAsync(SQLStatements.SQLStatementFor_ItemzChangeHistoryByProject, sqlArgs);

            return foundItemzChangeHistory;
        }

        public async Task<int> TotalNumberOfItemzChangeHistoryByProjectUptoDateTimeAsync(Guid ProjectId, DateTimeOffset? GetUptoDateTime = null)
        {
            if (ProjectId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(ProjectId));
            }
            if (GetUptoDateTime == null)
            {
                throw new ArgumentNullException(nameof(GetUptoDateTime));
            }

            KeyValuePair<string, object>[] sqlArgs = new KeyValuePair<string, object>[]
            {
                new KeyValuePair<string, object>("@__GetUptoDateTime__", GetUptoDateTime.Value),
                new KeyValuePair<string, object>("@__ProjectId__", ProjectId.ToString()),
            };
            var foundItemzChangeHistory = await _itemzChangeHistoryContext.CountByRawSqlAsync(SQLStatements.SQLStatementFor_ItemzChangeHistoryByProjectWithUptoDateTime, sqlArgs);

            return foundItemzChangeHistory;
        }
        public async Task<bool> SaveAsync()
        {
            return (await _itemzChangeHistoryContext.SaveChangesAsync() >= 0);
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
