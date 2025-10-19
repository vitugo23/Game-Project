using gameProject.Models;

namespace gameProject.Repositories.Interfaces
{
    public interface IQuizRepository
    {
        Task<Quiz?> GetByIdAsync(int id);
        Task<Quiz?> GetByIdWithQuestionsAsync(int id);
        Task<IEnumerable<Quiz>> GetAllByAsync();
        Task<IAsyncEnumerable<Quiz>> GetByCreatorIdAsync(int creatorId);
        Task<Quiz> CreateAsync(Quiz quiz);
        Task<Quiz?> UpdateAsync(Quiz quiz);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);


    }
}