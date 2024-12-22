// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ItemzApp.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ItemzApp.API.DbContexts.Interceptors
{
    public class ItemzContexInterceptor : ISaveChangesInterceptor
    {
        private IList<ItemzChangeHistory>? itemzChangeHistory;

        public readonly ItemzChangeHistoryContext _injectedItemzChangeHistoryContext;
        private readonly ILogger<ItemzContexInterceptor> _logger;

        public ItemzContexInterceptor(ItemzChangeHistoryContext InjectedItemzChangeHistoryContext,
            ILogger<ItemzContexInterceptor> logger)
        {
            _injectedItemzChangeHistoryContext = InjectedItemzChangeHistoryContext ?? throw new ArgumentNullException(nameof(InjectedItemzChangeHistoryContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        void ISaveChangesInterceptor.SaveChangesFailed(DbContextErrorEventData eventData)
        {
           
        }

        Task ISaveChangesInterceptor.SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        int ISaveChangesInterceptor.SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            if (itemzChangeHistory != null && itemzChangeHistory.Any()) 
            {
                foreach (var ich in itemzChangeHistory)
                {
                    _injectedItemzChangeHistoryContext.Add(ich);
                }
                _injectedItemzChangeHistoryContext.SaveChanges();
                _logger.LogDebug("{ITEMZ_CONTEX_INTERCEPTOR}Saved {NumberOfChanges} Change History Records in the database", "::ITEMZ_CONTEX_INTERCEPTOR:: ", itemzChangeHistory.Count());
                itemzChangeHistory.Clear();
            }
            return result;
        }

        async ValueTask<int> ISaveChangesInterceptor.SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken)
        {
            //if (itemzChangeHistory.Any())
            if ( itemzChangeHistory != null && itemzChangeHistory.Any())
            {
                foreach (var ich in itemzChangeHistory)
                {
                    _injectedItemzChangeHistoryContext.Add(ich);
                }
                _injectedItemzChangeHistoryContext.SaveChanges();
                _logger.LogDebug("{ITEMZ_CONTEX_INTERCEPTOR}Saved {NumberOfChanges} Change History Records in the database", "::ITEMZ_CONTEX_INTERCEPTOR:: ", itemzChangeHistory.Count());
                itemzChangeHistory.Clear();
            }
            return result;
        }

        InterceptionResult<int> ISaveChangesInterceptor.SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            itemzChangeHistory = CreateItemzChangeHistory(eventData.Context);
            return result;
        }

        async ValueTask<InterceptionResult<int>> ISaveChangesInterceptor.SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken)
        {
            itemzChangeHistory = CreateItemzChangeHistory(eventData.Context);
            return result;
        }

        #region CreateItemzChangeHistory
        private static IList<ItemzChangeHistory> CreateItemzChangeHistory(DbContext context)
        {
            context.ChangeTracker.DetectChanges();

            IList<ItemzChangeHistory> listOfItemzChangeHistory = new List<ItemzChangeHistory>();

            foreach (var entry in context.ChangeTracker.Entries().Where(e => e.State != EntityState.Deleted))
            {
                if (!entry.Entity.GetType().Equals(typeof(Itemz)))
                {
                    continue;
                }
                else if (entry.State != EntityState.Added && entry.State != EntityState.Modified)
                {
                    continue;
                }

                var itemzChangeHistory = new ItemzChangeHistory
                {
                    CreatedDate = DateTimeOffset.UtcNow,
                    ItemzId = (Guid)entry.Properties.Where(property => property.Metadata.IsPrimaryKey()).First().CurrentValue
                };

                switch (entry.State)
                {
                    //case EntityState.Deleted:
                    //    itemzChangeHistory.ChangeEvent = nameof(EntityState.Deleted);
                    //    itemzChangeHistory.OldValues= CreateDeletedChanges(entry);
                    //    break;
                    case EntityState.Added:
                        itemzChangeHistory.ChangeEvent = nameof(EntityState.Added);
                        itemzChangeHistory.CreatedDate = (DateTimeOffset) entry.Properties.Where(property => property.Metadata.Name == "CreatedDate").First().CurrentValue;
                        itemzChangeHistory.NewValues = CreateAddedChanges(entry);
                        listOfItemzChangeHistory.Add(itemzChangeHistory);
                        break;
                    case EntityState.Modified:
                        itemzChangeHistory.ChangeEvent = nameof(EntityState.Modified);
                        itemzChangeHistory.OldValues = CreateOldValueModifiedChanges(entry);
                        itemzChangeHistory.NewValues = CreateNewValueModifiedChanges(entry);
                        listOfItemzChangeHistory.Add(itemzChangeHistory);
                        break;
                    default:
                        break;
                }
            }

            return listOfItemzChangeHistory;

            string CreateAddedChanges(EntityEntry entry)
                => entry.Properties.Where(property => !property.Metadata.IsPrimaryKey()
                        && property.Metadata.Name != "CreatedDate")
                   .Aggregate(
                    "",
                    (auditString, property) => 
                            auditString + 
                            $"{property.Metadata.Name}: '{property.CurrentValue}' " + 
                            Environment.NewLine + Environment.NewLine);

            string CreateOldValueModifiedChanges(EntityEntry entry)
                => entry.Properties.Where(property => property.IsModified )
                    .Aggregate(
                    "",
                    (auditString, property) =>
                            auditString +
                            $"{property.Metadata.Name}: '{property.OriginalValue}' " +
                            Environment.NewLine + Environment.NewLine);

            string CreateNewValueModifiedChanges(EntityEntry entry)
                => entry.Properties.Where(property => property.IsModified)
                    .Aggregate(
                    "",
                    (auditString, property) => 
                            auditString + 
                            $"{property.Metadata.Name}: '{property.CurrentValue}' " + 
                            Environment.NewLine + Environment.NewLine);

            //string CreateDeletedChanges(EntityEntry entry)
            //    => entry.Properties.Where(property => !property.Metadata.IsPrimaryKey()
            //            && property.Metadata.Name != "CreatedDate"
            //            && property.Metadata.Name != "CreatedBy")
            //        .Aggregate(
            //        "",
            //        (auditString, property) => 
            //                auditString + 
            //                $"{property.Metadata.Name}: '{property.CurrentValue}' " + 
            //                Environment.NewLine + Environment.NewLine);
        }
        #endregion CreateItemzChangeHistory

    }
}
