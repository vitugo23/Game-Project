using gameProject.Models;

namespace gameProject.Repositories.Interfaces
{
    public interface IGameRecordRepository
    {
        Task<List<GameRecord>> GetAllGameRecordsAsync();
        Task<GameRecord?> GetGameRecordByIdAsync(int gameRecordId);
        Task<GameRecord> CreateGameRecordAsync(GameRecord gameRecord);
        Task<bool> DeleteGameRecordsAsync(int GameRecordId);
    }
}