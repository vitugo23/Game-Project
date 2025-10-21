using Microsoft.AspNetCore.Mvc;
using gameProject.DTOs;
using gameProject.Extensions;
using gameProject.Models;
using gameProject.Repositories.Interfaces;

namespace gameProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly ILogger<RoomController> _logger;

        public RoomController(
            IRoomRepository roomRepository,
            IPlayerRepository playerRepository,
            ILogger<RoomController> logger)
        {
            _roomRepository = roomRepository;
            _playerRepository = playerRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<RoomDto>>>> GetActiveRooms()
        {
            try
            {
                var rooms = await _roomRepository.GetAllActiveAsync();
                var roomDtos = rooms.Select(r => r.ToDto()).ToList();

                return Ok(new ApiResponse<List<RoomDto>>
                {
                    Success = true,
                    Message = "Active rooms retrieved successfully",
                    Data = roomDtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active rooms");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while retrieving rooms",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<RoomDetailDto>>> GetRoomById(int id)
        {
            try
            {
                var room = await _roomRepository.GetByIdWithPlayersAsync(id);
                if (room == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Message = $"Room with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<RoomDetailDto>
                {
                    Success = true,
                    Message = "Room retrieved successfully",
                    Data = room.ToDetailDto()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room with ID {RoomId}", id);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while retrieving the room",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("code/{roomCode}")]
        public async Task<ActionResult<ApiResponse<RoomDetailDto>>> GetRoomByCode(string roomCode)
        {
            try
            {
                var room = await _roomRepository.GetByRoomCodeAsync(roomCode);
                if (room == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Message = $"Room with code {roomCode} not found"
                    });
                }

                return Ok(new ApiResponse<RoomDetailDto>
                {
                    Success = true,
                    Message = "Room retrieved successfully",
                    Data = room.ToDetailDto()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room with code {RoomCode}", roomCode);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while retrieving the room",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<RoomDto>>> CreateRoom([FromBody] CreateRoomDto createRoomDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Invalid room data",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                // Generate unique room code
                var roomCode = await _roomRepository.GenerateUniqueRoomCodeAsync();

                var room = new Room
                {
                    RoomCode = roomCode,
                    HostUserId = createRoomDto.HostUserId,
                    MaxPlayers = createRoomDto.MaxPlayers,
                    IsActive = true
                };

                var createdRoom = await _roomRepository.CreateAsync(room);

                // Automatically add host as a player
                var hostPlayer = new Player
                {
                    UserId = createRoomDto.HostUserId,
                    RoomId = createdRoom.Id,
                    IsReady = false,
                    IsConnected = true
                };
                await _playerRepository.CreateAsync(hostPlayer);

                // Reload room with players
                createdRoom = await _roomRepository.GetByIdWithPlayersAsync(createdRoom.Id);

                return CreatedAtAction(
                    nameof(GetRoomById),
                    new { id = createdRoom!.Id },
                    new ApiResponse<RoomDto>
                    {
                        Success = true,
                        Message = "Room created successfully",
                        Data = createdRoom.ToDto()
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating room");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while creating the room",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPost("join")]
        public async Task<ActionResult<ApiResponse<PlayerDto>>> JoinRoom([FromBody] JoinRoomDto joinRoomDto)
        {
            try
            {
                var room = await _roomRepository.GetByRoomCodeAsync(joinRoomDto.RoomCode);
                if (room == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Message = $"Room with code {joinRoomDto.RoomCode} not found"
                    });
                }

                if (!room.IsActive)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "This room is no longer active"
                    });
                }

                // Check if room is full
                var currentPlayerCount = await _playerRepository.GetPlayerCountInRoomAsync(room.Id);
                if (currentPlayerCount >= room.MaxPlayers)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "This room is full"
                    });
                }

                // Check if player already in room
                var existingPlayer = await _playerRepository.GetByUserAndRoomAsync(joinRoomDto.UserId, room.Id);
                if (existingPlayer != null)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "You are already in this room"
                    });
                }

                var player = new Player
                {
                    UserId = joinRoomDto.UserId,
                    RoomId = room.Id,
                    IsReady = false,
                    IsConnected = true
                };

                var createdPlayer = await _playerRepository.CreateAsync(player);

                return Ok(new ApiResponse<PlayerDto>
                {
                    Success = true,
                    Message = "Joined room successfully",
                    Data = createdPlayer.ToDto()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining room");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while joining the room",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<RoomDto>>> UpdateRoom(int id, [FromBody] UpdateRoomDto updateRoomDto)
        {
            try
            {
                var existingRoom = await _roomRepository.GetByIdAsync(id);
                if (existingRoom == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Message = $"Room with ID {id} not found"
                    });
                }

                if (updateRoomDto.MaxPlayers.HasValue)
                {
                    existingRoom.MaxPlayers = updateRoomDto.MaxPlayers.Value;
                }

                if (updateRoomDto.IsActive.HasValue)
                {
                    existingRoom.IsActive = updateRoomDto.IsActive.Value;
                }

                var updatedRoom = await _roomRepository.UpdateAsync(existingRoom);

                return Ok(new ApiResponse<RoomDto>
                {
                    Success = true,
                    Message = "Room updated successfully",
                    Data = updatedRoom!.ToDto()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating room with ID {RoomId}", id);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while updating the room",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteRoom(int id)
        {
            try
            {
                var deleted = await _roomRepository.DeleteAsync(id);
                if (!deleted)
                {
                    return NotFound(new ErrorResponse
                    {
                        Message = $"Room with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Room deleted successfully",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting room with ID {RoomId}", id);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while deleting the room",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}