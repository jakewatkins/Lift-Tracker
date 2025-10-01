namespace LiftTracker.Domain.ValueObjects;

/// <summary>
/// Value object representing duration with validation for fractional increments
/// </summary>
public record Duration
{
    /// <summary>
    /// Duration value in minutes
    /// </summary>
    public decimal Value { get; }

    /// <summary>
    /// Creates a new Duration instance
    /// </summary>
    /// <param name="value">Duration value in minutes</param>
    /// <exception cref="ArgumentException">Thrown when duration is negative or uses invalid fractional increments</exception>
    public Duration(decimal value)
    {
        if (value < 0)
            throw new ArgumentException("Duration cannot be negative", nameof(value));

        if (!IsValidFractionalIncrement(value))
            throw new ArgumentException("Duration must use fractional increments of 0.25 (0.0, 0.25, 0.5, 0.75)", nameof(value));

        Value = value;
    }

    /// <summary>
    /// Validates that the duration uses only fractional increments of 0.25
    /// </summary>
    /// <param name="duration">Duration to validate</param>
    /// <returns>True if duration uses valid fractional increments</returns>
    public static bool IsValidFractionalIncrement(decimal duration)
    {
        var fractionalPart = duration - Math.Floor(duration);
        var validFractions = new[] { 0.0m, 0.25m, 0.5m, 0.75m };
        return validFractions.Contains(fractionalPart);
    }

    /// <summary>
    /// Creates a Duration from a decimal value in minutes
    /// </summary>
    /// <param name="minutes">Duration value in minutes</param>
    /// <returns>Duration instance</returns>
    public static Duration FromMinutes(decimal minutes) => new(minutes);

    /// <summary>
    /// Creates a Duration from seconds (converted to minutes)
    /// </summary>
    /// <param name="seconds">Duration value in seconds</param>
    /// <returns>Duration instance</returns>
    public static Duration FromSeconds(decimal seconds) => new(seconds / 60);

    /// <summary>
    /// Creates a Duration from hours (converted to minutes)
    /// </summary>
    /// <param name="hours">Duration value in hours</param>
    /// <returns>Duration instance</returns>
    public static Duration FromHours(decimal hours) => new(hours * 60);

    /// <summary>
    /// Implicit conversion from decimal to Duration (assumes minutes)
    /// </summary>
    /// <param name="value">Duration value in minutes</param>
    public static implicit operator Duration(decimal value) => new(value);

    /// <summary>
    /// Implicit conversion from Duration to decimal
    /// </summary>
    /// <param name="duration">Duration instance</param>
    public static implicit operator decimal(Duration duration) => duration.Value;

    /// <summary>
    /// Addition operator for durations
    /// </summary>
    /// <param name="left">Left duration</param>
    /// <param name="right">Right duration</param>
    /// <returns>Sum of durations</returns>
    public static Duration operator +(Duration left, Duration right) => new(left.Value + right.Value);

    /// <summary>
    /// Subtraction operator for durations
    /// </summary>
    /// <param name="left">Left duration</param>
    /// <param name="right">Right duration</param>
    /// <returns>Difference of durations</returns>
    public static Duration operator -(Duration left, Duration right) => new(left.Value - right.Value);

    /// <summary>
    /// Multiplication operator for duration and scalar
    /// </summary>
    /// <param name="duration">Duration</param>
    /// <param name="multiplier">Scalar multiplier</param>
    /// <returns>Multiplied duration</returns>
    public static Duration operator *(Duration duration, decimal multiplier) => new(duration.Value * multiplier);

    /// <summary>
    /// Division operator for duration and scalar
    /// </summary>
    /// <param name="duration">Duration</param>
    /// <param name="divisor">Scalar divisor</param>
    /// <returns>Divided duration</returns>
    public static Duration operator /(Duration duration, decimal divisor) => new(duration.Value / divisor);

    /// <summary>
    /// Comparison operator for durations
    /// </summary>
    /// <param name="left">Left duration</param>
    /// <param name="right">Right duration</param>
    /// <returns>True if left is greater than right</returns>
    public static bool operator >(Duration left, Duration right) => left.Value > right.Value;

    /// <summary>
    /// Comparison operator for durations
    /// </summary>
    /// <param name="left">Left duration</param>
    /// <param name="right">Right duration</param>
    /// <returns>True if left is less than right</returns>
    public static bool operator <(Duration left, Duration right) => left.Value < right.Value;

    /// <summary>
    /// Comparison operator for durations
    /// </summary>
    /// <param name="left">Left duration</param>
    /// <param name="right">Right duration</param>
    /// <returns>True if left is greater than or equal to right</returns>
    public static bool operator >=(Duration left, Duration right) => left.Value >= right.Value;

    /// <summary>
    /// Comparison operator for durations
    /// </summary>
    /// <param name="left">Left duration</param>
    /// <param name="right">Right duration</param>
    /// <returns>True if left is less than or equal to right</returns>
    public static bool operator <=(Duration left, Duration right) => left.Value <= right.Value;

    /// <summary>
    /// Gets the duration in seconds
    /// </summary>
    public decimal TotalSeconds => Value * 60;

    /// <summary>
    /// Gets the duration in hours
    /// </summary>
    public decimal TotalHours => Value / 60;

    /// <summary>
    /// Returns a string representation of the duration
    /// </summary>
    /// <returns>Duration formatted as "X.XX min"</returns>
    public override string ToString() => $"{Value:0.##} min";

    /// <summary>
    /// Returns a human-readable string representation of the duration
    /// </summary>
    /// <returns>Duration formatted as "X:YY" (minutes:seconds) or "X hr Y min" for longer durations</returns>
    public string ToHumanString()
    {
        if (Value >= 60)
        {
            var hours = (int)(Value / 60);
            var minutes = (int)(Value % 60);
            var seconds = (int)((Value % 1) * 60);
            
            if (seconds > 0)
                return $"{hours} hr {minutes} min {seconds} sec";
            else if (minutes > 0)
                return $"{hours} hr {minutes} min";
            else
                return $"{hours} hr";
        }
        else
        {
            var minutes = (int)Value;
            var seconds = (int)((Value % 1) * 60);
            
            if (minutes > 0)
                return $"{minutes}:{seconds:00}";
            else
                return $"0:{seconds:00}";
        }
    }

    /// <summary>
    /// Zero duration constant
    /// </summary>
    public static Duration Zero => new(0m);
}