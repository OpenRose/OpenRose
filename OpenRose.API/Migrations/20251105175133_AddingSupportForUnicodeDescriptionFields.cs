// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.


using Microsoft.EntityFrameworkCore.Migrations;
using System.IO;
using System.Linq;
using System.Reflection;


#nullable disable

namespace OpenRose.API.Migrations
{
    /// <inheritdoc />
    public partial class AddingSupportForUnicodeDescriptionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Projects",
                type: "NVARCHAR(MAX)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(MAX)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ItemzTypes",
                type: "NVARCHAR(MAX)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(MAX)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Itemzs",
                type: "NVARCHAR(MAX)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(MAX)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "BaselineItemzType",
                type: "NVARCHAR(MAX)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(MAX)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "BaselineItemz",
                type: "NVARCHAR(MAX)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(MAX)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Baseline",
                type: "NVARCHAR(MAX)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(MAX)",
                oldNullable: true);


			// Deploy new stored-proc files (these must NOT contain GO)
			var assembly = Assembly.GetExecutingAssembly();

			var newProcFiles = new[]
			{
				"userProcCreateBaselineByExistingBaselineID_202511051622.sql",
				"userProcCreateBaselineByItemzTypeID_202505111625.sql",
				"userProcCreateBaselineByProjectID_202511051626.sql"
			};

			foreach (var fileName in newProcFiles)
			{
				var resourceNames = assembly.GetManifestResourceNames()
					.Where(r => r.EndsWith(fileName));
				foreach (var resource in resourceNames)
				{
					using var stream = assembly.GetManifestResourceStream(resource);
					using var reader = new StreamReader(stream!);
					var sqlScript = reader.ReadToEnd();
					migrationBuilder.Sql(sqlScript); // single-call execution; script must not contain GO
				}
			}

		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

			// Restore old stored-proc files (these must also not contain GO)
			var assembly = Assembly.GetExecutingAssembly();

			var oldProcFiles = new[]
			{
				"userProcCreateBaselineByExistingBaselineID_202410300246.sql",
				"userProcCreateBaselineByItemzTypeID_202410300238.sql",
				"userProcCreateBaselineByProjectID_202410300219.sql"
			};

			foreach (var fileName in oldProcFiles)
			{
				var resourceNames = assembly.GetManifestResourceNames()
					.Where(r => r.EndsWith(fileName));
				foreach (var resource in resourceNames)
				{
					using var stream = assembly.GetManifestResourceStream(resource);
					using var reader = new StreamReader(stream!);
					var sqlScript = reader.ReadToEnd();
					migrationBuilder.Sql(sqlScript); // script must be executable as a single batch
				}
			}

			// Revert column types back to VARCHAR(MAX)
			migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Projects",
                type: "VARCHAR(MAX)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR(MAX)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ItemzTypes",
                type: "VARCHAR(MAX)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR(MAX)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Itemzs",
                type: "VARCHAR(MAX)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR(MAX)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "BaselineItemzType",
                type: "VARCHAR(MAX)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR(MAX)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "BaselineItemz",
                type: "VARCHAR(MAX)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR(MAX)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Baseline",
                type: "VARCHAR(MAX)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR(MAX)",
                oldNullable: true);
        }
    }
}
