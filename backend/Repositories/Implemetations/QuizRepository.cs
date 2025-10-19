using Microsoft.EntityFrameworkCore;
using gameProject.Data;
using gameProject.Models;
using gameProject.Repositories.Interfaces;

namespace gameProject.Repositories.Implementations
{
    public class QuizRepository : IQuizRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<QuizRepository> _logger;

        public QuizRepository(ApplicationDbContext context, ILogger<QuizRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Quiz?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Quizzes
                    .Include(q => q.Creator)
                    .FirstOrDefaultAsync(q => q.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving quiz with ID {QuizId}", id);
                throw;
            }
        }

        public async Task<Quiz?> GetByIdWithQuestionsAsync(int id)
        {
            try
            {
                return await _context.Quizzes
                    .Include(q => q.Creator)
                    .Include(q => q.Questions)
                        .ThenInclude(q => q.Choices)
                    .FirstOrDefaultAsync(q => q.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving quiz with questions for ID {QuizId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Quiz>> GetAllAsync()
        {
            try
            {
                return await _context.Quizzes
                    .Include(q => q.Creator)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all quizzes");
                throw;
            }
        }

        public async Task<IEnumerable<Quiz>> GetByCreatorIdAsync(int creatorId)
        {
            try
            {
                return await _context.Quizzes
                    .Where(q => q.CreatorId == creatorId)
                    .Include(q => q.Creator)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving quizzes for creator {CreatorId}", creatorId);
                throw;
            }
        }

        public async Task<Quiz> CreateAsync(Quiz quiz)
        {
            try
            {
                quiz.CreatedAt = DateTime.UtcNow;
                _context.Quizzes.Add(quiz);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new quiz with ID {QuizId}", quiz.Id);
                return quiz;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating quiz");
                throw;
            }
        }

        public async Task<Quiz?> UpdateAsync(Quiz quiz)
        {
            try
            {
                var existingQuiz = await _context.Quizzes.FindAsync(quiz.Id);
                if (existingQuiz == null)
                {
                    _logger.LogWarning("Quiz with ID {QuizId} not found for update", quiz.Id);
                    return null;
                }

                existingQuiz.QuizName = quiz.QuizName;
                existingQuiz.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated quiz with ID {QuizId}", quiz.Id);
                return existingQuiz;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating quiz with ID {QuizId}", quiz.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var quiz = await _context.Quizzes.FindAsync(id);
                if (quiz == null)
                {
                    _logger.LogWarning("Quiz with ID {QuizId} not found for deletion", id);
                    return false;
                }

                _context.Quizzes.Remove(quiz);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted quiz with ID {QuizId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting quiz with ID {QuizId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _context.Quizzes.AnyAsync(q => q.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of quiz {QuizId}", id);
                throw;
            }
        }
    }
}