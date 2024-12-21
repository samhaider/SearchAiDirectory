using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SearchAiDirectory.Shared.Migrations
{
    /// <inheritdoc />
    public partial class MinorTableChangeForTools : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LikeCount",
                schema: "dbo",
                table: "Tools");

            migrationBuilder.DropColumn(
                name: "Created",
                schema: "dbo",
                table: "ToolCategories");
        }
    }
}
