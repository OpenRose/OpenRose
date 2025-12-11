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
		private readonly ILogger<ItemzContexInterceptor> _logger;

		public ItemzContexInterceptor(ILogger<ItemzContexInterceptor> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		void ISaveChangesInterceptor.SaveChangesFailed(DbContextErrorEventData eventData) { }

		Task ISaveChangesInterceptor.SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken)
			=> Task.CompletedTask;

		int ISaveChangesInterceptor.SavedChanges(SaveChangesCompletedEventData eventData, int result)
		{
			// Nothing to do here anymore — audit entries are saved together
			return result;
		}

		ValueTask<int> ISaveChangesInterceptor.SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken)
			=> new ValueTask<int>(result);

		InterceptionResult<int> ISaveChangesInterceptor.SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
		{
			AddAuditEntries(eventData.Context);
			return result;
		}

		ValueTask<InterceptionResult<int>> ISaveChangesInterceptor.SavingChangesAsync(
			DbContextEventData eventData,
			InterceptionResult<int> result,
			CancellationToken cancellationToken)
		{
			AddAuditEntries(eventData.Context);
			return new ValueTask<InterceptionResult<int>>(result);
		}

		private void AddAuditEntries(DbContext context)
		{
			context.ChangeTracker.DetectChanges();

			var auditEntries = new List<ItemzChangeHistory>();

			foreach (var entry in context.ChangeTracker.Entries<Itemz>())
			{
				if (entry.State == EntityState.Added)
				{
					var history = new ItemzChangeHistory
					{
						ChangeEvent = nameof(EntityState.Added),
						CreatedDate = (DateTimeOffset)entry.Property("CreatedDate").CurrentValue!,
						ItemzId = (Guid)entry.Property("Id").CurrentValue!,
						NewValues = CreateAddedChanges(entry)
					};
					auditEntries.Add(history);
				}
				else if (entry.State == EntityState.Modified)
				{
					var history = new ItemzChangeHistory
					{
						ChangeEvent = nameof(EntityState.Modified),
						CreatedDate = DateTimeOffset.UtcNow,
						ItemzId = (Guid)entry.Property("Id").CurrentValue!,
						OldValues = CreateOldValueModifiedChanges(entry),
						NewValues = CreateNewValueModifiedChanges(entry)
					};
					auditEntries.Add(history);
				}
			}

			if (auditEntries.Any())
			{
				context.Set<ItemzChangeHistory>().AddRange(auditEntries);
				_logger.LogDebug("::ITEMZ_CONTEX_INTERCEPTOR:: Prepared {Count} change history records", auditEntries.Count);
			}
		}

		private static string CreateAddedChanges(EntityEntry entry) =>
			entry.Properties.Where(p => !p.Metadata.IsPrimaryKey() && p.Metadata.Name != "CreatedDate")
				.Aggregate("", (audit, p) => audit + $"{p.Metadata.Name}: '{p.CurrentValue}'{Environment.NewLine}");

		private static string CreateOldValueModifiedChanges(EntityEntry entry) =>
			entry.Properties.Where(p => p.IsModified)
				.Aggregate("", (audit, p) => audit + $"{p.Metadata.Name}: '{p.OriginalValue}'{Environment.NewLine}");

		private static string CreateNewValueModifiedChanges(EntityEntry entry) =>
			entry.Properties.Where(p => p.IsModified)
				.Aggregate("", (audit, p) => audit + $"{p.Metadata.Name}: '{p.CurrentValue}'{Environment.NewLine}");
	}
}
