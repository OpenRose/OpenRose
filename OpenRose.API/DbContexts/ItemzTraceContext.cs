// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using ItemzApp.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace ItemzApp.API.DbContexts
{
    public class ItemzTraceContext : DbContext
    {

        public ItemzTraceContext(DbContextOptions<ItemzTraceContext> options) : base(options)
        {

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
            {
                   // TODO: for some reason, this is always true here. Investigate why
                   // EF Core team has provided this property and what is the real use 
                   // of the same.
            }
        }

        public DbSet<ItemzJoinItemzTrace>? ItemzJoinItemzTrace { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ItemzJoinItemzTrace>()
                .HasKey(t => new { t.FromItemzId, t.ToItemzId });

            modelBuilder.Entity<ItemzTypeJoinItemz>()
                .HasKey(t => new { t.ItemzTypeId, t.ItemzId });

            modelBuilder.Entity<BaselineItemzTypeJoinBaselineItemz>()
                .HasKey(bitjbi => new { bitjbi.BaselineItemzTypeId, bitjbi.BaselineItemzId });
        }






    }
}
