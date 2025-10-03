using LiftTracker.Application.DTOs;
using System.ComponentModel;

namespace LiftTracker.Client.Services;

public class StateService : INotifyPropertyChanged
{
    private readonly ApiClient _apiClient;
    private readonly AuthService _authService;

    // User state
    private UserDto? _currentUser;
    private bool _isLoadingUser;

    // Workout sessions state
    private List<WorkoutSessionDto> _workoutSessions = new();
    private WorkoutSessionDto? _selectedWorkoutSession;
    private bool _isLoadingWorkoutSessions;

    // Exercise types state
    private List<ExerciseTypeDto> _exerciseTypes = new();
    private bool _isLoadingExerciseTypes;

    // Strength lifts state
    private List<StrengthLiftDto> _strengthLifts = new();
    private bool _isLoadingStrengthLifts;

    // Metcon workouts state
    private List<MetconWorkoutDto> _metconWorkouts = new();
    private bool _isLoadingMetconWorkouts;

    // Progress state
    private ProgressDto? _progress;
    private bool _isLoadingProgress;

    // UI state
    private bool _isSidebarOpen = true;
    private string _activeView = "dashboard";
    private Dictionary<string, object> _notifications = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public StateService(ApiClient apiClient, AuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;

        // Subscribe to authentication changes
        _authService.AuthenticationStateChanged += OnAuthenticationStateChanged;
    }

    // User properties
    public UserDto? CurrentUser
    {
        get => _currentUser;
        private set
        {
            _currentUser = value;
            OnPropertyChanged();
        }
    }

    public bool IsLoadingUser
    {
        get => _isLoadingUser;
        private set
        {
            _isLoadingUser = value;
            OnPropertyChanged();
        }
    }

    // Workout sessions properties
    public List<WorkoutSessionDto> WorkoutSessions
    {
        get => _workoutSessions;
        private set
        {
            _workoutSessions = value;
            OnPropertyChanged();
        }
    }

    public WorkoutSessionDto? SelectedWorkoutSession
    {
        get => _selectedWorkoutSession;
        set
        {
            _selectedWorkoutSession = value;
            OnPropertyChanged();
            _ = LoadWorkoutDetailsAsync();
        }
    }

    public bool IsLoadingWorkoutSessions
    {
        get => _isLoadingWorkoutSessions;
        private set
        {
            _isLoadingWorkoutSessions = value;
            OnPropertyChanged();
        }
    }

    // Exercise types properties
    public List<ExerciseTypeDto> ExerciseTypes
    {
        get => _exerciseTypes;
        private set
        {
            _exerciseTypes = value;
            OnPropertyChanged();
        }
    }

    public bool IsLoadingExerciseTypes
    {
        get => _isLoadingExerciseTypes;
        private set
        {
            _isLoadingExerciseTypes = value;
            OnPropertyChanged();
        }
    }

    // Strength lifts properties
    public List<StrengthLiftDto> StrengthLifts
    {
        get => _strengthLifts;
        private set
        {
            _strengthLifts = value;
            OnPropertyChanged();
        }
    }

    public bool IsLoadingStrengthLifts
    {
        get => _isLoadingStrengthLifts;
        private set
        {
            _isLoadingStrengthLifts = value;
            OnPropertyChanged();
        }
    }

    // Metcon workouts properties
    public List<MetconWorkoutDto> MetconWorkouts
    {
        get => _metconWorkouts;
        private set
        {
            _metconWorkouts = value;
            OnPropertyChanged();
        }
    }

    public bool IsLoadingMetconWorkouts
    {
        get => _isLoadingMetconWorkouts;
        private set
        {
            _isLoadingMetconWorkouts = value;
            OnPropertyChanged();
        }
    }

    // Progress properties
    public ProgressDto? Progress
    {
        get => _progress;
        private set
        {
            _progress = value;
            OnPropertyChanged();
        }
    }

