using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenRose.API.Migrations
{
    /// <inheritdoc />
    public partial class AddEstimationFieldsToItemzHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EstimationUnit",
                table: "ItemzHierarchy",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OwnEstimation",
                table: "ItemzHierarchy",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RolledUpEstimation",
                table: "ItemzHierarchy",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimationUnit",
                table: "ItemzHierarchy");

            migrationBuilder.DropColumn(
                name: "OwnEstimation",
                table: "ItemzHierarchy");

            migrationBuilder.DropColumn(
                name: "RolledUpEstimation",
                table: "ItemzHierarchy");
        }
    }
}
