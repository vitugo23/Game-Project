using gameProject.Models;

namespace gameProject.Repositories.Interfaces
{
    public interface IQuestionRepository
    {
        Task<Question?> GetByIdAsync(int id);
        Task<Question?> GetByIdWithChoicesAsync(int id);
        Task<IEnumerable<Question>> GetByQuizIdAsync(int quizId);
        Task<IEnumerable<Question>> GetByQuizIdWithChoicesAsync(int quizId);
        Task<Question> CreateAsync(Question question);
        Task<Question?> UpdateAsync(Question question);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);



    }
}

