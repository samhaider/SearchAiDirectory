using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SearchAiDirectory.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddNewCategoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tools_ToolCategories_CategoryID",
                schema: "dbo",
                table: "Tools");

            migrationBuilder.AlterColumn<long>(
                name: "CategoryID",
                schema: "dbo",
                table: "Tools",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "LikeCount",
                schema: "dbo",
                table: "Tools",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                schema: "dbo",
                table: "ToolCategories",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Categories",
                schema: "dbo",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MetaDescription = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MetaKeywords = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.ID);
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tools_ToolCategories_CategoryID",
                schema: "dbo",
                table: "Tools");

            migrationBuilder.DropTable(
                name: "Categories",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "LikeCount",
                schema: "dbo",
                table: "Tools");

            migrationBuilder.DropColumn(
                name: "Created",
                schema: "dbo",
                table: "ToolCategories");

            migrationBuilder.AlterColumn<long>(
                name: "CategoryID",
                schema: "dbo",
                table: "Tools",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tools_ToolCategories_CategoryID",
                schema: "dbo",
                table: "Tools",
                column: "CategoryID",
                principalSchema: "dbo",
                principalTable: "ToolCategories",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
