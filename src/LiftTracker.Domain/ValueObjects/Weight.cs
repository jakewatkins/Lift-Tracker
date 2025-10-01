namespace LiftTracker.Domain.ValueObjects;

/// <summary>
/// Value object representing weight with validation for fractional increments
/// </summary>
public record Weight
{
    /// <summary>
    /// Weight value in pounds
    /// </summary>
    public decimal Value { get; }

    /// <summary>
    /// Creates a new Weight instance
    /// </summary>
    /// <param name="value">Weight value in pounds</param>
    /// <exception cref="ArgumentException">Thrown when weight is negative or uses invalid fractional increments</exception>
    public Weight(decimal value)
    {
        if (value < 0)
            throw new ArgumentException("Weight cannot be negative", nameof(value));

        if (!IsValidFractionalIncrement(value))
            throw new ArgumentException("Weight must use fractional increments of 0.25 (0.0, 0.25, 0.5, 0.75)", nameof(value));

        Value = value;
    }

    /// <summary>
    /// Validates that the weight uses only fractional increments of 0.25
    /// </summary>
    /// <param name="weight">Weight to validate</param>
    /// <returns>True if weight uses valid fractional increments</returns>
    public static bool IsValidFractionalIncrement(decimal weight)
    {
        var fractionalPart = weight - Math.Floor(weight);
        var validFractions = new[] { 0.0m, 0.25m, 0.5m, 0.75m };
        return validFractions.Contains(fractionalPart);
    }

    /// <summary>
    /// Creates a Weight from a decimal value
    /// </summary>
    /// <param name="value">Weight value in pounds</param>
    /// <returns>Weight instance</returns>
    public static Weight FromDecimal(decimal value) => new(value);

    /// <summary>
    /// Implicit conversion from decimal to Weight
    /// </summary>
    /// <param name="value">Weight value in pounds</param>
    public static implicit operator Weight(decimal value) => new(value);

    /// <summary>
    /// Implicit conversion from Weight to decimal
    /// </summary>
    /// <param name="weight">Weight instance</param>
    public static implicit operator decimal(Weight weight) => weight.Value;

    /// <summary>
    /// Addition operator for weights
    /// </summary>
    /// <param name="left">Left weight</param>
    /// <param name="right">Right weight</param>
    /// <returns>Sum of weights</returns>
    public static Weight operator +(Weight left, Weight right) => new(left.Value + right.Value);

    /// <summary>
    /// Subtraction operator for weights
    /// </summary>
    /// <param name="left">Left weight</param>
    /// <param name="right">Right weight</param>
    /// <returns>Difference of weights</returns>
    public static Weight operator -(Weight left, Weight right) => new(left.Value - right.Value);

    /// <summary>
    /// Multiplication operator for weight and scalar
    /// </summary>
    /// <param name="weight">Weight</param>
    /// <param name="multiplier">Scalar multiplier</param>
    /// <returns>Multiplied weight</returns>
    public static Weight operator *(Weight weight, decimal multiplier) => new(weight.Value * multiplier);

    /// <summary>
    /// Division operator for weight and scalar
    /// </summary>
    /// <param name="weight">Weight</param>
    /// <param name="divisor">Scalar divisor</param>
    /// <returns>Divided weight</returns>
    public static Weight operator /(Weight weight, decimal divisor) => new(weight.Value / divisor);

    /// <summary>
    /// Comparison operator for weights
    /// </summary>
    /// <param name="left">Left weight</param>
    /// <param name="right">Right weight</param>
    /// <returns>True if left is greater than right</returns>
    public static bool operator >(Weight left, Weight right) => left.Value > right.Value;

    /// <summary>
    /// Comparison operator for weights
    /// </summary>
    /// <param name="left">Left weight</param>
    /// <param name="right">Right weight</param>
    /// <returns>True if left is less than right</returns>
    public static bool operator <(Weight left, Weight right) => left.Value < right.Value;

    /// <summary>
    /// Comparison operator for weights
    /// </summary>
    /// <param name="left">Left weight</param>
    /// <param name="right">Right weight</param>
    /// <returns>True if left is greater than or equal to right</returns>
    public static bool operator >=(Weight left, Weight right) => left.Value >= right.Value;

    /// <summary>
    /// Comparison operator for weights
    /// </summary>
    /// <param name="left">Left weight</param>
    /// <param name="right">Right weight</param>
    /// <returns>True if left is less than or equal to right</returns>
    public static bool operator <=(Weight left, Weight right) => left.Value <= right.Value;

    /// <summary>
    /// Returns a string representation of the weight
    /// </summary>
    /// <returns>Weight formatted as "X.XX lbs"</returns>
    public override string ToString() => $"{Value:0.##} lbs";

    /// <summary>
    /// Zero weight constant
    /// </summary>
    public static Weight Zero => new(0m);

    /// <summary>
    /// Checks if this weight represents bodyweight (zero)
    /// </summary>
    public bool IsBodyweight => Value == 0;
}