    public bool IsLoadingProgress
    {
        get => _isLoadingProgress;
        private set
        {
            _isLoadingProgress = value;
            OnPropertyChanged();
        }
    }

    // UI properties
    public bool IsSidebarOpen
    {
        get => _isSidebarOpen;
        set
        {
            _isSidebarOpen = value;
            OnPropertyChanged();
        }
    }

    public string ActiveView
    {
        get => _activeView;
        set
        {
            _activeView = value;
            OnPropertyChanged();
        }
    }

    // Initialization
    public async Task InitializeAsync()
    {
        if (_authService.IsAuthenticated)
        {
            await LoadUserAsync();
            await LoadExerciseTypesAsync();
            await LoadWorkoutSessionsAsync();
        }
    }

    // User operations
    public async Task LoadUserAsync()
    {
        if (!_authService.IsAuthenticated) return;

        IsLoadingUser = true;
        try
        {
            CurrentUser = await _apiClient.GetCurrentUserAsync();
        }
        finally
        {
            IsLoadingUser = false;
        }
    }

    public async Task<bool> UpdateUserAsync(UpdateUserDto updateDto)
    {
        if (!_authService.IsAuthenticated) return false;

        IsLoadingUser = true;
        try
        {
            var updatedUser = await _apiClient.UpdateUserAsync(updateDto);
            if (updatedUser != null)
            {
                CurrentUser = updatedUser;
                ShowNotification("Profile updated successfully", "success");
                return true;
            }

            ShowNotification("Failed to update profile", "error");
            return false;
        }
        finally
        {
            IsLoadingUser = false;
        }
    }

    // Workout session operations
    public async Task LoadWorkoutSessionsAsync(int page = 1, int pageSize = 10)
    {
        if (!_authService.IsAuthenticated) return;

        IsLoadingWorkoutSessions = true;
        try
        {
            WorkoutSessions = await _apiClient.GetWorkoutSessionsAsync(page, pageSize);
        }
        finally
        {
            IsLoadingWorkoutSessions = false;
        }
    }

    public async Task<bool> CreateWorkoutSessionAsync(CreateWorkoutSessionDto createDto)
    {
        if (!_authService.IsAuthenticated) return false;

        var created = await _apiClient.CreateWorkoutSessionAsync(createDto);
        if (created != null)
        {
            WorkoutSessions = new List<WorkoutSessionDto> { created }.Concat(WorkoutSessions).ToList();
            SelectedWorkoutSession = created;
            ShowNotification("Workout session created successfully", "success");
            return true;
        }

        ShowNotification("Failed to create workout session", "error");
        return false;
    }

    public async Task<bool> UpdateWorkoutSessionAsync(int id, UpdateWorkoutSessionDto updateDto)
    {
        if (!_authService.IsAuthenticated) return false;

        var updated = await _apiClient.UpdateWorkoutSessionAsync(id, updateDto);
        if (updated != null)
        {
            var index = WorkoutSessions.FindIndex(w => w.Id == id);
            if (index >= 0)
            {
                var updatedList = WorkoutSessions.ToList();
                updatedList[index] = updated;
                WorkoutSessions = updatedList;
            }

            if (SelectedWorkoutSession?.Id == id)
            {
                SelectedWorkoutSession = updated;
            }

            ShowNotification("Workout session updated successfully", "success");
            return true;
        }

        ShowNotification("Failed to update workout session", "error");
        return false;
    }

    public async Task<bool> DeleteWorkoutSessionAsync(int id)
    {
        if (!_authService.IsAuthenticated) return false;

        var success = await _apiClient.DeleteWorkoutSessionAsync(id);
        if (success)
        {
            WorkoutSessions = WorkoutSessions.Where(w => w.Id != id).ToList();

            if (SelectedWorkoutSession?.Id == id)
            {
                SelectedWorkoutSession = null;
            }

            ShowNotification("Workout session deleted successfully", "success");
            return true;
        }

        ShowNotification("Failed to delete workout session", "error");
        return false;
    }

