using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gameProject.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    auth_uuid = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Quizzes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    quiz_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    creator_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quizzes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quizzes_Users_creator_id",
                        column: x => x.creator_id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    room_code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    host_user_id = table.Column<int>(type: "int", nullable: false),
                    max_players = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rooms_Users_host_user_id",
                        column: x => x.host_user_id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "questions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    quiz_id = table.Column<int>(type: "int", nullable: false),
                    question_text = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    question_order = table.Column<int>(type: "int", nullable: false),
                    time_limit = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<int>(type: "int", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    quiz_id1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_questions_Quizzes_quiz_id",
                        column: x => x.quiz_id,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    room_id = table.Column<int>(type: "int", nullable: false),
                    quiz_id = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    current_question_number = table.Column<int>(type: "int", nullable: false),
                    current_question_end_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    started_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ended_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameSessions_Quizzes_quiz_id",
                        column: x => x.quiz_id,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameSessions_Rooms_room_id",
                        column: x => x.room_id,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    room_id = table.Column<int>(type: "int", nullable: false),
                    is_ready = table.Column<bool>(type: "bit", nullable: false),
                    is_connected = table.Column<bool>(type: "bit", nullable: false),
                    joined_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Players_Rooms_room_id",
                        column: x => x.room_id,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Players_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Choices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    question_id = table.Column<int>(type: "int", nullable: false),
                    choice_text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    is_correct = table.Column<bool>(type: "bit", nullable: false),
                    choice_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Choices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Choices_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Game Records",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    game_session_id = table.Column<int>(type: "int", nullable: false),
                    quiz_id = table.Column<int>(type: "int", nullable: false),
                    winner_player_id = table.Column<int>(type: "int", nullable: true),
                    total_players = table.Column<int>(type: "int", nullable: false),
                    game_duration_seconds = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Game Records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Game Records_GameSessions_game_session_id",
                        column: x => x.game_session_id,
                        principalTable: "GameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Game Records_Players_winner_player_id",
                        column: x => x.winner_player_id,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Game Records_Quizzes_quiz_id",
                        column: x => x.quiz_id,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Leaderboards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    game_session_id = table.Column<int>(type: "int", nullable: false),
                    player_id = table.Column<int>(type: "int", nullable: false),
                    score = table.Column<int>(type: "int", nullable: false),
                    rank = table.Column<int>(type: "int", nullable: false),
                    correct_answers = table.Column<int>(type: "int", nullable: false),
                    total_answers = table.Column<int>(type: "int", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leaderboards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leaderboards_GameSessions_game_session_id",
                        column: x => x.game_session_id,
                        principalTable: "GameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Leaderboards_Players_player_id",
                        column: x => x.player_id,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    player_id = table.Column<int>(type: "int", nullable: false),
                    game_session_id = table.Column<int>(type: "int", nullable: false),
                    question_id = table.Column<int>(type: "int", nullable: false),
                    choice_id = table.Column<int>(type: "int", nullable: false),
                    answer_time = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    time_taken_seconds = table.Column<double>(type: "float", nullable: false),
                    is_correct = table.Column<bool>(type: "bit", nullable: false),
                    points_earned = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerAnswers_Choices_choice_id",
                        column: x => x.choice_id,
                        principalTable: "Choices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerAnswers_GameSessions_game_session_id",
                        column: x => x.game_session_id,
                        principalTable: "GameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerAnswers_Players_player_id",
                        column: x => x.player_id,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerAnswers_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Choices_QuestionId",
                table: "Choices",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_Game Records_winner_player_id",
                table: "Game Records",
                column: "winner_player_id");

            migrationBuilder.CreateIndex(
                name: "IX_GameRecords_CreatedAt",
                table: "Game Records",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_GameRecords_GameSessionId",
                table: "Game Records",
                column: "game_session_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameRecords_QuizId",
                table: "Game Records",
                column: "quiz_id");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_QuizId",
                table: "GameSessions",
                column: "quiz_id");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_RoomId",
                table: "GameSessions",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_Status",
                table: "GameSessions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_Leaderboards_GameSessionId",
                table: "Leaderboards",
                column: "game_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_Leaderboards_PlayerId",
                table: "Leaderboards",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_Leaderboards_Session_Player",
                table: "Leaderboards",
                columns: new[] { "game_session_id", "player_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Leaderboards_Session_Score",
                table: "Leaderboards",
                columns: new[] { "game_session_id", "score" });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerAnswers_choice_id",
                table: "PlayerAnswers",
                column: "choice_id");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerAnswers_GameSessionId",
                table: "PlayerAnswers",
                column: "game_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerAnswers_Player_Question_Session",
                table: "PlayerAnswers",
                columns: new[] { "player_id", "question_id", "game_session_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerAnswers_PlayerId",
                table: "PlayerAnswers",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerAnswers_QuestionId",
                table: "PlayerAnswers",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_Players_RoomId",
                table: "Players",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "IX_Players_UserId",
                table: "Players",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Players_UserId_RoomId",
                table: "Players",
                columns: new[] { "user_id", "room_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_QuizId",
                table: "questions",
                column: "quiz_id");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_QuizId_Order",
                table: "questions",
                columns: new[] { "quiz_id", "question_order" });

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_CreatorId",
                table: "Quizzes",
                column: "creator_id");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_HostUserId",
                table: "Rooms",
                column: "host_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_IsActive",
                table: "Rooms",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_RoomCode",
                table: "Rooms",
                column: "room_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_AuthUuid",
                table: "Users",
                column: "auth_uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "username");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Game Records");

            migrationBuilder.DropTable(
                name: "Leaderboards");

            migrationBuilder.DropTable(
                name: "PlayerAnswers");

            migrationBuilder.DropTable(
                name: "Choices");

            migrationBuilder.DropTable(
                name: "GameSessions");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "questions");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "Quizzes");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
