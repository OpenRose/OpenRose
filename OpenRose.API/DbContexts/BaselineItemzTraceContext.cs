// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using ItemzApp.API.Entities;
using Microsoft.EntityFrameworkCore;
// using System;

namespace ItemzApp.API.DbContexts
{
    public class BaselineItemzTraceContext : DbContext
    {

        public BaselineItemzTraceContext(DbContextOptions<BaselineItemzTraceContext> options) : base(options)
        {

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
            {
                // TODO: for some reason, this is always true here. Investigate why
                // EF Core team has provided this property and what is the real use 
                // of the same.

                // EnableSensitiveDataLogging is used only during development phase 
                // to understand Entity Framwork SQL Queries generated during
                // execution of EF Queries.
                // optionsBuilder.EnableSensitiveDataLogging(true)
                //    .LogTo(Console.WriteLine);

            }
        }

        public DbSet<BaselineItemzJoinItemzTrace>? BaselineItemzJoinItemzTrace { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ItemzJoinItemzTrace>()
                .HasKey(t => new { t.FromItemzId, t.ToItemzId });

            modelBuilder.Entity<BaselineItemzJoinItemzTrace>()
                .HasKey(t => new { t.BaselineFromItemzId, t.BaselineToItemzId });

            modelBuilder.Entity<ItemzTypeJoinItemz>()
                .HasKey(t => new { t.ItemzTypeId, t.ItemzId });

            modelBuilder.Entity<BaselineItemzTypeJoinBaselineItemz>()
                .HasKey(bitjbi => new { bitjbi.BaselineItemzTypeId, bitjbi.BaselineItemzId });
        }






    }
}
