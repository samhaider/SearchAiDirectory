using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SearchAiDirectory.Shared.Migrations
{
    /// <inheritdoc />
    public partial class UpdateToolCategoryConnection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tools_ToolCategories_CategoryID",
                schema: "dbo",
                table: "Tools");

            migrationBuilder.DropTable(
                name: "ToolCategories",
                schema: "dbo");

            migrationBuilder.AddForeignKey(
                name: "FK_Tools_Categories_CategoryID",
                schema: "dbo",
                table: "Tools",
                column: "CategoryID",
                principalSchema: "dbo",
                principalTable: "Categories",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tools_Categories_CategoryID",
                schema: "dbo",
                table: "Tools");

            migrationBuilder.CreateTable(
                name: "ToolCategories",
                schema: "dbo",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MetaDescription = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MetaKeywords = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolCategories", x => x.ID);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Tools_ToolCategories_CategoryID",
                schema: "dbo",
                table: "Tools",
                column: "CategoryID",
                principalSchema: "dbo",
                principalTable: "ToolCategories",
                principalColumn: "ID");
        }
    }
}
