using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable enable

namespace TestFirstProject.Migrations
{
    /// <inheritdoc />
    public partial class AddSnakeGame : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- game_scores table ---
            migrationBuilder.CreateTable(
                name: "game_scores",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    score = table.Column<int>(type: "integer", nullable: false),
                    difficulty = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    duration_seconds = table.Column<int>(type: "integer", nullable: false),
                    enemies_defeated = table.Column<int>(type: "integer", nullable: false),
                    food_eaten = table.Column<int>(type: "integer", nullable: false),
                    max_combo = table.Column<int>(type: "integer", nullable: false),
                    rating_change = table.Column<int>(type: "integer", nullable: false),
                    played_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_scores", x => x.id);
                    table.ForeignKey(
                        name: "FK_game_scores_auth_users_user_id",
                        column: x => x.user_id,
                        principalTable: "auth_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_game_scores_score",
                table: "game_scores",
                column: "score");

            migrationBuilder.CreateIndex(
                name: "IX_game_scores_played_at",
                table: "game_scores",
                column: "played_at");

            migrationBuilder.CreateIndex(
                name: "IX_game_scores_user_id_played_at",
                table: "game_scores",
                columns: new[] { "user_id", "played_at" });

            // --- player_ratings table ---
            migrationBuilder.CreateTable(
                name: "player_ratings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rating = table.Column<int>(type: "integer", nullable: false, defaultValue: 1000),
                    games_played = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    highest_score = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_played_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_ratings", x => x.id);
                    table.ForeignKey(
                        name: "FK_player_ratings_auth_users_user_id",
                        column: x => x.user_id,
                        principalTable: "auth_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_player_ratings_user_id",
                table: "player_ratings",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_player_ratings_rating",
                table: "player_ratings",
                column: "rating");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "player_ratings");
            migrationBuilder.DropTable(name: "game_scores");
        }
    }
}
