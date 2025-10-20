using gameProject.Models;

namespace gameProject.Repositories.Interfaces
{
    public interface IRoomRepository
    {
        Task<Room?> GetByIdAsync(int id);
        Task<Room?> GetByIdWithPlayersAsync(int id);
        Task<Room?> GetByRoomCodeAsync(string roomCode);
        Task<IEnumerable<Room>> GetAllActiveAsync();
        Task<IEnumerable<Room>> GetByHostUserIdAsync(int hostUserId);
        Task<Room> CreateAsync(Room room);
        Task<Room> UpdateAsync(Room room);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsByRoomCodeAsync(string roomCode);
        Task<string> GenerateUniqueRoomCodeAsync();



    }
} 