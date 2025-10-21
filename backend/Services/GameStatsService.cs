using Microsoft.EntityFrameworkCore;
using gameProject.Data;
using gameProject.Repositories.Interfaces;

namespace gameProject.Services
{
    public class GameStatsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPlayerAnswerRepository _playerAnswerRepository;
        private readonly ILeaderboardRepository _leaderboardRepository;
        private readonly ILogger<GameStatsService> _logger;

        public GameStatsService(
            ApplicationDbContext context,
            IPlayerAnswerRepository playerAnswerRepository,
            ILeaderboardRepository leaderboardRepository,
            ILogger<GameStatsService> logger)
        {
            _context = context;
            _playerAnswerRepository = playerAnswerRepository;
            _leaderboardRepository = leaderboardRepository;
            _logger = logger;
        }

        /// <summary>
        /// Gets the leaderboard for a specific game
        /// </summary>
        /// <param name="gameRecordId">The ID of the game record</param>
        /// <returns>Dictionary with username as key and score as value</returns>
        public async Task<Dictionary<string, int>> GetLeaderboardForGameAsync(int gameRecordId)
        {
            try
            {
                // Get the game record with game session
                var gameRecord = await _context.GameRecords
                    .Include(gr => gr.GameSession)
                    .FirstOrDefaultAsync(gr => gr.Id == gameRecordId);

                if (gameRecord == null)
                {
                    _logger.LogWarning("Game record with ID {GameRecordId} not found", gameRecordId);
                    return new Dictionary<string, int>();
                }

                // Get leaderboard from the game session
                var leaderboard = await _context.Leaderboards
                    .Where(l => l.GameSessionId == gameRecord.GameSessionId)
                    .Include(l => l.Player)
                        .ThenInclude(p => p.User)
                    .OrderByDescending(l => l.Score)
                    .ToDictionaryAsync(l => l.Player.User.Username, l => l.Score);

                _logger.LogInformation("Retrieved leaderboard for game record {GameRecordId} with {Count} entries", 
                    gameRecordId, leaderboard.Count);

                return leaderboard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving leaderboard for game record {GameRecordId}", gameRecordId);
                throw;
            }
        }

        /// <summary>
        /// Gets the current leaderboard for an active game session
        /// </summary>
        /// <param name="gameSessionId">The ID of the game session</param>
        /// <returns>Dictionary with username as key and score as value</returns>
        public async Task<Dictionary<string, int>> GetCurrentLeaderboardAsync(int gameSessionId)
        {
            try
            {
                var leaderboard = await _context.Leaderboards
                    .Where(l => l.GameSessionId == gameSessionId)
                    .Include(l => l.Player)
                        .ThenInclude(p => p.User)
                    .OrderByDescending(l => l.Score)
                    .ToDictionaryAsync(l => l.Player.User.Username, l => l.Score);

                _logger.LogInformation("Retrieved current leaderboard for game session {GameSessionId} with {Count} entries", 
                    gameSessionId, leaderboard.Count);

                return leaderboard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current leaderboard for game session {GameSessionId}", gameSessionId);
                throw;
            }
        }

        /// <summary>
        /// Calculates score for a player based on their answers
        /// </summary>
        /// <param name="playerId">The ID of the player</param>
        /// <param name="gameSessionId">The ID of the game session</param>
        /// <returns>The total score</returns>
        public async Task<int> CalculatePlayerScoreAsync(int playerId, int gameSessionId)
        {
            try
            {
                var playerAnswers = await _playerAnswerRepository.GetByPlayerAndGameSessionAsync(playerId, gameSessionId);
                var score = playerAnswers.Sum(pa => pa.PointsEarned);

                _logger.LogInformation("Calculated score {Score} for player {PlayerId} in game {GameSessionId}", 
                    score, playerId, gameSessionId);

                return score;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating score for player {PlayerId}", playerId);
                throw;
            }
        }

        /// <summary>
        /// Updates or creates leaderboard entry for a player after they answer a question
        /// </summary>
        public async Task UpdatePlayerLeaderboardAsync(int playerId, int gameSessionId, int pointsEarned, bool isCorrect)
        {
            try
            {
                var leaderboardEntry = await _leaderboardRepository.GetByGameSessionAndPlayerAsync(gameSessionId, playerId);

                if (leaderboardEntry == null)
                {
                    // Create new leaderboard entry
                    leaderboardEntry = new Models.Leaderboard
                    {
                        GameSessionId = gameSessionId,
                        PlayerId = playerId,
                        Score = pointsEarned,
                        CorrectAnswers = isCorrect ? 1 : 0,
                        TotalAnswers = 1
                    };
                    await _leaderboardRepository.CreateAsync(leaderboardEntry);
                }
                else
                {
                    // Update existing entry
                    leaderboardEntry.Score += pointsEarned;
                    leaderboardEntry.TotalAnswers++;
                    if (isCorrect)
                    {
                        leaderboardEntry.CorrectAnswers++;
                    }
                    await _leaderboardRepository.UpdateAsync(leaderboardEntry);
                }

                // Update rankings for all players in this game session
                await _leaderboardRepository.UpdateRankingsAsync(gameSessionId);

                _logger.LogInformation("Updated leaderboard for player {PlayerId} in session {GameSessionId}", 
                    playerId, gameSessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating leaderboard for player {PlayerId} in session {GameSessionId}", 
                    playerId, gameSessionId);
                throw;
            }
        }

        /// <summary>
        /// Calculates points earned for an answer based on correctness and time taken
        /// </summary>
        public int CalculatePointsForAnswer(bool isCorrect, double timeTakenSeconds, int timeLimit)
        {
            if (!isCorrect)
            {
                return 0;
            }

            // Base points for correct answer
            const int basePoints = 100;

            // Time bonus: faster answers get more points (up to 50% bonus)
            var timeRatio = timeTakenSeconds / timeLimit;
            var timeBonus = (int)((1 - timeRatio) * (basePoints * 0.5));

            return basePoints + Math.Max(0, timeBonus);
        }

        /// <summary>
        /// Gets statistics for a completed game
        /// </summary>
        public async Task<GameStatistics> GetGameStatisticsAsync(int gameSessionId)
        {
            try
            {
                var leaderboards = await _leaderboardRepository.GetByGameSessionIdOrderedAsync(gameSessionId);
                var playerAnswers = await _playerAnswerRepository.GetByGameSessionIdAsync(gameSessionId);

                var stats = new GameStatistics
                {
                    TotalPlayers = leaderboards.Count(),
                    TotalQuestions = playerAnswers.Select(pa => pa.QuestionId).Distinct().Count(),
                    AverageScore = leaderboards.Any() ? (int)leaderboards.Average(l => l.Score) : 0,
                    HighestScore = leaderboards.Any() ? leaderboards.Max(l => l.Score) : 0,
                    LowestScore = leaderboards.Any() ? leaderboards.Min(l => l.Score) : 0
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving game statistics for session {GameSessionId}", gameSessionId);
                throw;
            }
        }
    }

    public class GameStatistics
    {
        public int TotalPlayers { get; set; }
        public int TotalQuestions { get; set; }
        public int AverageScore { get; set; }
        public int HighestScore { get; set; }
        public int LowestScore { get; set; }
    }
}