// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.


using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.SqlServer.Types;

#nullable disable

namespace OpenRose.API.Migrations
{
    /// <inheritdoc />
    public partial class OpenRoseVersion0001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BaselineItemz",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    ItemzId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Description = table.Column<string>(type: "VARCHAR(MAX)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Severity = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    IgnoreMeBaselineItemzTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    isIncluded = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaselineItemz", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BaselineItemzHierarchy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecordType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    BaselineItemzHierarchyId = table.Column<SqlHierarchyId>(type: "hierarchyid", nullable: true),
                    SourceItemzHierarchyId = table.Column<SqlHierarchyId>(type: "hierarchyid", nullable: true),
                    isIncluded = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaselineItemzHierarchy", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemzHierarchy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecordType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ItemzHierarchyId = table.Column<SqlHierarchyId>(type: "hierarchyid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemzHierarchy", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Itemzs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, defaultValue: "New"),
                    Priority = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, defaultValue: "Low"),
                    Description = table.Column<string>(type: "VARCHAR(MAX)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, defaultValue: "Low")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Itemzs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, defaultValue: "New"),
                    Description = table.Column<string>(type: "VARCHAR(MAX)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BaselineItemzJoinItemzTrace",
                columns: table => new
                {
                    BaselineFromItemzId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BaselineToItemzId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BaselineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaselineItemzJoinItemzTrace", x => new { x.BaselineFromItemzId, x.BaselineToItemzId });
                    table.ForeignKey(
                        name: "FK_BaselineItemzJoinItemzTrace_BaselineItemz_BaselineFromItemzId",
                        column: x => x.BaselineFromItemzId,
                        principalTable: "BaselineItemz",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BaselineItemzJoinItemzTrace_BaselineItemz_BaselineToItemzId",
                        column: x => x.BaselineToItemzId,
                        principalTable: "BaselineItemz",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemzChangeHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemzId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangeEvent = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemzChangeHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemzChangeHistory_Itemzs_ItemzId",
                        column: x => x.ItemzId,
                        principalTable: "Itemzs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemzJoinItemzTrace",
                columns: table => new
                {
                    FromItemzId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToItemzId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemzJoinItemzTrace", x => new { x.FromItemzId, x.ToItemzId });
                    table.ForeignKey(
                        name: "FK_ItemzJoinItemzTrace_Itemzs_FromItemzId",
                        column: x => x.FromItemzId,
                        principalTable: "Itemzs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemzJoinItemzTrace_Itemzs_ToItemzId",
                        column: x => x.ToItemzId,
                        principalTable: "Itemzs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Baseline",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "VARCHAR(MAX)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Baseline", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Baseline_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemzTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, defaultValue: "New"),
                    Description = table.Column<string>(type: "VARCHAR(MAX)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemzTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemzTypes_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BaselineItemzType",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    ItemzTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Description = table.Column<string>(type: "VARCHAR(MAX)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BaselineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaselineItemzType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaselineItemzType_Baseline_BaselineId",
                        column: x => x.BaselineId,
                        principalTable: "Baseline",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemzTypeJoinItemz",
                columns: table => new
                {
                    ItemzTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemzId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemzTypeJoinItemz", x => new { x.ItemzTypeId, x.ItemzId });
                    table.ForeignKey(
                        name: "FK_ItemzTypeJoinItemz_ItemzTypes_ItemzTypeId",
                        column: x => x.ItemzTypeId,
                        principalTable: "ItemzTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemzTypeJoinItemz_Itemzs_ItemzId",
                        column: x => x.ItemzId,
                        principalTable: "Itemzs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BaselineItemzTypeJoinBaselineItemz",
                columns: table => new
                {
                    BaselineItemzTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BaselineItemzId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaselineItemzTypeJoinBaselineItemz", x => new { x.BaselineItemzTypeId, x.BaselineItemzId });
                    table.ForeignKey(
                        name: "FK_BaselineItemzTypeJoinBaselineItemz_BaselineItemzType_BaselineItemzTypeId",
                        column: x => x.BaselineItemzTypeId,
                        principalTable: "BaselineItemzType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaselineItemzTypeJoinBaselineItemz_BaselineItemz_BaselineItemzId",
                        column: x => x.BaselineItemzId,
                        principalTable: "BaselineItemz",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Baseline_ProjectId_Name",
                table: "Baseline",
                columns: new[] { "ProjectId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BaselineItemzJoinItemzTrace_BaselineToItemzId",
                table: "BaselineItemzJoinItemzTrace",
                column: "BaselineToItemzId");

            migrationBuilder.CreateIndex(
                name: "IX_BaselineItemzType_BaselineId",
                table: "BaselineItemzType",
                column: "BaselineId");

            migrationBuilder.CreateIndex(
                name: "IX_BaselineItemzTypeJoinBaselineItemz_BaselineItemzId",
                table: "BaselineItemzTypeJoinBaselineItemz",
                column: "BaselineItemzId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemzChangeHistory_ItemzId",
                table: "ItemzChangeHistory",
                column: "ItemzId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemzJoinItemzTrace_ToItemzId",
                table: "ItemzJoinItemzTrace",
                column: "ToItemzId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemzTypeJoinItemz_ItemzId",
                table: "ItemzTypeJoinItemz",
                column: "ItemzId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemzTypes_ProjectId",
                table: "ItemzTypes",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Name",
                table: "Projects",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaselineItemzHierarchy");

            migrationBuilder.DropTable(
                name: "BaselineItemzJoinItemzTrace");

            migrationBuilder.DropTable(
                name: "BaselineItemzTypeJoinBaselineItemz");

            migrationBuilder.DropTable(
                name: "ItemzChangeHistory");

            migrationBuilder.DropTable(
                name: "ItemzHierarchy");

            migrationBuilder.DropTable(
                name: "ItemzJoinItemzTrace");

            migrationBuilder.DropTable(
                name: "ItemzTypeJoinItemz");

            migrationBuilder.DropTable(
                name: "BaselineItemzType");

            migrationBuilder.DropTable(
                name: "BaselineItemz");

            migrationBuilder.DropTable(
                name: "ItemzTypes");

            migrationBuilder.DropTable(
                name: "Itemzs");

            migrationBuilder.DropTable(
                name: "Baseline");

            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}
