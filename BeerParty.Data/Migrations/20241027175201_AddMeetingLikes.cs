using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BeerParty.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMeetingLikes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Likes");

            migrationBuilder.AddColumn<long>(
                name: "MeetingId1",
                table: "MeetingReviews",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MeetingLikes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    MeetingId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingLikes_Meetings_MeetingId",
                        column: x => x.MeetingId,
                        principalTable: "Meetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MeetingLikes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MeetingReviewLikes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    MeetingReviewId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingReviewLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingReviewLikes_MeetingReviews_MeetingReviewId",
                        column: x => x.MeetingReviewId,
                        principalTable: "MeetingReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MeetingReviewLikes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MeetingReviews_MeetingId1",
                table: "MeetingReviews",
                column: "MeetingId1");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingLikes_MeetingId",
                table: "MeetingLikes",
                column: "MeetingId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingLikes_UserId",
                table: "MeetingLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingReviewLikes_MeetingReviewId",
                table: "MeetingReviewLikes",
                column: "MeetingReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingReviewLikes_UserId",
                table: "MeetingReviewLikes",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingReviews_Meetings_MeetingId1",
                table: "MeetingReviews",
                column: "MeetingId1",
                principalTable: "Meetings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MeetingReviews_Meetings_MeetingId1",
                table: "MeetingReviews");

            migrationBuilder.DropTable(
                name: "MeetingLikes");

            migrationBuilder.DropTable(
                name: "MeetingReviewLikes");

            migrationBuilder.DropIndex(
                name: "IX_MeetingReviews_MeetingId1",
                table: "MeetingReviews");

            migrationBuilder.DropColumn(
                name: "MeetingId1",
                table: "MeetingReviews");

            migrationBuilder.CreateTable(
                name: "Likes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MeetingId = table.Column<long>(type: "bigint", nullable: false),
                    MeetingReviewId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    MeetingId1 = table.Column<long>(type: "bigint", nullable: true),
                    UserId1 = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Likes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Likes_MeetingReviews_MeetingReviewId",
                        column: x => x.MeetingReviewId,
                        principalTable: "MeetingReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Likes_Meetings_MeetingId",
                        column: x => x.MeetingId,
                        principalTable: "Meetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Likes_Meetings_MeetingId1",
                        column: x => x.MeetingId1,
                        principalTable: "Meetings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Likes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Likes_Users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Likes_MeetingId",
                table: "Likes",
                column: "MeetingId");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_MeetingId1",
                table: "Likes",
                column: "MeetingId1");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_MeetingReviewId",
                table: "Likes",
                column: "MeetingReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_UserId",
                table: "Likes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_UserId1",
                table: "Likes",
                column: "UserId1");
        }
    }
}
