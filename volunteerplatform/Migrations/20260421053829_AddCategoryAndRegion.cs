using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace volunteerplatform.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryAndRegion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Initiatives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "Initiatives",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Initiatives");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "Initiatives");
        }
    }
}
