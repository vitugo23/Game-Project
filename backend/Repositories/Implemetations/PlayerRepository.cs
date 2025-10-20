using Microsoft.EntityFrameworkCore;
using gameProject.Data;
using gameProject.Models;
using gameProject.Repositories.Interfaces;

namespace gameProject.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PlayerRepository> _logger;

        public PlayerRepository(ApplicationDbContext context, ILogger<PlayerRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Player?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Players
                    .Include(p => p.User)
                    .Include(p => p.Room)
                    .FirstOrDefaultAsync(p => p.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving player with ID {PlayerId}", id);
                throw;
            }
        }

        public async Task<Player?> GetByUserAndRoomAsync(int userId, int roomId)
        {
            try
            {
                return await _context.Players
                    .Include(p => p.User)
                    .Include(p => p.Room)
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.RoomId == roomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving player for user {UserId} in room {RoomId}", userId, roomId);
                throw;
            }
        }

        public async Task<IEnumerable<Player>> GetByRoomIdAsync(int roomId)
        {
            try
            {
                return await _context.Players
                    .Where(p => p.RoomId == roomId)
                    .Include(p => p.User)
                    .OrderBy(p => p.JoinedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving players for room {RoomId}", roomId);
                throw;
            }
        }

        public async Task<IEnumerable<Player>> GetByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Players
                    .Where(p => p.UserId == userId)
                    .Include(p => p.Room)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving players for user {UserId}", userId);
                throw;
            }
        }

        public async Task<Player> CreateAsync(Player player)
        {
            try
            {
                player.JoinedAt = DateTime.UtcNow;
                _context.Players.Add(player);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new player with ID {PlayerId}", player.Id);
                return player;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating player");
                throw;
            }
        }

        public async Task<Player?> UpdateAsync(Player player)
        {
            try
            {
                var existingPlayer = await _context.Players.FindAsync(player.Id);
                if (existingPlayer == null)
                {
                    _logger.LogWarning("Player with ID {PlayerId} not found for update", player.Id);
                    return null;
                }

                existingPlayer.IsReady = player.IsReady;
                existingPlayer.IsConnected = player.IsConnected;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated player with ID {PlayerId}", player.Id);
                return existingPlayer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating player with ID {PlayerId}", player.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var player = await _context.Players.FindAsync(id);
                if (player == null)
                {
                    _logger.LogWarning("Player with ID {PlayerId} not found for deletion", id);
                    return false;
                }

                _context.Players.Remove(player);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted player with ID {PlayerId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting player with ID {PlayerId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int userId, int roomId)
        {
            try
            {
                return await _context.Players.AnyAsync(p => p.UserId == userId && p.RoomId == roomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of player for user {UserId} in room {RoomId}", userId, roomId);
                throw;
            }
        }

        public async Task<int> GetPlayerCountInRoomAsync(int roomId)
        {
            try
            {
                return await _context.Players.CountAsync(p => p.RoomId == roomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting players in room {RoomId}", roomId);
                throw;
            }
        }
    }
}