using gameProject.Models;

namespace gameProject.Repositories.Interfaces
{
    public interface IGameSessionRepository
    {
        Task<GameSession?> GetByIdAsync(int id);
        Task<GameSession?> GetByIdWithDetailsAsync(int id);
        Task<GameSession?> GetByRoomIdAsync(int roomId);
        Task<GameSession?> GetActiveByRoomIdAsync(int roomId);
        Task<IEnumerable<GameSession>> GetAllAsync();
        Task<IEnumerable<GameSession>> GetByStatusAsync(GameStatus status);
        Task<GameSession> CreateAsync(GameSession gameSession);
        Task<GameSession?> UpdateAsync(GameSession gameSession);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}