using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenRose.API.Migrations
{
    /// <inheritdoc />
    public partial class AddEstimationFieldsToBaselineItemzHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EstimationUnit",
                table: "BaselineItemzHierarchy",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OwnEstimation",
                table: "BaselineItemzHierarchy",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RolledUpEstimation",
                table: "BaselineItemzHierarchy",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimationUnit",
                table: "BaselineItemzHierarchy");

            migrationBuilder.DropColumn(
                name: "OwnEstimation",
                table: "BaselineItemzHierarchy");

            migrationBuilder.DropColumn(
                name: "RolledUpEstimation",
                table: "BaselineItemzHierarchy");
        }
    }
}
