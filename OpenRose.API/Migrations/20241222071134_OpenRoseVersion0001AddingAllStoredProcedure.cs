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
    public partial class OpenRoseVersion0001AddingAllStoredProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

			var assembly = Assembly.GetExecutingAssembly();
			// EXPLANATION: In this Where clause, we are picking up specific .sql files
			// instead of files that EndsWith(".sql") only because we want to make sure
			// that if we have to undo the changes via DOWN method then we are removing 
			// particular .sql file. For now, becuase we are introducing a new .sql file 
			// as a stored procedure then we are fine to have this stored procedure dropped 
			// in the DOWN method below. In the future we have to look at best practice for
			// handling different versionf of the .sql (stored procedure) files. This DROP 
			// option within DOWN method does not help in cases where we have updated existing 
			// Stored Procedure.

			var sqlFiles = assembly.GetManifestResourceNames().
						Where(file => file.EndsWith("userProcCreateBaselineByExistingBaselineID_202410300246.sql")
						|| file.EndsWith("userProcCreateBaselineByItemzTypeID_202410300238.sql")
						|| file.EndsWith("userProcCreateBaselineByProjectID_202410300219.sql")
						|| file.EndsWith("userProcDeleteAllOrphanedBaselineItemz_202407251305.sql")
						|| file.EndsWith("userProcDeleteItemzHierarchyRecordsByItemzTypeId_202412091028.sql")
						|| file.EndsWith("userProcDeleteItemzHierarchyRecordsByProjectId_202412090713.sql")
						|| file.EndsWith("userProcDeleteSingleItemzByItemzID_202406081944.sql")
						|| file.EndsWith("userProcUpdateBaselineItemz_202408060015.sql")
						);
			foreach (var sqlFile in sqlFiles)
			{
				using (Stream? stream = assembly.GetManifestResourceStream(sqlFile))
				using (StreamReader reader = new StreamReader(stream!))
				{
					var sqlScript = reader.ReadToEnd();
					migrationBuilder.Sql(sqlScript);
				}
			}
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
			var dropuserProcCreateBaselineByExistingBaselineID = "DROP PROC userProcCreateBaselineByExistingBaselineID";
			migrationBuilder.Sql(dropuserProcCreateBaselineByExistingBaselineID);

			var dropuserProcCreateBaselineByItemzTypeID = "DROP PROC userProcCreateBaselineByItemzTypeID";
			migrationBuilder.Sql(dropuserProcCreateBaselineByItemzTypeID);
			
			var dropuserProcCreateBaselineByProjectID = "DROP PROC userProcCreateBaselineByProjectID";
			migrationBuilder.Sql(dropuserProcCreateBaselineByProjectID);

			var dropuserProcDeleteAllOrphanedBaselineItemz = "DROP PROC userProcDeleteAllOrphanedBaselineItemz";
			migrationBuilder.Sql(dropuserProcDeleteAllOrphanedBaselineItemz);

			var dropuserProcDeleteItemzHierarchyRecordsByItemzTypeId = "DROP PROC userProcDeleteItemzHierarchyRecordsByItemzTypeId";
			migrationBuilder.Sql(dropuserProcDeleteItemzHierarchyRecordsByItemzTypeId);

			var dropuserProcDeleteItemzHierarchyRecordsByProjectId = "DROP PROC userProcDeleteItemzHierarchyRecordsByProjectId";
			migrationBuilder.Sql(dropuserProcDeleteItemzHierarchyRecordsByProjectId);

			var dropuserProcDeleteSingleItemzByItemzID = "DROP PROC userProcDeleteSingleItemzByItemzID";
			migrationBuilder.Sql(dropuserProcDeleteSingleItemzByItemzID);

			var dropuserProcUpdateBaselineItemz = "DROP PROC userProcUpdateBaselineItemz";
			migrationBuilder.Sql(dropuserProcUpdateBaselineItemz);
		}
	}
}
