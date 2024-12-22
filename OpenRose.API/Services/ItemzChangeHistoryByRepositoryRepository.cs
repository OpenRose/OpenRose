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
    public class ItemzChangeHistoryByRepositoryRepository : IItemzChangeHistoryByRepositoryRepository, IDisposable
    {
        private readonly ItemzChangeHistoryContext _itemzChangeHistoryContext;

        public ItemzChangeHistoryByRepositoryRepository(ItemzChangeHistoryContext itemzChangeHistoryContext)
        {
            _itemzChangeHistoryContext = itemzChangeHistoryContext ?? throw new ArgumentNullException(nameof(itemzChangeHistoryContext));
        }
        public async Task<int> TotalNumberOfItemzChangeHistoryByRepositoryAsync()
        {
            var foundItemzChangeHistory = await _itemzChangeHistoryContext.CountByRawSqlAsync(SQLStatements.SQLStatementFor_ItemzChangeHistoryByRepository);

            return foundItemzChangeHistory;
        }

        public async Task<int> TotalNumberOfItemzChangeHistoryByRepositoryUptoDateTimeAsync(DateTimeOffset? GetUptoDateTime = null)
        {

            if (GetUptoDateTime == null)
            {
                throw new ArgumentNullException(nameof(GetUptoDateTime));
            }

            KeyValuePair<string, object>[] sqlArgs = new KeyValuePair<string, object>[]
            {
                new KeyValuePair<string, object>("@__GetUptoDateTime__", GetUptoDateTime.Value),
            };
            var foundItemzChangeHistory = await _itemzChangeHistoryContext.CountByRawSqlAsync(SQLStatements.SQLStatementFor_ItemzChangeHistoryByRepositoryWithUptoDateTime, sqlArgs);

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
