using AutoMapper;
using LiftTracker.Application.DTOs;
using LiftTracker.Application.Interfaces;
using LiftTracker.Domain.Entities;
using LiftTracker.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace LiftTracker.Application.Services;

/// <summary>
/// Service implementation for user management operations
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting user by ID: {UserId}", userId);

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            _logger.LogDebug("User not found with ID: {UserId}", userId);
        }

        return user;
    }

    /// <inheritdoc />
    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting user by email: {Email}", email);

        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (user == null)
        {
            _logger.LogDebug("User not found with email: {Email}", email);
        }

        return user;
    }

    /// <inheritdoc />
    public async Task<User> CreateUserAsync(CreateUserDto createUserDto, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Creating new user with email: {Email}", createUserDto.Email);

        // Check if user already exists
        var existingUser = await _userRepository.GetByEmailAsync(createUserDto.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("User already exists with email: {Email}", createUserDto.Email);
            throw new InvalidOperationException($"User with email {createUserDto.Email} already exists");
        }

        var user = _mapper.Map<User>(createUserDto);
        user.Id = Guid.NewGuid();

        var createdUser = await _userRepository.CreateAsync(user, cancellationToken);

        _logger.LogInformation("User created successfully with ID: {UserId}", createdUser.Id);
        return createdUser;
    }

    /// <inheritdoc />
    public async Task<User> UpdateUserAsync(Guid userId, UpdateUserDto updateUserDto, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Updating user with ID: {UserId}", userId);

        var existingUser = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (existingUser == null)
        {
            _logger.LogWarning("User not found for update with ID: {UserId}", userId);
            throw new InvalidOperationException($"User with ID {userId} not found");
        }

        _mapper.Map(updateUserDto, existingUser);
        var updatedUser = await _userRepository.UpdateAsync(existingUser, cancellationToken);

        _logger.LogInformation("User updated successfully with ID: {UserId}", userId);
        return updatedUser;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Deleting user with ID: {UserId}", userId);

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found for deletion with ID: {UserId}", userId);
            return false;
        }

        var result = await _userRepository.DeleteAsync(userId);

        if (result)
        {
            _logger.LogInformation("User deleted successfully with ID: {UserId}", userId);
        }
        else
        {
            _logger.LogWarning("Failed to delete user with ID: {UserId}", userId);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<User> CreateOrUpdateUserAsync(string email, string name, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Creating or updating user with email: {Email}", email);

        var existingUser = await _userRepository.GetByEmailAsync(email);
        if (existingUser != null)
        {
            // Update existing user
            existingUser.Name = name;
            existingUser.UpdateLastLogin();
            var updatedUser = await _userRepository.UpdateAsync(existingUser);
            _logger.LogInformation("User updated successfully with ID: {UserId}", updatedUser.Id);
            return updatedUser;
        }
        else
        {
            // Create new user
            var user = new User(email, name);
            user.UpdateLastLogin();

            var createdUser = await _userRepository.CreateAsync(user);
            _logger.LogInformation("User created successfully with ID: {UserId}", createdUser.Id);
            return createdUser;
        }
    }

    /// <inheritdoc />
    public async Task UpdateLastLoginAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Updating last login for user with ID: {UserId}", userId);

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found for last login update with ID: {UserId}", userId);
            throw new InvalidOperationException($"User with ID {userId} not found");
        }

        user.UpdateLastLogin();
        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("Last login updated successfully for user with ID: {UserId}", userId);
    }

    /// <inheritdoc />
    public async Task<User> UpdateUserProfileAsync(Guid userId, string name, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Updating profile for user with ID: {UserId}", userId);

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found for profile update with ID: {UserId}", userId);
            throw new InvalidOperationException($"User with ID {userId} not found");
        }

        user.Name = name;
        var updatedUser = await _userRepository.UpdateAsync(user);

        _logger.LogInformation("User profile updated successfully with ID: {UserId}", userId);
        return updatedUser;
    }

    /// <inheritdoc />
    public async Task<bool> UserExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Checking if user exists with email: {Email}", email);

        return await _userRepository.ExistsAsync(email);
    }
}
