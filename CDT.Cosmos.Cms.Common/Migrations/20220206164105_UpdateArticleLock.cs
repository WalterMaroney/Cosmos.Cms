using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CDT.Cosmos.Cms.Common.Migrations
{
    public partial class UpdateArticleLock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConnectionId",
                table: "ArticleLocks",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConnectionId",
                table: "ArticleLocks");
        }
    }
}
