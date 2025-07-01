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


namespace ItemzApp.API.Services
{
    public class ItemzTraceRepository : IItemzTraceRepository, IDisposable
    {
        private readonly ItemzContext _context;
        private readonly ItemzTraceContext _itemzTraceContext;
        private readonly IPropertyMappingService _propertyMappingService;
        public ItemzTraceRepository(ItemzContext context,
                                        ItemzTraceContext itemzTraceContext,
                                        IPropertyMappingService propertyMappingService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _itemzTraceContext = itemzTraceContext ?? throw new ArgumentNullException(nameof(itemzTraceContext));
            _propertyMappingService = propertyMappingService ??
                throw new ArgumentNullException(nameof(propertyMappingService));
        }


        public async Task<bool> SaveAsync()
        {
            return (await _itemzTraceContext.SaveChangesAsync() >= 0);
        }
        public async Task<IEnumerable<ItemzJoinItemzTrace>> GetAllTracesByItemzIdAsync(Guid itemzId)
        {
            if (itemzId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(itemzId));
            }

            return await _itemzTraceContext.ItemzJoinItemzTrace!
                .Where(ijit => ijit.FromItemzId == itemzId || ijit.ToItemzId == itemzId).AsNoTracking().ToListAsync();
        }

        public async Task<ItemzParentAndChildTraceDTO> GetAllParentAndChildTracesByItemzIdAsync(Guid itemzId)
        {
            if (itemzId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(itemzId));
            }

            var itemzParentAndChildTraceDTO = new ItemzParentAndChildTraceDTO();

            itemzParentAndChildTraceDTO.Itemz = new();
            itemzParentAndChildTraceDTO.Itemz.ChildItemz = new();
            itemzParentAndChildTraceDTO.Itemz.ParentItemz = new();
            itemzParentAndChildTraceDTO.Itemz.ID = itemzId;

            var allChildTraceItemzs = _itemzTraceContext.ItemzJoinItemzTrace!
                .Where(ijit => ijit.FromItemzId == itemzId);

            foreach (var childTraceItemz in allChildTraceItemzs)
            {
                var tempChildTraceItemzDTO = new ChildTraceItemz__DTO();
                tempChildTraceItemzDTO.ItemzID = childTraceItemz.ToItemzId;
                itemzParentAndChildTraceDTO.Itemz.ChildItemz.Add(tempChildTraceItemzDTO);
            }

            var allParentTraceItemzs = _itemzTraceContext.ItemzJoinItemzTrace!
                .Where(ijit => ijit.ToItemzId == itemzId);

            foreach (var parentTraceItemz in allParentTraceItemzs)
            {
                var tempParentTraceItemzDTO = new ParentTraceItemz__DTO();
                tempParentTraceItemzDTO.ItemzID = parentTraceItemz.FromItemzId;
                itemzParentAndChildTraceDTO.Itemz.ParentItemz.Add(tempParentTraceItemzDTO);
            }

            return itemzParentAndChildTraceDTO;
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

        /// <summary>
        /// Purpose of this method is to check if Trace is already found between FromItemz and ToItemz
        /// </summary>
        /// <param name="itemzTraceDTO"></param>
        /// <returns></returns>

        public async Task<bool> ItemzsTraceExistsAsync(ItemzTraceDTO itemzTraceDTO)
        {
            if (itemzTraceDTO.FromTraceItemzId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(itemzTraceDTO.FromTraceItemzId));
            }

