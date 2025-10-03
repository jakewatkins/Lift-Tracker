namespace LiftTracker.Infrastructure.Caching;

/// <summary>
/// Cache key constants for consistent cache key management
/// </summary>
public static class CacheKeys
{
    // User-related cache keys
    public const string USER_BY_ID = "user:id:{0}";
    public const string USER_BY_EMAIL = "user:email:{0}";
    public const string USER_WORKOUTS = "user:{0}:workouts";
    public const string USER_PROGRESS = "user:{0}:progress";

    // Exercise type cache keys
    public const string EXERCISE_TYPES = "exercise-types";
    public const string EXERCISE_TYPE_BY_ID = "exercise-type:id:{0}";
    public const string EXERCISE_TYPES_BY_CATEGORY = "exercise-types:category:{0}";

    // Metcon type cache keys
    public const string METCON_TYPES = "metcon-types";
    public const string METCON_TYPE_BY_ID = "metcon-type:id:{0}";

    // Movement type cache keys
    public const string MOVEMENT_TYPES = "movement-types";
    public const string MOVEMENT_TYPE_BY_ID = "movement-type:id:{0}";

    // Workout session cache keys
    public const string WORKOUT_SESSION_BY_ID = "workout-session:id:{0}";
    public const string USER_WORKOUT_SESSIONS = "user:{0}:workout-sessions";
    public const string USER_WORKOUT_SESSIONS_DATE_RANGE = "user:{0}:workout-sessions:{1}:{2}";

    // Progress and statistics cache keys
    public const string USER_STRENGTH_PROGRESS = "user:{0}:strength-progress:{1}";
    public const string USER_METCON_PROGRESS = "user:{0}:metcon-progress:{1}";
    public const string USER_RECENT_WORKOUTS = "user:{0}:recent-workouts:{1}";

    // Cache expiration times
    public static class Expiration
    {
        public static readonly TimeSpan Short = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan Medium = TimeSpan.FromMinutes(30);
        public static readonly TimeSpan Long = TimeSpan.FromHours(2);
        public static readonly TimeSpan ExtraLong = TimeSpan.FromHours(24);

        // User data - medium expiration
        public static readonly TimeSpan UserData = Medium;

        // Reference data - long expiration (exercise types, metcon types, etc.)
        public static readonly TimeSpan ReferenceData = Long;

        // Progress data - short expiration for real-time updates
        public static readonly TimeSpan ProgressData = Short;

        // Workout sessions - medium expiration
        public static readonly TimeSpan WorkoutData = Medium;
    }

    // Cache key patterns for bulk operations
    public static class Patterns
    {
        public const string USER_ALL = "user:.*";
        public const string USER_SPECIFIC = "user:{0}:.*";
        public const string EXERCISE_TYPES_ALL = "exercise-type.*";
        public const string METCON_TYPES_ALL = "metcon-type.*";
        public const string MOVEMENT_TYPES_ALL = "movement-type.*";
        public const string WORKOUT_SESSIONS_ALL = "workout-session.*";
        public const string PROGRESS_ALL = ".*:progress.*";
    }
}
