using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitaLibrary.Migrations
{
    /// <inheritdoc />
    public partial class butonlaaar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bookmarks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AcademicWorkId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookmarks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookmarks_AcademicWorks_AcademicWorkId",
                        column: x => x.AcademicWorkId,
                        principalTable: "AcademicWorks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bookmarks_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Favorites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AcademicWorkId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Favorites_AcademicWorks_AcademicWorkId",
                        column: x => x.AcademicWorkId,
                        principalTable: "AcademicWorks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Favorites_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_AcademicWorkId",
                table: "Bookmarks",
                column: "AcademicWorkId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_UserId_AcademicWorkId",
                table: "Bookmarks",
                columns: new[] { "UserId", "AcademicWorkId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_AcademicWorkId",
                table: "Favorites",
                column: "AcademicWorkId");

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_UserId_AcademicWorkId",
                table: "Favorites",
                columns: new[] { "UserId", "AcademicWorkId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookmarks");

            migrationBuilder.DropTable(
                name: "Favorites");
        }
    }
}
