using FluentAssertions;
using gameProject.Models;
using gameProject.Repositories;
using gameProject.Services;
using gameProject.Tests.Helpers;
using Xunit;

namespace gameProject.Tests.Services
{
    public class GameStatsServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly GameStatsService _service;
        private readonly PlayerAnswerRepository _playerAnswerRepository;
        private readonly LeaderboardRepository _leaderboardRepository;

        public GameStatsServiceTests()
        {
            _context = TestDbHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var logger = TestDbHelper.CreateMockLogger<GameStatsService>();
            var paLogger = TestDbHelper.CreateMockLogger<PlayerAnswerRepository>();
            var lbLogger = TestDbHelper.CreateMockLogger<LeaderboardRepository>();

            _playerAnswerRepository = new PlayerAnswerRepository(_context, paLogger);
            _leaderboardRepository = new LeaderboardRepository(_context, lbLogger);

            _service = new GameStatsService(
                _context,
                _playerAnswerRepository,
                _leaderboardRepository,
                logger);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Theory]
        [InlineData(true, 5.0, 30, 158)] // Correct, fast answer (5 seconds out of 30)
        [InlineData(true, 15.0, 30, 125)] // Correct, medium speed (15 seconds out of 30)
        [InlineData(true, 29.0, 30, 101)] // Correct, slow answer (29 seconds out of 30)
        [InlineData(false, 5.0, 30, 0)] // Wrong answer, no points
        public void CalculatePointsForAnswer_ShouldCalculateCorrectPoints(
            bool isCorrect, double timeTaken, int timeLimit, int expectedPoints)
        {
            // Act
            var points = _service.CalculatePointsForAnswer(isCorrect, timeTaken, timeLimit);

            // Assert
            points.Should().BeCloseTo(expectedPoints, 5); // Allow small variance
        }

