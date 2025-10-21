using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using gameProject.Controllers;
using gameProject.DTOs;
using gameProject.Models;
using gameProject.Repositories.Interfaces;
using gameProject.Tests.Helpers;
using Xunit;

namespace gameProject.Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserRepository> _mockRepository;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mockRepository = new Mock<IUserRepository>();
            var logger = TestDbHelper.CreateMockLogger<UserController>();
            _controller = new UserController(_mockRepository.Object, logger);
        }

        [Fact]
        public async Task GetAllUsers_ShouldReturnAllUsers_WhenUsersExist()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, AuthUuid = "uuid1", Username = "user1" },
                new User { Id = 2, AuthUuid = "uuid2", Username = "user2" }
            };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

            // Act
            var result = await _controller.GetAllUsers();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<ApiResponse<List<UserDto>>>().Subject;
            response.Success.Should().BeTrue();
            response.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetUserById_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var user = new User { Id = 1, AuthUuid = "test-uuid", Username = "testuser" };
            _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

            // Act
            var result = await _controller.GetUserById(1);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<ApiResponse<UserDto>>().Subject;
            response.Success.Should().BeTrue();
            response.Data!.Id.Should().Be(1);
            response.Data.Username.Should().Be("testuser");
        }

        [Fact]
        public async Task GetUserById_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((User?)null);

            // Act
            var result = await _controller.GetUserById(999);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task CreateUser_ShouldCreateUser_WhenValidDataProvided()
        {
            // Arrange
            var createDto = new CreateUserDto
            {
                AuthUuid = "new-uuid",
                Username = "newuser"
            };

            var createdUser = new User
            {
                Id = 1,
                AuthUuid = "new-uuid",
                Username = "newuser",
                CreatedAt = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.ExistsByAuthUuidAsync("new-uuid")).ReturnsAsync(false);
            _mockRepository.Setup(r => r.ExistsByUsernameAsync("newuser")).ReturnsAsync(false);
            _mockRepository.Setup(r => r.CreateAsync(It.IsAny<User>())).ReturnsAsync(createdUser);

            // Act
            var result = await _controller.CreateUser(createDto);

            // Assert
            var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            var response = createdResult.Value.Should().BeOfType<ApiResponse<UserDto>>().Subject;
            response.Success.Should().BeTrue();
            response.Data!.Username.Should().Be("newuser");
        }

        [Fact]
        public async Task CreateUser_ShouldReturnConflict_WhenAuthUuidAlreadyExists()
        {
            // Arrange
            var createDto = new CreateUserDto
            {
                AuthUuid = "existing-uuid",
                Username = "newuser"
            };

            _mockRepository.Setup(r => r.ExistsByAuthUuidAsync("existing-uuid")).ReturnsAsync(true);

            // Act
            var result = await _controller.CreateUser(createDto);

            // Assert
            result.Result.Should().BeOfType<ConflictObjectResult>();
        }

        [Fact]
        public async Task CreateUser_ShouldReturnConflict_WhenUsernameAlreadyExists()
        {
            // Arrange
            var createDto = new CreateUserDto
            {
                AuthUuid = "new-uuid",
                Username = "existinguser"
            };

            _mockRepository.Setup(r => r.ExistsByAuthUuidAsync("new-uuid")).ReturnsAsync(false);
            _mockRepository.Setup(r => r.ExistsByUsernameAsync("existinguser")).ReturnsAsync(true);

            // Act
            var result = await _controller.CreateUser(createDto);

            // Assert
            result.Result.Should().BeOfType<ConflictObjectResult>();
        }

        [Fact]
        public async Task UpdateUser_ShouldUpdateUser_WhenValidDataProvided()
        {
            // Arrange
            var existingUser = new User
            {
                Id = 1,
                AuthUuid = "test-uuid",
                Username = "oldusername"
            };

            var updateDto = new UpdateUserDto
            {
                Username = "newusername"
            };

            var updatedUser = new User
            {
                Id = 1,
                AuthUuid = "test-uuid",
                Username = "newusername",
                UpdatedAt = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingUser);
            _mockRepository.Setup(r => r.GetByUsernameAsync("newusername")).ReturnsAsync((User?)null);
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync(updatedUser);

            // Act
            var result = await _controller.UpdateUser(1, updateDto);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<ApiResponse<UserDto>>().Subject;
            response.Success.Should().BeTrue();
            response.Data!.Username.Should().Be("newusername");
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var updateDto = new UpdateUserDto { Username = "newusername" };
            _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((User?)null);

            // Act
            var result = await _controller.UpdateUser(999, updateDto);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task DeleteUser_ShouldDeleteUser_WhenUserExists()
        {
            // Arrange
            _mockRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteUser(1);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<ApiResponse<bool>>().Subject;
            response.Success.Should().BeTrue();
            response.Data.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            _mockRepository.Setup(r => r.DeleteAsync(999)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteUser(999);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task CheckUserExists_ShouldReturnTrue_WhenUserExists()
        {
            // Arrange
            var user = new User { Id = 1, AuthUuid = "test-uuid", Username = "testuser" };
            _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

            // Act
            var result = await _controller.CheckUserExists(1);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<ApiResponse<bool>>().Subject;
            response.Success.Should().BeTrue();
            response.Data.Should().BeTrue();
        }

        [Fact]
        public async Task CheckUserExists_ShouldReturnFalse_WhenUserDoesNotExist()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((User?)null);

            // Act
            var result = await _controller.CheckUserExists(999);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<ApiResponse<bool>>().Subject;
            response.Success.Should().BeTrue();
            response.Data.Should().BeFalse();
        }

        [Fact]
        public async Task CheckUsernameExists_ShouldReturnTrue_WhenUsernameExists()
        {
            // Arrange
            _mockRepository.Setup(r => r.ExistsByUsernameAsync("existinguser")).ReturnsAsync(true);

            // Act
            var result = await _controller.CheckUsernameExists("existinguser");

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<ApiResponse<bool>>().Subject;
            response.Success.Should().BeTrue();
            response.Data.Should().BeTrue();
        }

        [Fact]
        public async Task CheckUsernameExists_ShouldReturnFalse_WhenUsernameDoesNotExist()
        {
            // Arrange
            _mockRepository.Setup(r => r.ExistsByUsernameAsync("newuser")).ReturnsAsync(false);

            // Act
            var result = await _controller.CheckUsernameExists("newuser");

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<ApiResponse<bool>>().Subject;
            response.Success.Should().BeTrue();
            response.Data.Should().BeFalse();
        }

        [Fact]
        public async Task GetUserByAuthUuid_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var user = new User { Id = 1, AuthUuid = "auth-uuid-123", Username = "testuser" };
            _mockRepository.Setup(r => r.GetByAuthUuidAsync("auth-uuid-123")).ReturnsAsync(user);

            // Act
            var result = await _controller.GetUserByAuthUuid("auth-uuid-123");

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<ApiResponse<UserDto>>().Subject;
            response.Success.Should().BeTrue();
            response.Data!.AuthUuid.Should().Be("auth-uuid-123");
        }

        [Fact]
        public async Task GetUserByUsername_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var user = new User { Id = 1, AuthUuid = "test-uuid", Username = "testuser" };
            _mockRepository.Setup(r => r.GetByUsernameAsync("testuser")).ReturnsAsync(user);

            // Act
            var result = await _controller.GetUserByUsername("testuser");

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<ApiResponse<UserDto>>().Subject;
            response.Success.Should().BeTrue();
            response.Data!.Username.Should().Be("testuser");
        }
    }
}