            if (itemzTraceDTO.ToTraceItemzId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(itemzTraceDTO.ToTraceItemzId));
            }

            return await _itemzTraceContext.ItemzJoinItemzTrace
                            .AsNoTracking()
                            .AnyAsync(itrace => itrace.FromItemzId == itemzTraceDTO.FromTraceItemzId
                                        && itrace.ToItemzId == itemzTraceDTO.ToTraceItemzId);
        }

        /// <summary>
        /// Purpose for this method is to allow creating new Itemz Trace and saving it.
        /// </summary>
        /// <param name="itemzTraceDTO"></param>
        public async Task EstablishTraceBetweenItemzAsync(ItemzTraceDTO itemzTraceDTO)
        {
            if (itemzTraceDTO.FromTraceItemzId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(itemzTraceDTO.FromTraceItemzId));
            }

            if (itemzTraceDTO.ToTraceItemzId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(itemzTraceDTO.ToTraceItemzId));
            }

            if (!(await _context.Itemzs.AsNoTracking().AnyAsync(a => a.Id == itemzTraceDTO.FromTraceItemzId)))
            {
                throw new Exception(nameof(itemzTraceDTO.FromTraceItemzId));
            }

            if (!(await _context.Itemzs.AsNoTracking().AnyAsync(a => a.Id == itemzTraceDTO.ToTraceItemzId)))
            {
                throw new Exception(nameof(itemzTraceDTO.ToTraceItemzId));
            }

            var itrace = await _itemzTraceContext.ItemzJoinItemzTrace!.FindAsync(itemzTraceDTO.FromTraceItemzId, itemzTraceDTO.ToTraceItemzId);
            if(itrace == null)
            {
                var temp_itrace = new ItemzJoinItemzTrace
                {
                    FromItemzId = itemzTraceDTO.FromTraceItemzId,
                    ToItemzId = itemzTraceDTO.ToTraceItemzId
                };
                await _itemzTraceContext.ItemzJoinItemzTrace.AddAsync(temp_itrace);
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

        public async Task<int> GetFromTraceCountByItemz(Guid itemzId)
        {
            return await _itemzTraceContext.ItemzJoinItemzTrace
                .Include(ijit => ijit.FromItemz)
                .Where(ijit => ijit.ToItemzId == itemzId).CountAsync();
        }

        public async Task<int> GetToTraceCountByItemz(Guid itemzId)
        {
            return await _itemzTraceContext.ItemzJoinItemzTrace
                .Include(ijit => ijit.ToItemz)
                .Where(ijit => ijit.FromItemzId == itemzId).CountAsync();
        }
        public async Task<int> RemoveAllFromItemzTraceAsync(Guid itemzId)
        {
            var fromItemzJoinTraceItemzList = await _itemzTraceContext.ItemzJoinItemzTrace!
                    // .Include(ijit => ijit.FromItemz) // This Include here will end up in Database SQL Query Error. Find out why in the futre?
                    .Where(ijit => ijit.ToItemzId == itemzId).AsNoTracking().ToListAsync();
            var countOfDeletedFromItemzJointItemzTrace = 0;
            foreach (var tempIJIT in fromItemzJoinTraceItemzList)
            {   
                // TODO: add aditional check that tempIJIT.ToItemzId == itemzID; // This way we make sure that we only pick-up links where provided itemzID is a To-Trace
                var itrace = await _itemzTraceContext.ItemzJoinItemzTrace!.FindAsync(tempIJIT.FromItemzId, itemzId);
                if (itrace != null)
                {
                    _itemzTraceContext.ItemzJoinItemzTrace.Remove(itrace);
                    countOfDeletedFromItemzJointItemzTrace++;
                }
            }
            return countOfDeletedFromItemzJointItemzTrace;
        }

        public async Task<int> RemoveAllToItemzTraceAsync(Guid itemzId)
        {
            var toItemzJoinTraceItemzList = await _itemzTraceContext.ItemzJoinItemzTrace!
                    // .Include(ijit => ijit.ToItemz) // This Include here will end up in Database SQL Query Error. Find out why in the futre?
                    .Where(ijit => ijit.FromItemzId == itemzId).AsNoTracking().ToListAsync();
            var countOfDeletedToItemzJointItemzTrace = 0;
            foreach(var tempIJIT in toItemzJoinTraceItemzList)
            {
                // TODO: add aditional check that tempIJIT.FromItemzId == itemzID; // This way we make sure that we only pick-up links where provided itemzID is a From-Trace

                var itrace = await _itemzTraceContext.ItemzJoinItemzTrace!.FindAsync(itemzId, tempIJIT.ToItemzId);
                if (itrace != null)
                {
                    _itemzTraceContext.ItemzJoinItemzTrace.Remove(itrace);
                    countOfDeletedToItemzJointItemzTrace++;
                }
            }
            return countOfDeletedToItemzJointItemzTrace;
        }


        public async Task<int> GetAllFromAndToTracesCountByItemzIdAsync(Guid itemzId)
        {
            if (itemzId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(itemzId));
            }

            return await _itemzTraceContext.ItemzJoinItemzTrace!
                    .Where(ijit => ijit.FromItemzId == itemzId || ijit.ToItemzId == itemzId).CountAsync();
        }

        public async Task<bool> RemoveItemzTraceAsync(ItemzTraceDTO itemzTraceDTO)
        {
            var itrace = await _itemzTraceContext.ItemzJoinItemzTrace!.FindAsync(itemzTraceDTO.FromTraceItemzId, itemzTraceDTO.ToTraceItemzId);
            if (itrace != null)
            {
                _itemzTraceContext.ItemzJoinItemzTrace.Remove(itrace);
                return true; // Found and removed
            }
            return false;  // Could not be found
        }

		public async Task<List<ItemzJoinItemzTrace>> GetAllTracesForItemzIdsAsync(IEnumerable<Guid> itemzIds)
		{
			var idSet = itemzIds.ToHashSet(); // for efficient lookup if needed
			return await _itemzTraceContext.ItemzJoinItemzTrace!
				.Where(t => idSet.Contains(t.FromItemzId) || idSet.Contains(t.ToItemzId))
				.AsNoTracking()
				.ToListAsync();
		}
	}
}
