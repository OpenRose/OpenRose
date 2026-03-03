using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenRose.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTagSupportInItemzAndBaselineItemz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "Itemzs",
                type: "NVARCHAR(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "BaselineItemz",
                type: "NVARCHAR(512)",
                maxLength: 512,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Itemzs");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "BaselineItemz");
        }
    }
}
