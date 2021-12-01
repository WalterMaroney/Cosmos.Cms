using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CDT.Cosmos.Cms.Common.Migrations
{
    public partial class ArticleLockTemplateUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CommunityLayoutId",
                table: "Templates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LayoutId",
                table: "Templates",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommunityLayoutId",
                table: "Layouts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ArticleLock",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newid()"),
                    IdentityUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    LockSetDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleLock", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArticleLock_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArticleLock_AspNetUsers_IdentityUserId",
                        column: x => x.IdentityUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Templates_LayoutId",
                table: "Templates",
                column: "LayoutId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleLock_ArticleId",
                table: "ArticleLock",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleLock_IdentityUserId",
                table: "ArticleLock",
                column: "IdentityUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Templates_Layouts_LayoutId",
                table: "Templates",
                column: "LayoutId",
                principalTable: "Layouts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Templates_Layouts_LayoutId",
                table: "Templates");

            migrationBuilder.DropTable(
                name: "ArticleLock");

            migrationBuilder.DropIndex(
                name: "IX_Templates_LayoutId",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "CommunityLayoutId",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "LayoutId",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "CommunityLayoutId",
                table: "Layouts");
        }
    }
}
