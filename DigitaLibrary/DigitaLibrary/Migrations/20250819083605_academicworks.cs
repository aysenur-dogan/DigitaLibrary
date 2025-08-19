using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitaLibrary.Migrations
{
    /// <inheritdoc />
    public partial class academicworks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorTitle",
                table: "AcademicWorks",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverImagePath",
                table: "AcademicWorks",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorTitle",
                table: "AcademicWorks");

            migrationBuilder.DropColumn(
                name: "CoverImagePath",
                table: "AcademicWorks");
        }
    }
}
