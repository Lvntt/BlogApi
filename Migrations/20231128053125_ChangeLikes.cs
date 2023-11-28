using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogApi.Migrations
{
    /// <inheritdoc />
    public partial class ChangeLikes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Posts_LikedPostsId",
                table: "Likes");

            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Users_LikedUsersId",
                table: "Likes");

            migrationBuilder.RenameColumn(
                name: "LikedUsersId",
                table: "Likes",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "LikedPostsId",
                table: "Likes",
                newName: "PostId");

            migrationBuilder.RenameIndex(
                name: "IX_Likes_LikedUsersId",
                table: "Likes",
                newName: "IX_Likes_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Posts_PostId",
                table: "Likes",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Users_UserId",
                table: "Likes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Posts_PostId",
                table: "Likes");

            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Users_UserId",
                table: "Likes");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Likes",
                newName: "LikedUsersId");

            migrationBuilder.RenameColumn(
                name: "PostId",
                table: "Likes",
                newName: "LikedPostsId");

            migrationBuilder.RenameIndex(
                name: "IX_Likes_UserId",
                table: "Likes",
                newName: "IX_Likes_LikedUsersId");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Posts_LikedPostsId",
                table: "Likes",
                column: "LikedPostsId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Users_LikedUsersId",
                table: "Likes",
                column: "LikedUsersId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
