using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SearchAiDirectory.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddSlugToTools : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                schema: "dbo",
                table: "Tools",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaDescription",
                schema: "dbo",
                table: "ToolCategories",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MetaKeywords",
                schema: "dbo",
                table: "ToolCategories",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                schema: "dbo",
                table: "ToolCategories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Slug",
                schema: "dbo",
                table: "Tools");

            migrationBuilder.DropColumn(
                name: "MetaDescription",
                schema: "dbo",
                table: "ToolCategories");

            migrationBuilder.DropColumn(
                name: "MetaKeywords",
                schema: "dbo",
                table: "ToolCategories");

            migrationBuilder.DropColumn(
                name: "Slug",
                schema: "dbo",
                table: "ToolCategories");
        }
    }
}
