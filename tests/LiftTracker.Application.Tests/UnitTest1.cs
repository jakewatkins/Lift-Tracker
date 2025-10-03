using AutoMapper;
using LiftTracker.Application.Services;
using LiftTracker.Domain.Entities;
using LiftTracker.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace LiftTracker.Application.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<UserService>>();
        _userService = new UserService(_mockUserRepository.Object, _mockMapper.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ShouldReturnUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedUser = new User("test@example.com", "Test User") { Id = userId };
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(expectedUser);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("Test User", result.Name);
        _mockUserRepository.Verify(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        Assert.Null(result);
        _mockUserRepository.Verify(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithValidEmail_ShouldReturnUser()
    {
        // Arrange
        var email = "test@example.com";
        var expectedUser = new User(email, "Test User");
        _mockUserRepository.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(expectedUser);

        // Act
        var result = await _userService.GetUserByEmailAsync(email);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(email, result.Email);
        Assert.Equal("Test User", result.Name);
        _mockUserRepository.Verify(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithInvalidEmail_ShouldReturnNull()
    {
        // Arrange
        var email = "nonexistent@example.com";
        _mockUserRepository.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
                          .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetUserByEmailAsync(email);

        // Assert
        Assert.Null(result);
        _mockUserRepository.Verify(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrUpdateUserAsync_WithNewUser_ShouldReturnCreatedUser()
    {
        // Arrange
        var email = "test@example.com";
        var name = "Test User";
        var expectedUser = new User(email, name) { Id = Guid.NewGuid() };

        _mockUserRepository.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
                          .ReturnsAsync((User?)null);
        _mockUserRepository.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(expectedUser);

        // Act
        var result = await _userService.CreateOrUpdateUserAsync(email, name);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(email, result.Email);
        Assert.Equal(name, result.Name);
        _mockUserRepository.Verify(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserRepository.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrUpdateUserAsync_WithExistingUser_ShouldReturnUpdatedUser()
    {
        // Arrange
        var email = "test@example.com";
        var name = "Updated Test User";
        var existingUser = new User(email, "Old Name") { Id = Guid.NewGuid() };

        _mockUserRepository.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(existingUser);
        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(existingUser);

        // Act
        var result = await _userService.CreateOrUpdateUserAsync(email, name);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(email, result.Email);
        Assert.Equal(name, result.Name);
        _mockUserRepository.Verify(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(null, "Test User")]
    [InlineData("test@example.com", null)]
    public async Task CreateOrUpdateUserAsync_WithNullParameters_ShouldThrowArgumentNullException(string? email, string? name)
    {
        // Arrange - Set up mock to return null for GetByEmailAsync since it will be called
        _mockUserRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _userService.CreateOrUpdateUserAsync(email!, name!));

        // The service calls GetByEmailAsync before trying to create a User, so it should be called
        _mockUserRepository.Verify(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateLastLoginAsync_WithValidId_ShouldCompleteSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("test@example.com", "Test User") { Id = userId };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(user);

        // Act & Assert - Should not throw
        await _userService.UpdateLastLoginAsync(userId);

        Assert.NotNull(user.LastLoginDate);
        _mockUserRepository.Verify(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateLastLoginAsync_WithInvalidId_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.UpdateLastLoginAsync(userId));
        _mockUserRepository.Verify(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserProfileAsync_WithValidData_ShouldReturnUpdatedUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newName = "Updated Name";
        var user = new User("test@example.com", "Old Name") { Id = userId };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(user);

        // Act
        var result = await _userService.UpdateUserProfileAsync(userId, newName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal(newName, result.Name);
        _mockUserRepository.Verify(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("test@example.com", "Test User") { Id = userId };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.DeleteAsync(userId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(true);

        // Act
        var result = await _userService.DeleteUserAsync(userId);

        // Assert
        Assert.True(result);
        _mockUserRepository.Verify(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserRepository.Verify(x => x.DeleteAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.DeleteUserAsync(userId);

        // Assert
        Assert.False(result);
        _mockUserRepository.Verify(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
