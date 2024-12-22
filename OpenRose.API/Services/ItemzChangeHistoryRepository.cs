// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using ItemzApp.API.DbContexts;
using ItemzApp.API.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ItemzApp.API.Services
{
    public class ItemzChangeHistoryRepository : IItemzChangeHistoryRepository, IDisposable
    {
        private readonly ItemzChangeHistoryContext _context;

        public ItemzChangeHistoryRepository(ItemzChangeHistoryContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context)); 
        }

        public async Task<IEnumerable<ItemzChangeHistory>> GetItemzChangeHistoryAsync(Guid ItemzId)
        {
            if (ItemzId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(ItemzId));
            }

            return await _context.ItemzChangeHistory!
                .Where(ich => ich.ItemzId == ItemzId)
                .OrderByDescending(ich => ich.CreatedDate).AsNoTracking().ToListAsync();
        }
        /// <summary>
        /// Used for deleting Itemz Change History based on ItemzId and provided Date and Time as Cut-off for deletion
        /// 
        /// If no records are deleted then we return ZERO integer value. This is to make this method behave as an idempotent method.
        /// </summary>
        /// <param name="ItemzId">GUID representing ItemzID for which Change History records should be deleted</param>
        /// <param name="DeleteUptoDateTime">Date time value which is treated as Cut-Off for identifying records that should be deleted</param>
        /// <returns>Integer value indicating number of itemz change history records that are deleted.</returns>
        public async Task<int> DeleteItemzChangeHistoryAsync(Guid ItemzId, DateTimeOffset? DeleteUptoDateTime = null)
        {
            if (DeleteUptoDateTime == null)
            {
                return 0;
            }
            var numberOfItemzChangeHistoryToBeRemoved = 0;
            var foundItemzChangeHistory = _context.ItemzChangeHistory!.Where(ich => ich.ItemzId == ItemzId);
            if (foundItemzChangeHistory.Count() > 0)
            {
                foreach (var each_fich in foundItemzChangeHistory)
                {
                    if (each_fich.CreatedDate < DeleteUptoDateTime)
                    {
                        _context.Remove(each_fich);
                        numberOfItemzChangeHistoryToBeRemoved += 1;
                    }
                }
            }
            if(numberOfItemzChangeHistoryToBeRemoved > 0)
            {
                await _context.SaveChangesAsync();
                return numberOfItemzChangeHistoryToBeRemoved;
            }
            return 0;
        }

        public async Task<bool> SaveAsync()
        {
            return (await _context.SaveChangesAsync() >= 0);
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
