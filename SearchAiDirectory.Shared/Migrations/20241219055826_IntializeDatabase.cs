using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SearchAiDirectory.Shared.Migrations
{
    /// <inheritdoc />
    public partial class IntializeDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "AppLogs",
                schema: "dbo",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserIp = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Host = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Url = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Code = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StackTrace = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppLogs", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ToolCategories",
                schema: "dbo",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolCategories", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "UserCodes",
                schema: "dbo",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(99)", maxLength: 99, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCodes", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Tools",
                schema: "dbo",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryID = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WebsiteContent = table.Column<string>(type: "nvarchar(max)", maxLength: 7500, nullable: true),
                    MetaDescription = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MetaKeywords = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PriceModel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tools", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tools_ToolCategories_CategoryID",
                        column: x => x.CategoryID,
                        principalSchema: "dbo",
                        principalTable: "ToolCategories",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "dbo",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(99)", maxLength: 99, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(99)", maxLength: 99, nullable: false),
                    SaltCode = table.Column<string>(type: "nvarchar(99)", maxLength: 99, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(99)", maxLength: 99, nullable: false),
                    Avatar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Registration = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    ToolID = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Users_Tools_ToolID",
                        column: x => x.ToolID,
                        principalSchema: "dbo",
                        principalTable: "Tools",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tools_CategoryID",
                schema: "dbo",
                table: "Tools",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ToolID",
                schema: "dbo",
                table: "Users",
                column: "ToolID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppLogs",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UserCodes",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Tools",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ToolCategories",
                schema: "dbo");
        }
    }
}
