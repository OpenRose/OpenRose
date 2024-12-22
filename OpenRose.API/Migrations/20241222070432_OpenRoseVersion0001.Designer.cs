﻿// <auto-generated />
using System;
using ItemzApp.API.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.SqlServer.Types;

#nullable disable

namespace OpenRose.API.Migrations
{
    [DbContext(typeof(ItemzContext))]
    [Migration("20241222070432_OpenRoseVersion0001")]
    partial class OpenRoseVersion0001
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("ItemzApp.API.Entities.Baseline", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("NEWID()");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<DateTimeOffset>("CreatedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetimeoffset")
                        .HasDefaultValueSql("SYSDATETIMEOFFSET()");

                    b.Property<string>("Description")
                        .HasColumnType("VARCHAR(MAX)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<Guid>("ProjectId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("ProjectId", "Name")
                        .IsUnique();

                    b.ToTable("Baseline");
                });

            modelBuilder.Entity("ItemzApp.API.Entities.BaselineItemz", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("NEWID()");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<DateTimeOffset?>("CreatedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Description")
                        .HasColumnType("VARCHAR(MAX)");

                    b.Property<Guid>("IgnoreMeBaselineItemzTypeId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ItemzId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Priority")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("Severity")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Status")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<bool>("isIncluded")
                        .HasColumnType("bit")
                        .HasDefaultValue(true);

                    b.HasKey("Id");

                    b.ToTable("BaselineItemz");
                });

            modelBuilder.Entity("ItemzApp.API.Entities.BaselineItemzHierarchy", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<SqlHierarchyId?>("BaselineItemzHierarchyId")
                        .HasColumnType("hierarchyid");

                    b.Property<string>("Name")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("RecordType")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<SqlHierarchyId?>("SourceItemzHierarchyId")
                        .HasColumnType("hierarchyid");

                    b.Property<bool>("isIncluded")
                        .HasColumnType("bit")
                        .HasDefaultValue(true);

                    b.HasKey("Id");

                    b.ToTable("BaselineItemzHierarchy");
                });

            modelBuilder.Entity("ItemzApp.API.Entities.BaselineItemzJoinItemzTrace", b =>
                {
                    b.Property<Guid>("BaselineFromItemzId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("BaselineToItemzId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("BaselineId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("BaselineFromItemzId", "BaselineToItemzId");

                    b.HasIndex("BaselineToItemzId");

                    b.ToTable("BaselineItemzJoinItemzTrace");
                });

            modelBuilder.Entity("ItemzApp.API.Entities.BaselineItemzType", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("NEWID()");

                    b.Property<Guid>("BaselineId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<DateTimeOffset?>("CreatedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Description")
                        .HasColumnType("VARCHAR(MAX)");

                    b.Property<bool>("IsSystem")
                        .HasColumnType("bit");

                    b.Property<Guid>("ItemzTypeId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Status")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.HasKey("Id");

                    b.HasIndex("BaselineId");

                    b.ToTable("BaselineItemzType");
                });

            modelBuilder.Entity("ItemzApp.API.Entities.BaselineItemzTypeJoinBaselineItemz", b =>
                {
                    b.Property<Guid>("BaselineItemzTypeId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("BaselineItemzId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("BaselineItemzTypeId", "BaselineItemzId");

                    b.HasIndex("BaselineItemzId");

                    b.ToTable("BaselineItemzTypeJoinBaselineItemz");
                });

            modelBuilder.Entity("ItemzApp.API.Entities.Itemz", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("NEWID()");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<DateTimeOffset>("CreatedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Description")
                        .HasColumnType("VARCHAR(MAX)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Priority")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)")
                        .HasDefaultValue("Low");

                    b.Property<string>("Severity")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasDefaultValue("Low");

                    b.Property<string>("Status")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)")
                        .HasDefaultValue("New");

                    b.HasKey("Id");

                    b.ToTable("Itemzs");
                });

            modelBuilder.Entity("ItemzApp.API.Entities.ItemzChangeHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ChangeEvent")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<DateTimeOffset>("CreatedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid>("ItemzId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("NewValues")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OldValues")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("ItemzId");

                    b.ToTable("ItemzChangeHistory");
                });

            modelBuilder.Entity("ItemzApp.API.Entities.ItemzHierarchy", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<SqlHierarchyId?>("ItemzHierarchyId")
                        .HasColumnType("hierarchyid");

                    b.Property<string>("Name")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("RecordType")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.HasKey("Id");

                    b.ToTable("ItemzHierarchy");
                });

            modelBuilder.Entity("ItemzApp.API.Entities.ItemzJoinItemzTrace", b =>
                {
                    b.Property<Guid>("FromItemzId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ToItemzId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("FromItemzId", "ToItemzId");

                    b.HasIndex("ToItemzId");

                    b.ToTable("ItemzJoinItemzTrace");
                });

            modelBuilder.Entity("ItemzApp.API.Entities.ItemzType", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("NEWID()");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<DateTimeOffset>("CreatedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Description")
                        .HasColumnType("VARCHAR(MAX)");

                    b.Property<bool>("IsSystem")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<Guid>("ProjectId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Status")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)")
                        .HasDefaultValue("New");

                    b.HasKey("Id");

                    b.HasIndex("ProjectId");

                    b.ToTable("ItemzTypes");
                });

            modelBuilder.Entity("ItemzApp.API.Entities.ItemzTypeJoinItemz", b =>
                {
                    b.Property<Guid>("ItemzTypeId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ItemzId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("ItemzTypeId", "ItemzId");

                    b.HasIndex("ItemzId");

                    b.ToTable("ItemzTypeJoinItemz");
                });

            modelBuilder.Entity("ItemzApp.API.Entities.Project", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("NEWID()");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<DateTimeOffset>("CreatedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Description")
                        .HasColumnType("VARCHAR(MAX)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)")
                        .HasDefaultValue("New");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("ItemzApp.API.Entities.Baseline", b =>
                {
                    b.HasOne("ItemzApp.API.Entities.Project", "Project")
                        .WithMany("Baseline")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Project");
                });

            modelBuilder.Entity("ItemzApp.API.Entities.BaselineItemzJoinItemzTrace", b =>
                {
                    b.HasOne("ItemzApp.API.Entities.BaselineItemz", "BaselineFromItemz")
                        .WithMany()
                        .HasForeignKey("BaselineFromItemzId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("ItemzApp.API.Entities.BaselineItemz", "BaselineToItemz")
                        .WithMany()
                        .HasForeignKey("BaselineToItemzId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("BaselineFromItemz");

                    b.Navigation("BaselineToItemz");
                });

            modelBuilder.Entity("ItemzApp.API.Entities.BaselineItemzType", b =>
                {
                    b.HasOne("ItemzApp.API.Entities.Baseline", "Baseline")
                        .WithMany("BaselineItemzTypes")
                        .HasForeignKey("BaselineId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Baseline");
                });

            modelBuilder.Entity("ItemzApp.API.Entities.BaselineItemzTypeJoinBaselineItemz", b =>
                {
                    b.HasOne("ItemzApp.API.Entities.BaselineItemz", "BaselineItemz")
                        .WithMany("BaselineItemzTypeJoinBaselineItemz")
                        .HasForeignKey("BaselineItemzId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ItemzApp.API.Entities.BaselineItemzType", "BaselineItemzType")
                        .WithMany("BaselineItemzTypeJoinBaselineItemz")
                        .HasForeignKey("BaselineItemzTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BaselineItemz");

                    b.Navigation("BaselineItemzType");
                });

            modelBuilder.Entity("ItemzApp.API.Entities.ItemzChangeHistory", b =>
                {
                    b.HasOne("ItemzApp.API.Entities.Itemz", "Itemz")
                        .WithMany()
                        .HasForeignKey("ItemzId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Itemz");
                });

            modelBuilder.Entity("ItemzApp.API.Entities.ItemzJoinItemzTrace", b =>
                {
                    b.HasOne("ItemzApp.API.Entities.Itemz", "FromItemz")
                        .WithMany()
                        .HasForeignKey("FromItemzId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("ItemzApp.API.Entities.Itemz", "ToItemz")
                        .WithMany()
                        .HasForeignKey("ToItemzId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("FromItemz");

                    b.Navigation("ToItemz");
                });

            modelBuilder.Entity("ItemzApp.API.Entities.ItemzType", b =>
                {
                    b.HasOne("ItemzApp.API.Entities.Project", "Project")
                        .WithMany("ItemzTypes")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Project");
                });

            modelBuilder.Entity("ItemzApp.API.Entities.ItemzTypeJoinItemz", b =>
                {
                    b.HasOne("ItemzApp.API.Entities.Itemz", "Itemz")
                        .WithMany("ItemzTypeJoinItemz")
                        .HasForeignKey("ItemzId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ItemzApp.API.Entities.ItemzType", "ItemzType")
                        .WithMany("ItemzTypeJoinItemz")
                        .HasForeignKey("ItemzTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Itemz");

                    b.Navigation("ItemzType");
                });

            modelBuilder.Entity("ItemzApp.API.Entities.Baseline", b =>
                {
                    b.Navigation("BaselineItemzTypes");
                });

            modelBuilder.Entity("ItemzApp.API.Entities.BaselineItemz", b =>
                {
                    b.Navigation("BaselineItemzTypeJoinBaselineItemz");
                });

            modelBuilder.Entity("ItemzApp.API.Entities.BaselineItemzType", b =>
                {
                    b.Navigation("BaselineItemzTypeJoinBaselineItemz");
                });

            modelBuilder.Entity("ItemzApp.API.Entities.Itemz", b =>
                {
                    b.Navigation("ItemzTypeJoinItemz");
                });

            modelBuilder.Entity("ItemzApp.API.Entities.ItemzType", b =>
                {
                    b.Navigation("ItemzTypeJoinItemz");
                });

            modelBuilder.Entity("ItemzApp.API.Entities.Project", b =>
                {
                    b.Navigation("Baseline");

                    b.Navigation("ItemzTypes");
                });
#pragma warning restore 612, 618
        }
    }
}
