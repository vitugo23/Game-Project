using Microsoft.EntityFrameworkCore;
using gameProject.Data;
using gameProject.Models;
using gameProject.Repositories.Interfaces;

namespace gameProject.Repositories
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<QuestionRepository> _logger;

        public QuestionRepository(ApplicationDbContext context, ILogger<QuestionRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Question?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Questions.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving question with ID {QuestionId}", id);
                throw;
            }
        }

        public async Task<Question?> GetByIdWithChoicesAsync(int id)
        {
            try
            {
                return await _context.Questions
                    .Include(q => q.Choices)
                    .FirstOrDefaultAsync(q => q.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving question with choices for ID {QuestionId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Question>> GetByQuizIdAsync(int quizId)
        {
            try
            {
                return await _context.Questions
                    .Where(q => q.QuizId == quizId)
                    .OrderBy(q => q.QuestionOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving questions for quiz {QuizId}", quizId);
                throw;
            }
        }

        public async Task<IEnumerable<Question>> GetByQuizIdWithChoicesAsync(int quizId)
        {
            try
            {
                return await _context.Questions
                    .Where(q => q.QuizId == quizId)
                    .Include(q => q.Choices.OrderBy(c => c.ChoiceOrder))
                    .OrderBy(q => q.QuestionOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving questions with choices for quiz {QuizId}", quizId);
                throw;
            }
        }

        public async Task<Question> CreateAsync(Question question)
        {
            try
            {
                question.CreatedAt = DateTime.UtcNow;
                _context.Questions.Add(question);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new question with ID {QuestionId}", question.Id);
                return question;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating question");
                throw;
            }
        }

        public async Task<Question?> UpdateAsync(Question question)
        {
            try
            {
                var existingQuestion = await _context.Questions.FindAsync(question.Id);
                if (existingQuestion == null)
                {
                    _logger.LogWarning("Question with ID {QuestionId} not found for update", question.Id);
                    return null;
                }

                existingQuestion.QuestionText = question.QuestionText;
                existingQuestion.QuestionOrder = question.QuestionOrder;
                existingQuestion.TimeLimit = question.TimeLimit;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated question with ID {QuestionId}", question.Id);
                return existingQuestion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating question with ID {QuestionId}", question.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var question = await _context.Questions.FindAsync(id);
                if (question == null)
                {
                    _logger.LogWarning("Question with ID {QuestionId} not found for deletion", id);
                    return false;
                }

                _context.Questions.Remove(question);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted question with ID {QuestionId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting question with ID {QuestionId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _context.Questions.AnyAsync(q => q.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of question {QuestionId}", id);
                throw;
            }
        }
    }
}