using gameProject.Models;

namespace gameProject.Repositories.Interfaces
{
    public interface IGameRecordRepository
    {
        Task<List<GameRecord>> GetAllGameRecordAsync();
        Task<GameRecord?> GetGameRecordByIdAsync(int gameRecordId);
        Task<GameRecord> CreateGameRecordAsync(GameRecord gameRecord);
        Task<bool> DeleteGameRecordAsync(int GameRecordId);
    }
}