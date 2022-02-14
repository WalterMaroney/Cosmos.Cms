using CDT.Cosmos.Cms.Common.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CDT.Cosmos.Cms.Common.Migrations
{
    public partial class UpdateArticleLock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConnectionId",
                table: "ArticleLock",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.RenameTable("ArticleLock", "dbo", "ArticleLocks", "dbo");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConnectionId",
                table: "ArticleLocks");

            migrationBuilder.RenameTable("ArticleLocks", "dbo", "ArticleLock", "dbo");
        }
    }
}
