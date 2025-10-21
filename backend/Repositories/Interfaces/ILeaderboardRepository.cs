using gameProject.Models;

namespace gameProject.Repositories.Interfaces
{
    public interface ILeaderboardRepository
    {
        Task<Leaderboard?> GetByIdAsync(int id);
        Task<Leaderboard?> GetByGameSessionAndPlayerAsync(int gameSessionId, int playerId);
        Task<IEnumerable<Leaderboard>> GetByGameSessionIdAsync(int gameSessionId);
        Task<IEnumerable<Leaderboard>> GetByGameSessionIdOrderedAsync(int gameSessionId);
        Task<Leaderboard> CreateAsync(Leaderboard leaderboard);
        Task<Leaderboard?> UpdateAsync(Leaderboard leaderboard);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int gameSessionId, int playerId);
        Task UpdateRankingsAsync(int gameSessionId);
    }
}