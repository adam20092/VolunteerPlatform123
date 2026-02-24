using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace volunteerplatform.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchemaV3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CertificateCode",
                table: "Enrolments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CertificateCode",
                table: "Enrolments");
        }
    }
}
