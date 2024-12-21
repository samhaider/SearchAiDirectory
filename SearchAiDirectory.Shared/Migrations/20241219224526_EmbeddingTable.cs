using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SearchAiDirectory.Shared.Migrations
{
    /// <inheritdoc />
    public partial class EmbeddingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Embeddings",
                schema: "dbo",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToolID = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmbeddingCode = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Embeddings", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Embeddings_Tools_ToolID",
                        column: x => x.ToolID,
                        principalSchema: "dbo",
                        principalTable: "Tools",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Embeddings_ToolID",
                schema: "dbo",
                table: "Embeddings",
                column: "ToolID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Embeddings",
                schema: "dbo");
        }
    }
}