    // Exercise type operations
    public async Task LoadExerciseTypesAsync()
    {
        if (!_authService.IsAuthenticated) return;

        IsLoadingExerciseTypes = true;
        try
        {
            ExerciseTypes = await _apiClient.GetExerciseTypesAsync();
        }
        finally
        {
            IsLoadingExerciseTypes = false;
        }
    }

    // Strength lift operations
    private async Task LoadWorkoutDetailsAsync()
    {
        if (SelectedWorkoutSession == null) return;

        await LoadStrengthLiftsAsync(SelectedWorkoutSession.Id);
        await LoadMetconWorkoutsAsync(SelectedWorkoutSession.Id);
    }

    private async Task LoadStrengthLiftsAsync(int workoutSessionId)
    {
        IsLoadingStrengthLifts = true;
        try
        {
            StrengthLifts = await _apiClient.GetStrengthLiftsAsync(workoutSessionId);
        }
        finally
        {
            IsLoadingStrengthLifts = false;
        }
    }

    public async Task<bool> CreateStrengthLiftAsync(CreateStrengthLiftDto createDto)
    {
        if (!_authService.IsAuthenticated) return false;

        var created = await _apiClient.CreateStrengthLiftAsync(createDto);
        if (created != null)
        {
            StrengthLifts = StrengthLifts.Concat(new[] { created }).ToList();
            ShowNotification("Strength lift added successfully", "success");
            return true;
        }

        ShowNotification("Failed to add strength lift", "error");
        return false;
    }

    // Metcon workout operations
    private async Task LoadMetconWorkoutsAsync(int workoutSessionId)
    {
        IsLoadingMetconWorkouts = true;
        try
        {
            MetconWorkouts = await _apiClient.GetMetconWorkoutsAsync(workoutSessionId);
        }
        finally
        {
            IsLoadingMetconWorkouts = false;
        }
    }

    public async Task<bool> CreateMetconWorkoutAsync(CreateMetconWorkoutDto createDto)
    {
        if (!_authService.IsAuthenticated) return false;

        var created = await _apiClient.CreateMetconWorkoutAsync(createDto);
        if (created != null)
        {
            MetconWorkouts = MetconWorkouts.Concat(new[] { created }).ToList();
            ShowNotification("Metcon workout added successfully", "success");
            return true;
        }

        ShowNotification("Failed to add metcon workout", "error");
        return false;
    }

    // Progress operations
    public async Task LoadProgressAsync(int days = 30)
    {
        if (!_authService.IsAuthenticated) return;

        IsLoadingProgress = true;
        try
        {
            Progress = await _apiClient.GetProgressAsync(days);
        }
        finally
        {
            IsLoadingProgress = false;
        }
    }

    // UI operations
    public void ShowNotification(string message, string type)
    {
        var notificationId = Guid.NewGuid().ToString();
        _notifications[notificationId] = new { Message = message, Type = type, Timestamp = DateTime.Now };
        OnPropertyChanged(nameof(_notifications));

        // Auto-remove notification after 5 seconds
        _ = Task.Delay(5000).ContinueWith(_ => RemoveNotification(notificationId));
    }

    public void RemoveNotification(string id)
    {
        _notifications.Remove(id);
        OnPropertyChanged(nameof(_notifications));
    }

    public IEnumerable<object> GetNotifications()
    {
        return _notifications.Values;
    }

    // Event handlers
    private async void OnAuthenticationStateChanged(System.Security.Claims.ClaimsPrincipal? user)
    {
        if (user != null)
        {
            await InitializeAsync();
        }
        else
        {
            // Clear all state when user logs out
            CurrentUser = null;
            WorkoutSessions = new List<WorkoutSessionDto>();
            SelectedWorkoutSession = null;
            ExerciseTypes = new List<ExerciseTypeDto>();
            StrengthLifts = new List<StrengthLiftDto>();
            MetconWorkouts = new List<MetconWorkoutDto>();
            Progress = null;
        }
    }

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
