using Microsoft.EntityFrameworkCore;
using gameProject.Data;
using gameProject.Models;
using gameProject.Repositories.Interfaces;

namespace gameProject.Repositories
{
    public class PlayerAnswerRepository : IPlayerAnswerRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PlayerAnswerRepository> _logger;

        public PlayerAnswerRepository(ApplicationDbContext context, ILogger<PlayerAnswerRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PlayerAnswer?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.PlayerAnswers
                    .Include(pa => pa.Player)
                        .ThenInclude(p => p.User)
                    .Include(pa => pa.Question)
                    .Include(pa => pa.Choice)
                    .FirstOrDefaultAsync(pa => pa.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving player answer with ID {PlayerAnswerId}", id);
                throw;
            }
        }

        public async Task<PlayerAnswer?> GetByPlayerQuestionAndSessionAsync(int playerId, int questionId, int gameSessionId)
        {
            try
            {
                return await _context.PlayerAnswers
                    .Include(pa => pa.Player)
                    .Include(pa => pa.Question)
                    .Include(pa => pa.Choice)
                    .FirstOrDefaultAsync(pa => pa.PlayerId == playerId && 
                                              pa.QuestionId == questionId && 
                                              pa.GameSessionId == gameSessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving player answer for player {PlayerId}, question {QuestionId}, session {GameSessionId}", 
                    playerId, questionId, gameSessionId);
                throw;
            }
        }

        public async Task<IEnumerable<PlayerAnswer>> GetByPlayerIdAsync(int playerId)
        {
            try
            {
                return await _context.PlayerAnswers
                    .Where(pa => pa.PlayerId == playerId)
                    .Include(pa => pa.Question)
                    .Include(pa => pa.Choice)
                    .OrderBy(pa => pa.AnswerTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving player answers for player {PlayerId}", playerId);
                throw;
            }
        }

        public async Task<IEnumerable<PlayerAnswer>> GetByGameSessionIdAsync(int gameSessionId)
        {
            try
            {
                return await _context.PlayerAnswers
                    .Where(pa => pa.GameSessionId == gameSessionId)
                    .Include(pa => pa.Player)
                        .ThenInclude(p => p.User)
                    .Include(pa => pa.Question)
                    .Include(pa => pa.Choice)
                    .OrderBy(pa => pa.AnswerTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving player answers for game session {GameSessionId}", gameSessionId);
                throw;
            }
        }

        public async Task<IEnumerable<PlayerAnswer>> GetByPlayerAndGameSessionAsync(int playerId, int gameSessionId)
        {
            try
            {
                return await _context.PlayerAnswers
                    .Where(pa => pa.PlayerId == playerId && pa.GameSessionId == gameSessionId)
                    .Include(pa => pa.Question)
                    .Include(pa => pa.Choice)
                    .OrderBy(pa => pa.AnswerTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving player answers for player {PlayerId} in session {GameSessionId}", 
                    playerId, gameSessionId);
                throw;
            }
        }

        public async Task<PlayerAnswer> CreateAsync(PlayerAnswer playerAnswer)
        {
            try
            {
                playerAnswer.AnswerTime = DateTime.UtcNow;
                _context.PlayerAnswers.Add(playerAnswer);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new player answer with ID {PlayerAnswerId}", playerAnswer.Id);
                return playerAnswer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating player answer");
                throw;
            }
        }

        public async Task<PlayerAnswer?> UpdateAsync(PlayerAnswer playerAnswer)
        {
            try
            {
                var existingAnswer = await _context.PlayerAnswers.FindAsync(playerAnswer.Id);
                if (existingAnswer == null)
                {
                    _logger.LogWarning("Player answer with ID {PlayerAnswerId} not found for update", playerAnswer.Id);
                    return null;
                }

                existingAnswer.ChoiceId = playerAnswer.ChoiceId;
                existingAnswer.TimeTakenSeconds = playerAnswer.TimeTakenSeconds;
                existingAnswer.IsCorrect = playerAnswer.IsCorrect;
                existingAnswer.PointsEarned = playerAnswer.PointsEarned;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated player answer with ID {PlayerAnswerId}", playerAnswer.Id);
                return existingAnswer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating player answer with ID {PlayerAnswerId}", playerAnswer.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var playerAnswer = await _context.PlayerAnswers.FindAsync(id);
                if (playerAnswer == null)
                {
                    _logger.LogWarning("Player answer with ID {PlayerAnswerId} not found for deletion", id);
                    return false;
                }

                _context.PlayerAnswers.Remove(playerAnswer);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted player answer with ID {PlayerAnswerId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting player answer with ID {PlayerAnswerId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int playerId, int questionId, int gameSessionId)
        {
            try
            {
                return await _context.PlayerAnswers
                    .AnyAsync(pa => pa.PlayerId == playerId && 
                                   pa.QuestionId == questionId && 
                                   pa.GameSessionId == gameSessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of player answer for player {PlayerId}, question {QuestionId}, session {GameSessionId}", 
                    playerId, questionId, gameSessionId);
                throw;
            }
        }

        public async Task<int> GetCorrectAnswerCountAsync(int playerId, int gameSessionId)
        {
            try
            {
                return await _context.PlayerAnswers
                    .Where(pa => pa.PlayerId == playerId && 
                                pa.GameSessionId == gameSessionId && 
                                pa.IsCorrect)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting correct answers for player {PlayerId} in session {GameSessionId}", 
                    playerId, gameSessionId);
                throw;
            }
        }
    }
}