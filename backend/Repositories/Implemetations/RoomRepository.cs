using Microsoft.EntityFrameworkCore;
using gameProject.Data;
using gameProject.Models;
using gameProject.Repositories.Interfaces;

namespace gameProject.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RoomRepository> _logger;

        public RoomRepository(ApplicationDbContext context, ILogger<RoomRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Room?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Rooms
                    .Include(r => r.Host)
                    .FirstOrDefaultAsync(r => r.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room with ID {RoomId}", id);
                throw;
            }
        }

        public async Task<Room?> GetByIdWithPlayersAsync(int id)
        {
            try
            {
                return await _context.Rooms
                    .Include(r => r.Host)
                    .Include(r => r.Players)
                        .ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(r => r.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room with players for ID {RoomId}", id);
                throw;
            }
        }

        public async Task<Room?> GetByRoomCodeAsync(string roomCode)
        {
            try
            {
                return await _context.Rooms
                    .Include(r => r.Host)
                    .Include(r => r.Players)
                        .ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(r => r.RoomCode == roomCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room with code {RoomCode}", roomCode);
                throw;
            }
        }

        public async Task<IEnumerable<Room>> GetAllActiveAsync()
        {
            try
            {
                return await _context.Rooms
                    .Where(r => r.IsActive)
                    .Include(r => r.Host)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active rooms");
                throw;
            }
        }

        public async Task<IEnumerable<Room>> GetByHostUserIdAsync(int hostUserId)
        {
            try
            {
                return await _context.Rooms
                    .Where(r => r.HostUserId == hostUserId)
                    .Include(r => r.Host)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rooms for host {HostUserId}", hostUserId);
                throw;
            }
        }

        public async Task<Room> CreateAsync(Room room)
        {
            try
            {
                room.CreatedAt = DateTime.UtcNow;
                _context.Rooms.Add(room);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new room with ID {RoomId} and code {RoomCode}", room.Id, room.RoomCode);
                return room;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating room");
                throw;
            }
        }

        public async Task<Room?> UpdateAsync(Room room)
        {
            try
            {
                var existingRoom = await _context.Rooms.FindAsync(room.Id);
                if (existingRoom == null)
                {
                    _logger.LogWarning("Room with ID {RoomId} not found for update", room.Id);
                    return null;
                }

                existingRoom.MaxPlayers = room.MaxPlayers;
                existingRoom.IsActive = room.IsActive;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated room with ID {RoomId}", room.Id);
                return existingRoom;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating room with ID {RoomId}", room.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var room = await _context.Rooms.FindAsync(id);
                if (room == null)
                {
                    _logger.LogWarning("Room with ID {RoomId} not found for deletion", id);
                    return false;
                }

                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted room with ID {RoomId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting room with ID {RoomId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsByRoomCodeAsync(string roomCode)
        {
            try
            {
                return await _context.Rooms.AnyAsync(r => r.RoomCode == roomCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of room code {RoomCode}", roomCode);
                throw;
            }
        }

        public async Task<string> GenerateUniqueRoomCodeAsync()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            string roomCode;

            do
            {
                roomCode = new string(Enumerable.Repeat(chars, 6)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            }
            while (await ExistsByRoomCodeAsync(roomCode));

            return roomCode;
        }
    }
}