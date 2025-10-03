using LiftTracker.API.Controllers;
using LiftTracker.Application.DTOs;
using LiftTracker.Application.Interfaces;
using LiftTracker.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace LiftTracker.API.Tests;

/// <summary>
/// Unit tests for UsersController
/// </summary>
public class UsersControllerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<ILogger<UsersController>> _loggerMock;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _loggerMock = new Mock<ILogger<UsersController>>();
        _controller = new UsersController(_userServiceMock.Object, _loggerMock.Object);
    }

    /// <summary>
    /// Sets up authenticated user context for tests
    /// </summary>
    private void SetupAuthenticatedUser(Guid userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    #region GetCurrentUser Tests

    [Fact]
    public async Task GetCurrentUser_WithValidUser_ReturnsOkWithUserData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "John Doe",
            Email = "john@example.com",
            CreatedDate = DateTime.UtcNow,
            LastLoginDate = DateTime.UtcNow
        };

        SetupAuthenticatedUser(userId);
        _userServiceMock.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _userServiceMock.Verify(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCurrentUser_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange - No authenticated user set up

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(unauthorizedResult.Value);
    }

    [Fact]
    public async Task GetCurrentUser_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);
        _userServiceMock.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(notFoundResult.Value);
    }

    #endregion

    #region UpdateCurrentUser Tests

    [Fact]
    public async Task UpdateCurrentUser_WithValidData_ReturnsOkWithUpdatedUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateDto = new UpdateUserDto { Name = "Jane Smith" };
        var updatedUser = new User
        {
            Id = userId,
            Name = "Jane Smith",
            Email = "jane@example.com",
            CreatedDate = DateTime.UtcNow,
            LastLoginDate = DateTime.UtcNow
        };

        SetupAuthenticatedUser(userId);
        _userServiceMock.Setup(x => x.UpdateUserProfileAsync(userId, updateDto.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _controller.UpdateCurrentUser(updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _userServiceMock.Verify(x => x.UpdateUserProfileAsync(userId, updateDto.Name, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCurrentUser_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var updateDto = new UpdateUserDto { Name = "Jane Smith" };

        // Act
        var result = await _controller.UpdateCurrentUser(updateDto);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(unauthorizedResult.Value);
    }

    [Fact]
    public async Task UpdateCurrentUser_WithInvalidOperationException_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateDto = new UpdateUserDto { Name = "Jane Smith" };

        SetupAuthenticatedUser(userId);
        _userServiceMock.Setup(x => x.UpdateUserProfileAsync(userId, updateDto.Name, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("User not found"));

        // Act
        var result = await _controller.UpdateCurrentUser(updateDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(notFoundResult.Value);
    }

    [Fact]
    public async Task UpdateCurrentUser_WithGenericException_ReturnsInternalServerError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateDto = new UpdateUserDto { Name = "Jane Smith" };

        SetupAuthenticatedUser(userId);
        _userServiceMock.Setup(x => x.UpdateUserProfileAsync(userId, updateDto.Name, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.UpdateCurrentUser(updateDto);

        // Assert
        var serverErrorResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, serverErrorResult.StatusCode);
    }

    #endregion

    #region DeleteCurrentUser Tests

    [Fact]
    public async Task DeleteCurrentUser_WithValidUser_ReturnsOkWithConfirmation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);
        _userServiceMock.Setup(x => x.DeleteUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteCurrentUser();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _userServiceMock.Verify(x => x.DeleteUserAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCurrentUser_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange - No authenticated user set up

        // Act
        var result = await _controller.DeleteCurrentUser();

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(unauthorizedResult.Value);
    }

    [Fact]
    public async Task DeleteCurrentUser_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);
        _userServiceMock.Setup(x => x.DeleteUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteCurrentUser();

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(notFoundResult.Value);
    }

    [Fact]
    public async Task DeleteCurrentUser_WithException_ReturnsInternalServerError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);
        _userServiceMock.Setup(x => x.DeleteUserAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.DeleteCurrentUser();

        // Assert
        var serverErrorResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, serverErrorResult.StatusCode);
    }

    #endregion

    #region CheckEmailAvailability Tests

    [Fact]
    public async Task CheckEmailAvailability_WithValidEmail_ReturnsAvailabilityStatus()
    {
        // Arrange
        var email = "test@example.com";
        _userServiceMock.Setup(x => x.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null); // Email is available

        // Act
        var result = await _controller.CheckEmailAvailability(email);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _userServiceMock.Verify(x => x.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckEmailAvailability_WithEmptyEmail_ReturnsBadRequest()
    {
        // Arrange
        var email = "";

        // Act
        var result = await _controller.CheckEmailAvailability(email);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task CheckEmailAvailability_WithException_ReturnsInternalServerError()
    {
        // Arrange
        var email = "test@example.com";
        _userServiceMock.Setup(x => x.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.CheckEmailAvailability(email);

        // Assert
        var serverErrorResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, serverErrorResult.StatusCode);
    }

    #endregion

    #region GetUserStats Tests

    [Fact]
    public async Task GetUserStats_WithValidUser_ReturnsOkWithStats()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "John Doe",
            Email = "john@example.com",
            CreatedDate = DateTime.UtcNow,
            LastLoginDate = DateTime.UtcNow
        };

        SetupAuthenticatedUser(userId);
        _userServiceMock.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.GetUserStats();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _userServiceMock.Verify(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserStats_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange - No authenticated user set up

        // Act
        var result = await _controller.GetUserStats();

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(unauthorizedResult.Value);
    }

    [Fact]
    public async Task GetUserStats_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);
        _userServiceMock.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.GetUserStats();

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(notFoundResult.Value);
    }

    [Fact]
    public async Task GetUserStats_WithException_ReturnsInternalServerError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupAuthenticatedUser(userId);
        _userServiceMock.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetUserStats();

        // Assert
        var serverErrorResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, serverErrorResult.StatusCode);
    }

    #endregion
}
