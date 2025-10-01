using System.Net.Http.Json;
using System.Text.Json;
using LiftTracker.Application.DTOs;

namespace LiftTracker.Client.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AuthService _authService;

    public ApiClient(HttpClient httpClient, AuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }

    private async Task EnsureAuthenticatedAsync()
    {
        await _authService.RefreshTokenIfNeededAsync();

        var token = await _authService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }

    // User operations
    public async Task<UserDto?> GetCurrentUserAsync()
    {
        await EnsureAuthenticatedAsync();

        try
        {
            return await _httpClient.GetFromJsonAsync<UserDto>("api/users/me");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<UserDto?> UpdateUserAsync(UpdateUserDto updateUserDto)
    {
        await EnsureAuthenticatedAsync();

        try
        {
            var response = await _httpClient.PutAsJsonAsync("api/users/me", updateUserDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserDto>();
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    // Workout Session operations
    public async Task<List<WorkoutSessionDto>> GetWorkoutSessionsAsync(int page = 1, int pageSize = 10)
    {
        await EnsureAuthenticatedAsync();

        try
        {
            return await _httpClient.GetFromJsonAsync<List<WorkoutSessionDto>>(
                $"api/workout-sessions?page={page}&pageSize={pageSize}") ?? new List<WorkoutSessionDto>();
        }
        catch (HttpRequestException)
        {
            return new List<WorkoutSessionDto>();
        }
    }

    public async Task<WorkoutSessionDto?> GetWorkoutSessionAsync(int id)
    {
        await EnsureAuthenticatedAsync();

        try
        {
            return await _httpClient.GetFromJsonAsync<WorkoutSessionDto>($"api/workout-sessions/{id}");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<WorkoutSessionDto?> CreateWorkoutSessionAsync(CreateWorkoutSessionDto createDto)
    {
        await EnsureAuthenticatedAsync();

        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/workout-sessions", createDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<WorkoutSessionDto>();
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<WorkoutSessionDto?> UpdateWorkoutSessionAsync(int id, UpdateWorkoutSessionDto updateDto)
    {
        await EnsureAuthenticatedAsync();

        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/workout-sessions/{id}", updateDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<WorkoutSessionDto>();
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<bool> DeleteWorkoutSessionAsync(int id)
    {
        await EnsureAuthenticatedAsync();

        try
        {
            var response = await _httpClient.DeleteAsync($"api/workout-sessions/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    // Strength Lift operations
    public async Task<List<StrengthLiftDto>> GetStrengthLiftsAsync(int workoutSessionId)
    {
        await EnsureAuthenticatedAsync();

        try
        {
            return await _httpClient.GetFromJsonAsync<List<StrengthLiftDto>>(
                $"api/strength-lifts?workoutSessionId={workoutSessionId}") ?? new List<StrengthLiftDto>();
        }
        catch (HttpRequestException)
        {
            return new List<StrengthLiftDto>();
        }
    }

    public async Task<StrengthLiftDto?> CreateStrengthLiftAsync(CreateStrengthLiftDto createDto)
    {
        await EnsureAuthenticatedAsync();

        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/strength-lifts", createDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<StrengthLiftDto>();
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<StrengthLiftDto?> UpdateStrengthLiftAsync(int id, UpdateStrengthLiftDto updateDto)
    {
        await EnsureAuthenticatedAsync();

        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/strength-lifts/{id}", updateDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<StrengthLiftDto>();
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<bool> DeleteStrengthLiftAsync(int id)
    {
        await EnsureAuthenticatedAsync();

        try
        {
            var response = await _httpClient.DeleteAsync($"api/strength-lifts/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    // Metcon Workout operations
    public async Task<List<MetconWorkoutDto>> GetMetconWorkoutsAsync(int workoutSessionId)
    {
        await EnsureAuthenticatedAsync();

        try
        {
            return await _httpClient.GetFromJsonAsync<List<MetconWorkoutDto>>(
                $"api/metcon-workouts?workoutSessionId={workoutSessionId}") ?? new List<MetconWorkoutDto>();
        }
        catch (HttpRequestException)
        {
            return new List<MetconWorkoutDto>();
        }
    }

    public async Task<MetconWorkoutDto?> CreateMetconWorkoutAsync(CreateMetconWorkoutDto createDto)
    {
        await EnsureAuthenticatedAsync();

        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/metcon-workouts", createDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<MetconWorkoutDto>();
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<MetconWorkoutDto?> UpdateMetconWorkoutAsync(int id, UpdateMetconWorkoutDto updateDto)
    {
        await EnsureAuthenticatedAsync();

        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/metcon-workouts/{id}", updateDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<MetconWorkoutDto>();
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<bool> DeleteMetconWorkoutAsync(int id)
    {
        await EnsureAuthenticatedAsync();

        try
        {
            var response = await _httpClient.DeleteAsync($"api/metcon-workouts/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    // Exercise Type operations
    public async Task<List<ExerciseTypeDto>> GetExerciseTypesAsync()
    {
        await EnsureAuthenticatedAsync();

        try
        {
            return await _httpClient.GetFromJsonAsync<List<ExerciseTypeDto>>("api/exercise-types")
                   ?? new List<ExerciseTypeDto>();
        }
        catch (HttpRequestException)
        {
            return new List<ExerciseTypeDto>();
        }
    }

    // Progress operations
    public async Task<ProgressDto?> GetProgressAsync(int days = 30)
    {
        await EnsureAuthenticatedAsync();

        try
        {
            return await _httpClient.GetFromJsonAsync<ProgressDto>($"api/progress?days={days}");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<List<StrengthProgressDto>> GetStrengthProgressAsync(int exerciseTypeId, int days = 30)
    {
        await EnsureAuthenticatedAsync();

        try
        {
            return await _httpClient.GetFromJsonAsync<List<StrengthProgressDto>>(
                $"api/progress/strength?exerciseTypeId={exerciseTypeId}&days={days}")
                ?? new List<StrengthProgressDto>();
        }
        catch (HttpRequestException)
        {
            return new List<StrengthProgressDto>();
        }
    }

    public async Task<List<MetconProgressDto>> GetMetconProgressAsync(int metconTypeId, int days = 30)
    {
        await EnsureAuthenticatedAsync();

        try
        {
            return await _httpClient.GetFromJsonAsync<List<MetconProgressDto>>(
                $"api/progress/metcon?metconTypeId={metconTypeId}&days={days}")
                ?? new List<MetconProgressDto>();
        }
        catch (HttpRequestException)
        {
            return new List<MetconProgressDto>();
        }
    }
}
