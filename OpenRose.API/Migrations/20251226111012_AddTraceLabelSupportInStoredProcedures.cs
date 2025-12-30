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
    public partial class AddTraceLabelSupportInStoredProcedures : Migration
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
						Where(file => file.EndsWith("userProcCopyRecordWithChildrenByRecordID_202512251653.sql")
						|| file.EndsWith("userProcCreateBaselineByExistingBaselineID_202512251705.sql")
						|| file.EndsWith("userProcCreateBaselineByItemzTypeID_202512251739.sql")
						|| file.EndsWith("userProcCreateBaselineByProjectID_202512251819.sql")
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
						Where(file => file.EndsWith("userProcCopyRecordWithChildrenByRecordID_202503221207.sql")
						|| file.EndsWith("userProcCreateBaselineByExistingBaselineID_202511051622.sql")
						|| file.EndsWith("userProcCreateBaselineByItemzTypeID_202505111625.sql")
						|| file.EndsWith("userProcCreateBaselineByProjectID_202511051626.sql")
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
    }
}
