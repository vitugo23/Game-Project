using Microsoft.EntityFrameworkCore;
using gameProject.Data;
using gameProject.Models;
using gameProject.Repositories.Interfaces;

namespace gameProject.Repositories
{
    public class GameSessionRepository : IGameSessionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GameSessionRepository> _logger;

        public GameSessionRepository(ApplicationDbContext context, ILogger<GameSessionRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<GameSession?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.GameSessions
                    .Include(gs => gs.Room)
                    .Include(gs => gs.Quiz)
                    .FirstOrDefaultAsync(gs => gs.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving game session with ID {GameSessionId}", id);
                throw;
            }
        }

        public async Task<GameSession?> GetByIdWithDetailsAsync(int id)
        {
            try
            {
                return await _context.GameSessions
                    .Include(gs => gs.Room)
                        .ThenInclude(r => r.Players)
                            .ThenInclude(p => p.User)
                    .Include(gs => gs.Quiz)
                        .ThenInclude(q => q.Questions)
                            .ThenInclude(q => q.Choices)
                    .Include(gs => gs.Leaderboards)
                    .FirstOrDefaultAsync(gs => gs.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving game session with details for ID {GameSessionId}", id);
                throw;
            }
        }

        public async Task<GameSession?> GetByRoomIdAsync(int roomId)
        {
            try
            {
                return await _context.GameSessions
                    .Include(gs => gs.Room)
                    .Include(gs => gs.Quiz)
                    .Where(gs => gs.RoomId == roomId)
                    .OrderByDescending(gs => gs.CreatedAt)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving game session for room {RoomId}", roomId);
                throw;
            }
        }

        public async Task<GameSession?> GetActiveByRoomIdAsync(int roomId)
        {
            try
            {
                return await _context.GameSessions
                    .Include(gs => gs.Room)
                    .Include(gs => gs.Quiz)
                    .FirstOrDefaultAsync(gs => gs.RoomId == roomId && gs.Status == GameStatus.Active);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active game session for room {RoomId}", roomId);
                throw;
            }
        }

        public async Task<IEnumerable<GameSession>> GetAllAsync()
        {
            try
            {
                return await _context.GameSessions
                    .Include(gs => gs.Room)
                    .Include(gs => gs.Quiz)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all game sessions");
                throw;
            }
        }

        public async Task<IEnumerable<GameSession>> GetByStatusAsync(GameStatus status)
        {
            try
            {
                return await _context.GameSessions
                    .Where(gs => gs.Status == status)
                    .Include(gs => gs.Room)
                    .Include(gs => gs.Quiz)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving game sessions with status {Status}", status);
                throw;
            }
        }

        public async Task<GameSession> CreateAsync(GameSession gameSession)
        {
            try
            {
                gameSession.CreatedAt = DateTime.UtcNow;
                _context.GameSessions.Add(gameSession);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new game session with ID {GameSessionId}", gameSession.Id);
                return gameSession;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating game session");
                throw;
            }
        }

        public async Task<GameSession?> UpdateAsync(GameSession gameSession)
        {
            try
            {
                var existingSession = await _context.GameSessions.FindAsync(gameSession.Id);
                if (existingSession == null)
                {
                    _logger.LogWarning("Game session with ID {GameSessionId} not found for update", gameSession.Id);
                    return null;
                }

                existingSession.Status = gameSession.Status;
                existingSession.CurrentQuestionNumber = gameSession.CurrentQuestionNumber;
                existingSession.CurrentQuestionEndTime = gameSession.CurrentQuestionEndTime;
                existingSession.StartedAt = gameSession.StartedAt;
                existingSession.EndedAt = gameSession.EndedAt;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated game session with ID {GameSessionId}", gameSession.Id);
                return existingSession;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating game session with ID {GameSessionId}", gameSession.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var gameSession = await _context.GameSessions.FindAsync(id);
                if (gameSession == null)
                {
                    _logger.LogWarning("Game session with ID {GameSessionId} not found for deletion", id);
                    return false;
                }

                _context.GameSessions.Remove(gameSession);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted game session with ID {GameSessionId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting game session with ID {GameSessionId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _context.GameSessions.AnyAsync(gs => gs.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of game session {GameSessionId}", id);
                throw;
            }
        }
    }
}