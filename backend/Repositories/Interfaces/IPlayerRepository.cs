using gameProject.Models;

namespace gameProject.Repositories.Interfaces
{
    public interface IPlayerRepository
    {
        Task<Player?> GetByIdAsync(int id);
        Task<Player?> GetByUserAndRoomAsync(int userId, int roomId);
        Task<IEnumerable<Player>> GetByRoomIdAsync(int roomId);
        Task<IEnumerable<Player>> GetByUserIdAsync(int userId);
        Task<Player> CreateAsync(Player player);
        Task<Player?> UpdateAsync(Player player);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int userId, int roomId);
        Task<int> GetPlayerCountInRoomAsync(int roomId);
    }
}