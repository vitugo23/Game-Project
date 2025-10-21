using FluentAssertions;
using gameProject.Models;
using gameProject.Repositories;
using gameProject.Tests.Helpers;
using Xunit;

namespace gameProject.Tests.Repositories
{
    public class UserRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UserRepository _repository;

        public UserRepositoryTests()
        {
            _context = TestDbHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());
            var logger = TestDbHelper.CreateMockLogger<UserRepository>();
            _repository = new UserRepository(_context, logger);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateUser_WhenValidDataProvided()
        {
            // Arrange
            var user = new User
            {
                AuthUuid = "test-uuid-123",
                Username = "testuser"
            };

            // Act
            var result = await _repository.CreateAsync(user);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.AuthUuid.Should().Be("test-uuid-123");
            result.Username.Should().Be("testuser");
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var user = new User
            {
                AuthUuid = "test-uuid-456",
                Username = "existinguser"
            };
            var createdUser = await _repository.CreateAsync(user);

            // Act
            var result = await _repository.GetByIdAsync(createdUser.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(createdUser.Id);
            result.Username.Should().Be("existinguser");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Act
            var result = await _repository.GetByIdAsync(9999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByAuthUuidAsync_ShouldReturnUser_WhenExists()
        {
            // Arrange
            var user = new User
            {
                AuthUuid = "unique-auth-uuid",
                Username = "authuser"
            };
            await _repository.CreateAsync(user);

            // Act
            var result = await _repository.GetByAuthUuidAsync("unique-auth-uuid");

            // Assert
            result.Should().NotBeNull();
            result!.AuthUuid.Should().Be("unique-auth-uuid");
        }

        [Fact]
        public async Task GetByUsernameAsync_ShouldReturnUser_WhenExists()
        {
            // Arrange
            var user = new User
            {
                AuthUuid = "test-uuid-789",
                Username = "uniqueuser"
            };
            await _repository.CreateAsync(user);

            // Act
            var result = await _repository.GetByUsernameAsync("uniqueuser");

            // Assert
            result.Should().NotBeNull();
            result!.Username.Should().Be("uniqueuser");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllUsers()
        {
            // Arrange
            await _repository.CreateAsync(new User { AuthUuid = "uuid1", Username = "user1" });
            await _repository.CreateAsync(new User { AuthUuid = "uuid2", Username = "user2" });
            await _repository.CreateAsync(new User { AuthUuid = "uuid3", Username = "user3" });

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateUser_WhenUserExists()
        {
            // Arrange
            var user = new User
            {
                AuthUuid = "update-uuid",
                Username = "oldusername"
            };
            var createdUser = await _repository.CreateAsync(user);

            // Act
            createdUser.Username = "newusername";
            var result = await _repository.UpdateAsync(createdUser);

            // Assert
            result.Should().NotBeNull();
            result!.Username.Should().Be("newusername");
            result.UpdatedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var user = new User
            {
                Id = 9999,
                AuthUuid = "nonexistent",
                Username = "nouser"
            };

            // Act
            var result = await _repository.UpdateAsync(user);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteUser_WhenUserExists()
        {
            // Arrange
            var user = new User
            {
                AuthUuid = "delete-uuid",
                Username = "deleteuser"
            };
            var createdUser = await _repository.CreateAsync(user);

            // Act
            var result = await _repository.DeleteAsync(createdUser.Id);

            // Assert
            result.Should().BeTrue();
            var deletedUser = await _repository.GetByIdAsync(createdUser.Id);
            deletedUser.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenUserDoesNotExist()
        {
            // Act
            var result = await _repository.DeleteAsync(9999);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsByAuthUuidAsync_ShouldReturnTrue_WhenUserExists()
        {
            // Arrange
            var user = new User
            {
                AuthUuid = "exists-uuid",
                Username = "existuser"
            };
            await _repository.CreateAsync(user);

            // Act
            var result = await _repository.ExistsByAuthUuidAsync("exists-uuid");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByAuthUuidAsync_ShouldReturnFalse_WhenUserDoesNotExist()
        {
            // Act
            var result = await _repository.ExistsByAuthUuidAsync("nonexistent-uuid");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsByUsernameAsync_ShouldReturnTrue_WhenUserExists()
        {
            // Arrange
            var user = new User
            {
                AuthUuid = "username-check-uuid",
                Username = "checkusername"
            };
            await _repository.CreateAsync(user);

            // Act
            var result = await _repository.ExistsByUsernameAsync("checkusername");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByUsernameAsync_ShouldReturnFalse_WhenUserDoesNotExist()
        {
            // Act
            var result = await _repository.ExistsByUsernameAsync("nonexistentusername");

            // Assert
            result.Should().BeFalse();
        }
    }
}