        [Fact]
        public async Task UpdatePlayerLeaderboardAsync_ShouldCreateNewEntry_WhenPlayerHasNoEntry()
        {
            // Arrange
            var user = new User { AuthUuid = "test-uuid", Username = "testplayer" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var room = new Room { RoomCode = "ABC123", HostUserId = user.Id };
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            var player = new Player { UserId = user.Id, RoomId = room.Id };
            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            var quiz = new Quiz { QuizName = "Test Quiz", CreatorId = user.Id };
            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            var gameSession = new GameSession
            {
                RoomId = room.Id,
                QuizId = quiz.Id,
                Status = GameStatus.Active
            };
            _context.GameSessions.Add(gameSession);
            await _context.SaveChangesAsync();

            // Act
            await _service.UpdatePlayerLeaderboardAsync(player.Id, gameSession.Id, 100, true);

            // Assert
            var leaderboard = await _leaderboardRepository.GetByGameSessionAndPlayerAsync(
                gameSession.Id, player.Id);

            leaderboard.Should().NotBeNull();
            leaderboard!.Score.Should().Be(100);
            leaderboard.CorrectAnswers.Should().Be(1);
            leaderboard.TotalAnswers.Should().Be(1);
        }

        [Fact]
        public async Task UpdatePlayerLeaderboardAsync_ShouldUpdateExistingEntry_WhenPlayerHasEntry()
        {
            // Arrange
            var user = new User { AuthUuid = "test-uuid-2", Username = "testplayer2" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var room = new Room { RoomCode = "DEF456", HostUserId = user.Id };
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            var player = new Player { UserId = user.Id, RoomId = room.Id };
            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            var quiz = new Quiz { QuizName = "Test Quiz 2", CreatorId = user.Id };
            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            var gameSession = new GameSession
            {
                RoomId = room.Id,
                QuizId = quiz.Id,
                Status = GameStatus.Active
            };
            _context.GameSessions.Add(gameSession);
            await _context.SaveChangesAsync();

            var existingLeaderboard = new Leaderboard
            {
                GameSessionId = gameSession.Id,
                PlayerId = player.Id,
                Score = 50,
                CorrectAnswers = 1,
                TotalAnswers = 1,
                Rank = 1
            };
            _context.Leaderboards.Add(existingLeaderboard);
            await _context.SaveChangesAsync();

            // Act - Add another answer
            await _service.UpdatePlayerLeaderboardAsync(player.Id, gameSession.Id, 150, true);

            // Assert
            var leaderboard = await _leaderboardRepository.GetByGameSessionAndPlayerAsync(
                gameSession.Id, player.Id);

            leaderboard.Should().NotBeNull();
            leaderboard!.Score.Should().Be(200); // 50 + 150
            leaderboard.CorrectAnswers.Should().Be(2); // 1 + 1
            leaderboard.TotalAnswers.Should().Be(2); // 1 + 1
        }

        [Fact]
        public async Task GetCurrentLeaderboardAsync_ShouldReturnOrderedLeaderboard()
        {
            // Arrange
            var user1 = new User { AuthUuid = "uuid1", Username = "player1" };
            var user2 = new User { AuthUuid = "uuid2", Username = "player2" };
            var user3 = new User { AuthUuid = "uuid3", Username = "player3" };
            _context.Users.AddRange(user1, user2, user3);
            await _context.SaveChangesAsync();

            var room = new Room { RoomCode = "GHI789", HostUserId = user1.Id };
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            var player1 = new Player { UserId = user1.Id, RoomId = room.Id };
            var player2 = new Player { UserId = user2.Id, RoomId = room.Id };
            var player3 = new Player { UserId = user3.Id, RoomId = room.Id };
            _context.Players.AddRange(player1, player2, player3);
            await _context.SaveChangesAsync();

            var quiz = new Quiz { QuizName = "Test Quiz 3", CreatorId = user1.Id };
            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            var gameSession = new GameSession
            {
                RoomId = room.Id,
                QuizId = quiz.Id,
                Status = GameStatus.Active
            };
            _context.GameSessions.Add(gameSession);
            await _context.SaveChangesAsync();

            // Add leaderboard entries with different scores
            _context.Leaderboards.AddRange(
                new Leaderboard
                {
                    GameSessionId = gameSession.Id,
                    PlayerId = player1.Id,
                    Score = 150,
                    Rank = 1
                },
                new Leaderboard
                {
                    GameSessionId = gameSession.Id,
                    PlayerId = player2.Id,
                    Score = 250, // Highest
                    Rank = 1
                },
                new Leaderboard
                {
                    GameSessionId = gameSession.Id,
                    PlayerId = player3.Id,
                    Score = 100, // Lowest
                    Rank = 1
                }
            );
            await _context.SaveChangesAsync();

            // Act
            var leaderboard = await _service.GetCurrentLeaderboardAsync(gameSession.Id);

            // Assert
            leaderboard.Should().HaveCount(3);
            leaderboard.First().Key.Should().Be("player2"); // Highest score
            leaderboard.First().Value.Should().Be(250);
            leaderboard.Last().Key.Should().Be("player3"); // Lowest score
            leaderboard.Last().Value.Should().Be(100);
        }

        [Fact]
        public async Task CalculatePlayerScoreAsync_ShouldReturnTotalPoints()
        {
            // Arrange
            var user = new User { AuthUuid = "calc-uuid", Username = "calcplayer" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var room = new Room { RoomCode = "JKL012", HostUserId = user.Id };
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            var player = new Player { UserId = user.Id, RoomId = room.Id };
            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            var quiz = new Quiz { QuizName = "Score Test Quiz", CreatorId = user.Id };
            _context.Quizzes.Add(quiz);

            var question1 = new Question { QuizId = quiz.Id, QuestionText = "Q1", QuestionOrder = 1 };
            var question2 = new Question { QuizId = quiz.Id, QuestionText = "Q2", QuestionOrder = 2 };
            _context.Questions.AddRange(question1, question2);
            await _context.SaveChangesAsync();

            var choice1 = new Choice { QuestionId = question1.Id, ChoiceText = "A", IsCorrect = true };
            var choice2 = new Choice { QuestionId = question2.Id, ChoiceText = "B", IsCorrect = true };
            _context.Choices.AddRange(choice1, choice2);
            await _context.SaveChangesAsync();

            var gameSession = new GameSession
            {
                RoomId = room.Id,
                QuizId = quiz.Id,
                Status = GameStatus.Active
            };
            _context.GameSessions.Add(gameSession);
            await _context.SaveChangesAsync();

            // Add player answers with points
            _context.PlayerAnswers.AddRange(
                new PlayerAnswer
                {
                    PlayerId = player.Id,
                    GameSessionId = gameSession.Id,
                    QuestionId = question1.Id,
                    ChoiceId = choice1.Id,
                    IsCorrect = true,
                    PointsEarned = 120
                },
                new PlayerAnswer
                {
                    PlayerId = player.Id,
                    GameSessionId = gameSession.Id,
                    QuestionId = question2.Id,
                    ChoiceId = choice2.Id,
                    IsCorrect = true,
                    PointsEarned = 130
                }
            );
            await _context.SaveChangesAsync();

            // Act
            var score = await _service.CalculatePlayerScoreAsync(player.Id, gameSession.Id);

            // Assert
            score.Should().Be(250); // 120 + 130
        }
    }
}