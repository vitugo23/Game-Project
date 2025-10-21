using Microsoft.EntityFrameworkCore;
using gameProject.Data;
using gameProject.Models;
using gameProject.Repositories.Interfaces;

namespace gameProject.Repositories
{
    public class LeaderboardRepository : ILeaderboardRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LeaderboardRepository> _logger;

        public LeaderboardRepository(ApplicationDbContext context, ILogger<LeaderboardRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Leaderboard?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Leaderboards
                    .Include(l => l.Player)
                        .ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(l => l.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving leaderboard entry with ID {LeaderboardId}", id);
                throw;
            }
        }

        public async Task<Leaderboard?> GetByGameSessionAndPlayerAsync(int gameSessionId, int playerId)
        {
            try
            {
                return await _context.Leaderboards
                    .Include(l => l.Player)
                        .ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(l => l.GameSessionId == gameSessionId && l.PlayerId == playerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving leaderboard entry for game {GameSessionId} and player {PlayerId}", 
                    gameSessionId, playerId);
                throw;
            }
        }

        public async Task<IEnumerable<Leaderboard>> GetByGameSessionIdAsync(int gameSessionId)
        {
            try
            {
                return await _context.Leaderboards
                    .Where(l => l.GameSessionId == gameSessionId)
                    .Include(l => l.Player)
                        .ThenInclude(p => p.User)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving leaderboard for game session {GameSessionId}", gameSessionId);
                throw;
            }
        }

        public async Task<IEnumerable<Leaderboard>> GetByGameSessionIdOrderedAsync(int gameSessionId)
        {
            try
            {
                return await _context.Leaderboards
                    .Where(l => l.GameSessionId == gameSessionId)
                    .Include(l => l.Player)
                        .ThenInclude(p => p.User)
                    .OrderByDescending(l => l.Score)
                    .ThenBy(l => l.Player.User.Username)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ordered leaderboard for game session {GameSessionId}", gameSessionId);
                throw;
            }
        }

        public async Task<Leaderboard> CreateAsync(Leaderboard leaderboard)
        {
            try
            {
                leaderboard.UpdatedAt = DateTime.UtcNow;
                _context.Leaderboards.Add(leaderboard);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new leaderboard entry with ID {LeaderboardId}", leaderboard.Id);
                return leaderboard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating leaderboard entry");
                throw;
            }
        }

        public async Task<Leaderboard?> UpdateAsync(Leaderboard leaderboard)
        {
            try
            {
                var existingLeaderboard = await _context.Leaderboards.FindAsync(leaderboard.Id);
                if (existingLeaderboard == null)
                {
                    _logger.LogWarning("Leaderboard entry with ID {LeaderboardId} not found for update", leaderboard.Id);
                    return null;
                }

                existingLeaderboard.Score = leaderboard.Score;
                existingLeaderboard.Rank = leaderboard.Rank;
                existingLeaderboard.CorrectAnswers = leaderboard.CorrectAnswers;
                existingLeaderboard.TotalAnswers = leaderboard.TotalAnswers;
                existingLeaderboard.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated leaderboard entry with ID {LeaderboardId}", leaderboard.Id);
                return existingLeaderboard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating leaderboard entry with ID {LeaderboardId}", leaderboard.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var leaderboard = await _context.Leaderboards.FindAsync(id);
                if (leaderboard == null)
                {
                    _logger.LogWarning("Leaderboard entry with ID {LeaderboardId} not found for deletion", id);
                    return false;
                }

                _context.Leaderboards.Remove(leaderboard);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted leaderboard entry with ID {LeaderboardId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting leaderboard entry with ID {LeaderboardId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int gameSessionId, int playerId)
        {
            try
            {
                return await _context.Leaderboards
                    .AnyAsync(l => l.GameSessionId == gameSessionId && l.PlayerId == playerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of leaderboard entry for game {GameSessionId} and player {PlayerId}", 
                    gameSessionId, playerId);
                throw;
            }
        }

        public async Task UpdateRankingsAsync(int gameSessionId)
        {
            try
            {
                var leaderboards = await _context.Leaderboards
                    .Where(l => l.GameSessionId == gameSessionId)
                    .OrderByDescending(l => l.Score)
                    .ToListAsync();

                int rank = 1;
                foreach (var entry in leaderboards)
                {
                    entry.Rank = rank++;
                    entry.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated rankings for game session {GameSessionId}", gameSessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating rankings for game session {GameSessionId}", gameSessionId);
                throw;
            }
        }
    }
}