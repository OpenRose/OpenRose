// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using ItemzApp.API.DbContexts.Interceptors;
using ItemzApp.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ItemzApp.API.DbContexts
{
    public class ItemzContext : DbContext
    {
        public ItemzContext(DbContextOptions<ItemzContext> options) : base(options)
        {

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            bool IsItemzContexInterceptorRegisteredAlready = false;

            if (optionsBuilder.IsConfigured)
            {
                // EXPLANATION: optionsBuilder.IsConfigured is true because in the startup.cs
                // file we are using SQL Server as part of
                // services.AddDbContext<ItemzContext>
                // We could use this if condition here to check if the optionsBuilder.IsConfigured
                // is set to true. If it's not true then we can add our own custom
                // optionsBuilder as part of OnConfiguring method this way we can
                // point to some custom SQL Server environment.

            }
        }

        public DbSet<Itemz>? Itemzs { get; set; }
        public DbSet<Project>? Projects { get; set; }
        //public DbSet<ProjectJoinItemz> ProjectJoinItemz { get; set; }

        public DbSet<ItemzType>? ItemzTypes { get; set; }
        public DbSet<ItemzTypeJoinItemz>? ItemzTypeJoinItemz { get; set; }
        public DbSet<ItemzChangeHistory>? ItemzChangeHistory { get; set; }
        public DbSet<Baseline>? Baseline { get; set; }
        public DbSet<BaselineItemzType>? BaselineItemzType { get; set; }
        public DbSet<BaselineItemz>? BaselineItemz { get; set; }
        public DbSet<BaselineItemzTypeJoinBaselineItemz>? BaselineItemzTypeJoinBaselineItemz { get; set; }
        public DbSet<ItemzJoinItemzTrace>? ItemzJoinItemzTrace { get; set; }
        public DbSet<BaselineItemzJoinItemzTrace>? BaselineItemzJoinItemzTrace { get; set; }
        public DbSet<ItemzHierarchy>? ItemzHierarchy { get; set; }
        public DbSet<BaselineItemzHierarchy>? BaselineItemzHierarchy { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // seed the database with dummy data






            ////////modelBuilder.Entity<Itemz>().HasData(
            ////////    new Itemz()
            ////////    {
            ////////        Id = Guid.Parse("9153a516-d69e-4364-b17e-03b87442e21c"),
            ////////        Name = "Item 1",
            ////////        Status = "New",
            ////////        Priority = "High",
            ////////        Description = "Requirements to be described here.",
            ////////        CreatedBy = "User 1",
            ////////        CreatedDate = new DateTime(2019, 7, 01),
            ////////    },
            ////////    new Itemz()
            ////////    {
            ////////        Id = Guid.Parse("5e76f8e8-d3e7-41db-b084-f64c107c6783"),
            ////////        Name = "Item 2",
            ////////        Status = "New",
            ////////        Priority = "Medium",
            ////////        Description = "Requirements to be described here.",
            ////////        CreatedBy = "User 2",
            ////////        CreatedDate = new DateTime(2019, 7, 02),
            ////////    },
            ////////    new Itemz()
            ////////    {
            ////////        Id = Guid.Parse("885b8e56-ffe6-4bc9-82e2-ce23230991db"),
            ////////        Name = "Item 3",
            ////////        Status = "New",
            ////////        Priority = "Low",
            ////////        Description = "Requirements to be described here.",
            ////////        CreatedBy = "User 3",
            ////////        CreatedDate = new DateTime(2019, 7, 03),
            ////////    },
            ////////    new Itemz()
            ////////    {
            ////////        Id = Guid.Parse("0850bc8a-84c4-4c52-8ab6-b06e7bc2195b"),
            ////////        Name = "Item 4",
            ////////        Status = "New",
            ////////        Priority = "Low",
            ////////        Description = "Requirements to be described here.",
            ////////        CreatedBy = "User 4",
            ////////        CreatedDate = new DateTime(2019, 7, 04),
            ////////    },
            ////////    new Itemz()
            ////////    {
            ////////        Id = Guid.Parse("4adde56d-8081-45ea-bd37-5c830b63873b"),
            ////////        Name = "Item 5",
            ////////        Status = "New",
            ////////        Priority = "Low",
            ////////        Description = "Requirements to be described here.",
            ////////        CreatedBy = "User 5",
            ////////        CreatedDate = new DateTime(2019, 7, 05),
            ////////    }
            ////////);








            // EXPLANATION: This will make sure that GUID property is set to autogenerate in the 
            // SQL Server Database as well.

            modelBuilder.Entity<Itemz>().Property(x => x.Id).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<Itemz>(entity =>
            {
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();
            });

            // EXPLANATION: Severity is controlled by build in ENUM which has a fixed 
            // list of values. ItemzAPP API is going to control this values but 
            // by design we are only storing string values into the database
            // for selected Severity Value. 

            modelBuilder.Entity<Itemz>()
                 .Property(i => i.Severity)
              .HasMaxLength(128)
              .HasConversion(new EnumToStringConverter<ItemzSeverity>())
              .HasDefaultValue((ItemzSeverity)Enum.Parse(
                                    typeof(ItemzSeverity),
                                    EntityPropertyDefaultValues.ItemzSeverityDefaultValue,
                                    true));

			modelBuilder.Entity<Itemz>()
				 .Property(i => i.Priority)
			  .HasMaxLength(64)
			  .HasConversion(new EnumToStringConverter<ItemzPriority>())
			  .HasDefaultValue((ItemzPriority)Enum.Parse(
									typeof(ItemzPriority),
									EntityPropertyDefaultValues.ItemzPriorityDefaultValue,
									true))
			  ;

			modelBuilder.Entity<Itemz>()
				 .Property(i => i.Status)
			  .HasMaxLength(64)
			  .HasConversion(new EnumToStringConverter<ItemzStatus>())
			  .HasDefaultValue((ItemzStatus)Enum.Parse(
									typeof(ItemzStatus),
									EntityPropertyDefaultValues.ItemzStatusDefaultValue,
									true))
			  ;

			modelBuilder.Entity<Itemz>()
                .HasMany(i => i.FromItemzJoinItemzTrace)
                .WithMany(i => i.ToItemzJoinItemzTrace)
                .UsingEntity<ItemzJoinItemzTrace>(
                    b => b.HasOne(e => e.FromItemz!).WithMany().HasForeignKey(e => e.FromItemzId).OnDelete(DeleteBehavior.Restrict),
                    b => b.HasOne(e => e.ToItemz!).WithMany().HasForeignKey(e => e.ToItemzId).OnDelete(DeleteBehavior.Restrict)
                    );


            // EXPLANATION: This will make sure that GUID property is set to autogenerate in the 
            // SQL Server Database as well.

            modelBuilder.Entity<Project>().Property(x => x.Id).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<Project>(entity =>
            {
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();
            });

            // EXPLANATION: This way, we are adding Unique Index for Project Name.
            // It's not possible to use Attribute for creating Unique Index in
            // EF Core 3.1. That is why I'm using Fluent API for the same.
            // Docs ca be found at ...
            // https://docs.microsoft.com/en-us/ef/core/modeling/indexes
            modelBuilder.Entity<Project>()
                .HasIndex(p => p.Name)
                .IsUnique();

			modelBuilder.Entity<Project>()
				 .Property(i => i.Status)
			  .HasMaxLength(64)
			  .HasConversion(new EnumToStringConverter<ProjectStatus>())
			  .HasDefaultValue((ProjectStatus)Enum.Parse(
									typeof(ProjectStatus),
									EntityPropertyDefaultValues.ProjectStatusDefaultValue,
									true))
			  ;

			//// EXPLANATION: Here we are defining a composite key for a join table.
			//// it will use ProjectID + Itemz ID as it's composite key.

			//modelBuilder.Entity<ProjectJoinItemz>()
			//    .HasKey(t => new { t.ProjectId, t.ItemzId });

			//// EXPLANATION: Here we are defining Many to Many relationship between
			//// Project and Itemz

			//modelBuilder.Entity<ProjectJoinItemz>()
			//    .HasOne(p => p.Project)
			//    .WithMany(itemz => itemz.ProjectJoinItemz)
			//    .HasForeignKey(p => p.ProjectId);
			//modelBuilder.Entity<ProjectJoinItemz>()
			//    .HasOne(itemz => itemz.Itemz)
			//    .WithMany(p => p.ProjectJoinItemz)
			//    .HasForeignKey(itemz => itemz.ItemzId);





			//////////modelBuilder.Entity<Project>().HasData(
			//////////    new Project()
			//////////    {
			//////////        Id = Guid.Parse("42f62a6c-fcda-4dac-a06c-406ac1c17770"),
			//////////        Name = "Project 1",
			//////////        Status = "Active",
			//////////        Description = "This is Project 1",
			//////////        CreatedBy = "User 1",
			//////////        CreatedDate = new DateTime(2019, 7, 01),
			//////////    },
			//////////    new Project()
			//////////    {
			//////////        Id = Guid.Parse("b69cf0d7-70ad-4f73-aa4a-8daad5181e1e"),
			//////////        Name = "Project 2",
			//////////        Status = "Active",
			//////////        Description = "This is Project 2",
			//////////        CreatedBy = "User 1",
			//////////        CreatedDate = new DateTime(2019, 7, 01),
			//////////    }
			//////////    );




			//modelBuilder.Entity<ProjectJoinItemz>().HasData(
			//     new ProjectJoinItemz()
			//     {
			//         ProjectId = new Guid("42f62a6c-fcda-4dac-a06c-406ac1c17770"),
			//         ItemzId = new Guid("9153a516-d69e-4364-b17e-03b87442e21c")
			//     },
			//    new ProjectJoinItemz()
			//    {
			//        ProjectId = new Guid("b69cf0d7-70ad-4f73-aa4a-8daad5181e1e"),
			//        ItemzId = new Guid("5e76f8e8-d3e7-41db-b084-f64c107c6783")
			//    }
			//     );








			// EXPLANATION: This will make sure that GUID property is set to autogenerate in the 
			// SQL Server Database as well.

			modelBuilder.Entity<ItemzType>().Property(x => x.Id).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<ItemzType>(entity =>
            {
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();
            });


			modelBuilder.Entity<ItemzType>()
				 .Property(i => i.Status)
			  .HasMaxLength(64)
			  .HasConversion(new EnumToStringConverter<ItemzTypeStatus>())
			  .HasDefaultValue((ItemzTypeStatus)Enum.Parse(
									typeof(ItemzTypeStatus),
									EntityPropertyDefaultValues.ItemzTypeStatusDefaultValue,
									true))
			  ;


			// EXPLANATION: Here we are defining a composite key for a join table.
			// it will use ItemzTypeID + Itemz ID as it's composite key.

			modelBuilder.Entity<ItemzTypeJoinItemz>()
                .HasKey(t => new { t.ItemzTypeId, t.ItemzId });

            // EXPLANATION: Here we are defining Many to Many relationship between
            // ItemzType and Itemz
            // This is described as Indirect Many-to-Many relationships in Microsoft Docs at ...
            // https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key#indirect-many-to-many-relationships

            modelBuilder.Entity<ItemzTypeJoinItemz>()
                .HasOne(itji => itji.ItemzType)
                .WithMany(it => it!.ItemzTypeJoinItemz)
                .HasForeignKey(itji => itji.ItemzTypeId);
            modelBuilder.Entity<ItemzTypeJoinItemz>()
                .HasOne(itji => itji.Itemz)
                .WithMany(i => i!.ItemzTypeJoinItemz)
                .HasForeignKey(itji => itji.ItemzId);

            modelBuilder.Entity<ItemzType>()
                .HasOne(it => it.Project)
                .WithMany(p => p!.ItemzTypes)
                .HasForeignKey(it => it.ProjectId);

            
            
            
            
            
            
            //////////modelBuilder.Entity<ItemzType>().HasData(
            //////////    new ItemzType()
            //////////    {
            //////////        Id = Guid.Parse("473fe535-1420-42cf-8a40-224388b8df24"),
            //////////        Name = "Parking Lot",
            //////////        Status = "Active",
            //////////        Description = "This is Parking Lot system ItemzType",
            //////////        CreatedBy = "User 1",
            //////////        CreatedDate = new DateTime(2019, 7, 01),
            //////////        IsSystem = true,
            //////////        ProjectId = Guid.Parse("42f62a6c-fcda-4dac-a06c-406ac1c17770")
            //////////    },
            //////////    new ItemzType()
            //////////    {
            //////////        Id = Guid.Parse("611639db-577a-48f6-9b08-f6aef477368f"),
            //////////        Name = "ItemzType 1",
            //////////        Status = "Active",
            //////////        Description = "This is ItemzType 1",
            //////////        CreatedBy = "User 1",
            //////////        CreatedDate = new DateTime(2019, 7, 01),
            //////////        ProjectId = Guid.Parse("42f62a6c-fcda-4dac-a06c-406ac1c17770")
            //////////    },
            //////////    new ItemzType()
            //////////    {
            //////////        Id = Guid.Parse("1a069648-6ad6-4c8a-be05-be747bdeb8da"),
            //////////        Name = "Parking Lot",
            //////////        Status = "Active",
            //////////        Description = "This is Parking Lot system ItemzType",
            //////////        CreatedBy = "User 1",
            //////////        CreatedDate = new DateTime(2019, 7, 01),
            //////////        IsSystem = true,
            //////////        ProjectId = Guid.Parse("b69cf0d7-70ad-4f73-aa4a-8daad5181e1e")
            //////////    },
            //////////    new ItemzType()
            //////////    {
            //////////        Id = Guid.Parse("8414bf58-6331-4b3e-bbf0-f780950a337b"),
            //////////        Name = "ItemzType 2",
            //////////        Status = "Active",
            //////////        Description = "This is ItemzType 2",
            //////////        CreatedBy = "User 1",
            //////////        CreatedDate = new DateTime(2019, 7, 01),
            //////////        ProjectId = Guid.Parse("b69cf0d7-70ad-4f73-aa4a-8daad5181e1e")
            //////////    },
            //////////    new ItemzType()
            //////////    {
            //////////        Id = Guid.Parse("52ca1dfc-b187-47fc-a379-57af33404b34"),
            //////////        Name = "TESTING - Unique ItemzType Name",
            //////////        Status = "Active",
            //////////        Description = "Used for testing that ItemzType Name remains unique within a given Project",
            //////////        CreatedBy = "User 1",
            //////////        CreatedDate = new DateTime(2019, 7, 01),
            //////////        ProjectId = Guid.Parse("b69cf0d7-70ad-4f73-aa4a-8daad5181e1e")
            //////////    }
            //////////);
            //////////modelBuilder.Entity<ItemzTypeJoinItemz>().HasData(
            //////////     new ItemzTypeJoinItemz()
            //////////     {
            //////////         ItemzTypeId = new Guid("611639db-577a-48f6-9b08-f6aef477368f"),
            //////////         ItemzId = new Guid("9153a516-d69e-4364-b17e-03b87442e21c")
            //////////     },
            //////////    new ItemzTypeJoinItemz()
            //////////    {
            //////////        ItemzTypeId = new Guid("8414bf58-6331-4b3e-bbf0-f780950a337b"),
            //////////        ItemzId = new Guid("5e76f8e8-d3e7-41db-b084-f64c107c6783")
            //////////    }
            //////////);
            


            // EXPLANATION: We are setting relationship between ItemzChangeHistory
            // and Itemz where by we only store ItemzId in the ItemzChangeHistory. 
            // For now, there is not need to store ItemzChangeHistory in the Itemz
            // Entity as we don't want to have Unique ID (Primary Key) for ItemzChangeHistory 
            // table in the Database.

            modelBuilder.Entity<ItemzChangeHistory>()
                .HasOne(ich => ich.Itemz)
                .WithMany()
                .HasForeignKey(ich => ich.ItemzId);


            #region Baseline 

            // EXPLANATION : We introduced Constant String 'baselineForeignKey' to make sure
            // that we enable composit index for uniqueness. This is to allow single project
            // to have baselines with unique names but different projects to have baseline
            // with same name. 
            // We learned about this technique via following article.
            // https://stackoverflow.com/a/63747079

            const string baselineForeignKey = "ProjectId";
            modelBuilder.Entity<Baseline>()
                .HasOne(b => b.Project)
                .WithMany(p => p!.Baseline)
                .HasForeignKey(baselineForeignKey);

            // EXPLANATION: This way, we are adding Unique Index for
            // Baseline Name PLUS ProjectID.
            // It's not possible to use Attribute for creating Unique Index in
            // EF Core 3.1. That is why I'm using Fluent API for the same.
            // Docs ca be found at ...
            // https://docs.microsoft.com/en-us/ef/core/modeling/indexes

            modelBuilder.Entity<Baseline>()
                .HasIndex(baselineForeignKey, nameof(ItemzApp.API.Entities.Baseline.Name))
                .IsUnique();

            // EXPLANATION: This will make sure that GUID property is set to autogenerate in the 
            // SQL Server Database as well.

            modelBuilder.Entity<Baseline>().Property(x => x.Id).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<Baseline>(entity =>
            {
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();
            });

            // EXPLANATION: This will make sure that CreatedDate property is set to autogenerate in the 
            // SQL Server Database as well.
            modelBuilder.Entity<Baseline>().Property(x => x.CreatedDate).HasDefaultValueSql("SYSDATETIMEOFFSET()");
            modelBuilder.Entity<Baseline>(entity =>
            {
                entity.Property(e => e.CreatedDate)
                    .ValueGeneratedOnAdd();
            });

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
                entity.Property(bi => bi.Id)
                    .ValueGeneratedOnAdd();
            });

            // EXPLANATION: This will make sure that isIncluded property is set with default value 1 (as true)
            // in SQL Server Database. This means that by default BaselineItemz are included in the
            // Baseline.

            modelBuilder.Entity<BaselineItemz>(entity =>
            {
                entity.Property(bi => bi.isIncluded)
                    .HasDefaultValue(true);
            });

            modelBuilder.Entity<BaselineItemz>()
                .HasMany(bi => bi.BaselineFromItemzJoinItemzTrace)
                .WithMany(bi => bi.BaselineToItemzJoinItemzTrace)
                .UsingEntity<BaselineItemzJoinItemzTrace>(
                    b => b.HasOne(e => e.BaselineFromItemz!).WithMany().HasForeignKey(e => e.BaselineFromItemzId).OnDelete(DeleteBehavior.Restrict),
                    b => b.HasOne(e => e.BaselineToItemz!).WithMany().HasForeignKey(e => e.BaselineToItemzId).OnDelete(DeleteBehavior.Restrict)
                    );

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

            #region ItemzHierarchy

            //modelBuilder.Entity<ItemzHierarchy>(eb =>
            //{
            //    eb.HasNoKey();
            //});





            ////////modelBuilder.Entity<ItemzHierarchy>().HasData(
            ////////    new ItemzHierarchy()
            ////////    {
            ////////        Id = Guid.Parse("18B014C8-6C31-408F-B8F3-3A4F342ABBB1"),
            ////////        RecordType = "Repository",
            ////////        ItemzHierarchyId = HierarchyId.Parse("/")
            ////////    },
            ////////    new ItemzHierarchy()
            ////////    {
            ////////        Id = Guid.Parse("42f62a6c-fcda-4dac-a06c-406ac1c17770"),
            ////////        RecordType = "Project",
            ////////        ItemzHierarchyId = HierarchyId.Parse("/1/")
            ////////    },
            ////////    new ItemzHierarchy()
            ////////    {
            ////////        Id = Guid.Parse("473fe535-1420-42cf-8a40-224388b8df24"),
            ////////        RecordType = "ItemzType",
            ////////        ItemzHierarchyId = HierarchyId.Parse("/1/1/")
            ////////    },
            ////////    new ItemzHierarchy()
            ////////    {
            ////////        Id = Guid.Parse("611639db-577a-48f6-9b08-f6aef477368f"),
            ////////        RecordType = "ItemzType",
            ////////        ItemzHierarchyId = HierarchyId.Parse("/1/2/")
            ////////    },
            ////////    new ItemzHierarchy()
            ////////    {
            ////////        Id = Guid.Parse("9153a516-d69e-4364-b17e-03b87442e21c"),
            ////////        RecordType = "Itemz",
            ////////        ItemzHierarchyId = HierarchyId.Parse("/1/2/1/")
            ////////    },
            ////////    new ItemzHierarchy()
            ////////    {
            ////////        Id = Guid.Parse("b69cf0d7-70ad-4f73-aa4a-8daad5181e1e"),
            ////////        RecordType = "Project",
            ////////        ItemzHierarchyId = HierarchyId.Parse("/2/")
            ////////    },
            ////////    new ItemzHierarchy()
            ////////    {
            ////////        Id = Guid.Parse("1a069648-6ad6-4c8a-be05-be747bdeb8da"),
            ////////        RecordType = "ItemzType",
            ////////        ItemzHierarchyId = HierarchyId.Parse("/2/1/")
            ////////    },
            ////////    new ItemzHierarchy()
            ////////    {
            ////////        Id = Guid.Parse("8414bf58-6331-4b3e-bbf0-f780950a337b"),
            ////////        RecordType = "ItemzType",
            ////////        ItemzHierarchyId = HierarchyId.Parse("/2/2/")
            ////////    },
            ////////    new ItemzHierarchy()
            ////////    {
            ////////        Id = Guid.Parse("5e76f8e8-d3e7-41db-b084-f64c107c6783"),
            ////////        RecordType = "Itemz",
            ////////        ItemzHierarchyId = HierarchyId.Parse("/2/2/1/")
            ////////    },
            ////////    new ItemzHierarchy()
            ////////    {
            ////////        Id = Guid.Parse("52ca1dfc-b187-47fc-a379-57af33404b34"),
            ////////        RecordType = "ItemzType",
            ////////        ItemzHierarchyId = HierarchyId.Parse("/2/3/")
            ////////    }
            ////////);
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
