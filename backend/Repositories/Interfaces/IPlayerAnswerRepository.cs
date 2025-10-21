using gameProject.Models;

namespace gameProject.Repositories.Interfaces
{
    public interface IPlayerAnswerRepository
    {
        Task<PlayerAnswer?> GetByIdAsync(int id);
        Task<PlayerAnswer?> GetByPlayerQuestionAndSessionAsync(int playerId, int questionId, int gameSessionId);
        Task<IEnumerable<PlayerAnswer>> GetByPlayerIdAsync(int playerId);
        Task<IEnumerable<PlayerAnswer>> GetByGameSessionIdAsync(int gameSessionId);
        Task<IEnumerable<PlayerAnswer>> GetByPlayerAndGameSessionAsync(int playerId, int gameSessionId);
        Task<PlayerAnswer> CreateAsync(PlayerAnswer playerAnswer);
        Task<PlayerAnswer?> UpdateAsync(PlayerAnswer playerAnswer);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int playerId, int questionId, int gameSessionId);
        Task<int> GetCorrectAnswerCountAsync(int playerId, int gameSessionId);
    }
}