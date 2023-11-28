using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogApi.Migrations
{
    /// <inheritdoc />
    public partial class AddLikes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasLike",
                table: "Posts");

            migrationBuilder.CreateTable(
                name: "Likes",
                columns: table => new
                {
                    LikedPostsId = table.Column<Guid>(type: "uuid", nullable: false),
                    LikedUsersId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Likes", x => new { x.LikedPostsId, x.LikedUsersId });
                    table.ForeignKey(
                        name: "FK_Likes_Posts_LikedPostsId",
                        column: x => x.LikedPostsId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Likes_Users_LikedUsersId",
                        column: x => x.LikedUsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Likes_LikedUsersId",
                table: "Likes",
                column: "LikedUsersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Likes");

            migrationBuilder.AddColumn<bool>(
                name: "HasLike",
                table: "Posts",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
