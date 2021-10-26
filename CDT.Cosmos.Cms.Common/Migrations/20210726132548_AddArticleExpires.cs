using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace CDT.Cosmos.Cms.Common.Migrations
{
    public partial class AddArticleExpires : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Expires",
                table: "Articles",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Expires",
                table: "Articles");
        }
    }
}
