// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using ItemzApp.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace ItemzApp.API.DbContexts
{
    public class BaselineContext : DbContext
    {

        public BaselineContext(DbContextOptions<BaselineContext> options) : base(options)
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

        public DbSet<Baseline>? Baseline { get; set; }
        public DbSet<BaselineItemzType>? BaselineItemzType { get; set; }
        public DbSet<BaselineItemz>? BaselineItemz { get; set; }
        public DbSet<BaselineItemzTypeJoinBaselineItemz>? BaselineItemzTypeJoinBaselineItemz { get; set; }
        public DbSet<BaselineItemzHierarchy>? BaselineItemzHierarchy { get; set; }

        // EXPLANATION : Additional DBSets listed here that are required 
        // for query and update purposes in services
        // that utilizes BaselineContext
        public DbSet<Project>? Projects { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // TODO: Main configuration is defined in ItemzContext. This context may not need all
            // the configuration details here. We have tried checking this in ItemzChangeHistoryContext 
            // where by we have disabled most of the configuration details in there. Only details that are 
            // kept intect are those related to Primary Keys i.e. HasKey details.

            #region Baseline 
            modelBuilder.Entity<Baseline>()
                .HasOne(b => b.Project)
                .WithMany(p => p!.Baseline)
                .HasForeignKey(b => b.ProjectId);

            // EXPLANATION: This will make sure that GUID property is set to autogenerate in the 
            // SQL Server Database as well.

            modelBuilder.Entity<Baseline>().Property(x => x.Id).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<Baseline>(entity =>
            {
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();
            });

            // EXPLANATION: This way, we are adding Unique Index for Baseline Name.
            // It's not possible to use Attribute for creating Unique Index in
            // EF Core 3.1. That is why I'm using Fluent API for the same.
            // Docs can be found at ...
            // https://docs.microsoft.com/en-us/ef/core/modeling/indexes

            modelBuilder.Entity<Baseline>()
                .HasIndex(b => b.Name)
                .IsUnique();
            #endregion

            #region BaselineItemzType
            modelBuilder.Entity<BaselineItemzType>()
                .HasOne(bitype => bitype.Baseline)
                .WithMany(b => b!.BaselineItemzTypes)
                .HasForeignKey(bitype => bitype.BaselineId);

            // EXPLANATION: This will make sure that GUID property is set to autogenerate in the 
            // SQL Server Database as well.

            modelBuilder.Entity<BaselineItemzType>().Property(x => x.Id).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<BaselineItemzType>(entity =>
            {
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();
            });
            #endregion

            #region BaselineItemz
            // EXPLANATION: This will make sure that GUID property is set to autogenerate in the 
            // SQL Server Database as well.

            modelBuilder.Entity<BaselineItemz>().Property(x => x.Id).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<BaselineItemz>(entity =>
            {
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();
            });
            #endregion


            #region BaselineItemzTypeJoinBaselineItemz
            // EXPLANATION: Here we are defining a composite key for a join table.
            // it will use BaselineItemzTypeID + BaselineItemzID as it's composite key.

            modelBuilder.Entity<BaselineItemzTypeJoinBaselineItemz>()
                .HasKey(bitjbi => new { bitjbi.BaselineItemzTypeId, bitjbi.BaselineItemzId });

            // EXPLANATION: Here we are defining Many to Many relationship between
            // BaselineItemzType and BaselineItemz
            // This is described as Indirect Many-to-Many relationships in Microsoft Docs at ...
            // https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key#indirect-many-to-many-relationships

            modelBuilder.Entity<BaselineItemzTypeJoinBaselineItemz>()
                .HasOne(bitjbi => bitjbi.BaselineItemzType)
                .WithMany(bitype => bitype!.BaselineItemzTypeJoinBaselineItemz)
                .HasForeignKey(bitjbi => bitjbi.BaselineItemzTypeId);
            modelBuilder.Entity<BaselineItemzTypeJoinBaselineItemz>()
                .HasOne(bitjbi => bitjbi.BaselineItemz)
                .WithMany(bit => bit!.BaselineItemzTypeJoinBaselineItemz)
                .HasForeignKey(bitjbi => bitjbi.BaselineItemzId);

            #endregion

            #region ItemzTypeJoinItemz
            // EXPLANATION: Here we are defining a composite key for a join table.
            // it will use ItemzTypeID + Itemz ID as it's composite key.

            modelBuilder.Entity<ItemzTypeJoinItemz>()
                .HasKey(t => new { t.ItemzTypeId, t.ItemzId });

            #endregion

            #region BaselineItemzHierarchy

            modelBuilder.Entity<BaselineItemzHierarchy>(entity =>
            {
                entity.Property(bih => bih.isIncluded)
                    .HasDefaultValue(true);
            });

            #endregion

            base.OnModelCreating(modelBuilder);
        }
    }
}
