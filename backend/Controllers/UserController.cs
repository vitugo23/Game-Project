using Microsoft.AspNetCore.Mvc;
using gameProject.DTOs;
using gameProject.Extensions;
using gameProject.Models;
using gameProject.Repositories.Interfaces;

namespace gameProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserRepository userRepository, ILogger<UserController> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetAllUsers()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                var userDtos = users.Select(u => u.ToDto()).ToList();

                return Ok(new ApiResponse<List<UserDto>>
                {
                    Success = true,
                    Message = "Users retrieved successfully",
                    Data = userDtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while retrieving users",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetUserById(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Message = $"User with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<UserDto>
                {
                    Success = true,
                    Message = "User retrieved successfully",
                    Data = user.ToDto()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with ID {UserId}", id);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while retrieving the user",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("auth/{authUuid}")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetUserByAuthUuid(string authUuid)
        {
            try
            {
                var user = await _userRepository.GetByAuthUuidAsync(authUuid);
                if (user == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Message = $"User with auth UUID {authUuid} not found"
                    });
                }

                return Ok(new ApiResponse<UserDto>
                {
                    Success = true,
                    Message = "User retrieved successfully",
                    Data = user.ToDto()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with auth UUID {AuthUuid}", authUuid);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while retrieving the user",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("username/{username}")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetUserByUsername(string username)
        {
            try
            {
                var user = await _userRepository.GetByUsernameAsync(username);
                if (user == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Message = $"User with username {username} not found"
                    });
                }

                return Ok(new ApiResponse<UserDto>
                {
                    Success = true,
                    Message = "User retrieved successfully",
                    Data = user.ToDto()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with username {Username}", username);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while retrieving the user",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Invalid user data",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                // Check if auth UUID already exists
                if (await _userRepository.ExistsByAuthUuidAsync(createUserDto.AuthUuid))
                {
                    return Conflict(new ErrorResponse
                    {
                        Message = "User with this auth UUID already exists"
                    });
                }

                // Check if username already exists
                if (await _userRepository.ExistsByUsernameAsync(createUserDto.Username))
                {
                    return Conflict(new ErrorResponse
                    {
                        Message = "Username already taken"
                    });
                }

                var user = new User
                {
                    AuthUuid = createUserDto.AuthUuid,
                    Username = createUserDto.Username
                };

                var createdUser = await _userRepository.CreateAsync(user);

                return CreatedAtAction(
                    nameof(GetUserById),
                    new { id = createdUser.Id },
                    new ApiResponse<UserDto>
                    {
                        Success = true,
                        Message = "User created successfully",
                        Data = createdUser.ToDto()
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while creating the user",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                var existingUser = await _userRepository.GetByIdAsync(id);
                if (existingUser == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Message = $"User with ID {id} not found"
                    });
                }

                if (!string.IsNullOrEmpty(updateUserDto.Username))
                {
                    // Check if new username is already taken by another user
                    var userWithUsername = await _userRepository.GetByUsernameAsync(updateUserDto.Username);
                    if (userWithUsername != null && userWithUsername.Id != id)
                    {
                        return Conflict(new ErrorResponse
                        {
                            Message = "Username already taken"
                        });
                    }

                    existingUser.Username = updateUserDto.Username;
                }

                var updatedUser = await _userRepository.UpdateAsync(existingUser);

                return Ok(new ApiResponse<UserDto>
                {
                    Success = true,
                    Message = "User updated successfully",
                    Data = updatedUser!.ToDto()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID {UserId}", id);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while updating the user",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(int id)
        {
            try
            {
                var deleted = await _userRepository.DeleteAsync(id);
                if (!deleted)
                {
                    return NotFound(new ErrorResponse
                    {
                        Message = $"User with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "User deleted successfully",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID {UserId}", id);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while deleting the user",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("{id}/exists")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckUserExists(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                var exists = user != null;

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = exists ? "User exists" : "User does not exist",
                    Data = exists
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user exists with ID {UserId}", id);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while checking user existence",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("username/{username}/exists")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckUsernameExists(string username)
        {
            try
            {
                var exists = await _userRepository.ExistsByUsernameAsync(username);

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = exists ? "Username exists" : "Username available",
                    Data = exists
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if username exists: {Username}", username);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while checking username availability",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}