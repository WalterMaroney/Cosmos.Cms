using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CDT.Cosmos.Cms.Common.Migrations
{
    public partial class RemoveFontIconObsoleteLayoutItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articles_FontIcons_FontIconId",
                table: "Articles");

            migrationBuilder.DropTable(
                name: "FontIcons");

            migrationBuilder.DropIndex(
                name: "IX_Articles_FontIconId",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "BodyHeaderHtmlAttributes",
                table: "Layouts");

            migrationBuilder.DropColumn(
                name: "FooterHtmlAttributes",
                table: "Layouts");

            migrationBuilder.DropColumn(
                name: "PostFooterBlock",
                table: "Layouts");

            migrationBuilder.DropColumn(
                name: "FontIconId",
                table: "Articles");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BodyHeaderHtmlAttributes",
                table: "Layouts",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FooterHtmlAttributes",
                table: "Layouts",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostFooterBlock",
                table: "Layouts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FontIconId",
                table: "Articles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FontIcons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IconCode = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FontIcons", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_FontIconId",
                table: "Articles",
                column: "FontIconId");

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_FontIcons_FontIconId",
                table: "Articles",
                column: "FontIconId",
                principalTable: "FontIcons",
                principalColumn: "Id");
        }
    }
}
