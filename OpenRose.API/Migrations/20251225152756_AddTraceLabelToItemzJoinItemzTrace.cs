using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenRose.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTraceLabelToItemzJoinItemzTrace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TraceLabel",
                table: "ItemzJoinItemzTrace",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TraceLabel",
                table: "BaselineItemzJoinItemzTrace",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TraceLabel",
                table: "ItemzJoinItemzTrace");

            migrationBuilder.DropColumn(
                name: "TraceLabel",
                table: "BaselineItemzJoinItemzTrace");
        }
    }
}
