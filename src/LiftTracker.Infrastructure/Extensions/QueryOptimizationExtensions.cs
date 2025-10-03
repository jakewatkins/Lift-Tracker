using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LiftTracker.Infrastructure.Extensions;

/// <summary>
/// Extension methods for optimizing Entity Framework queries
/// </summary>
public static class QueryOptimizationExtensions
{
    /// <summary>
    /// Applies common query optimizations including tracking and split query behavior
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="queryable">The queryable to optimize</param>
    /// <param name="trackChanges">Whether to track changes (default: false for read-only scenarios)</param>
    /// <param name="useSplitQuery">Whether to use split query for includes (default: false)</param>
    /// <returns>Optimized queryable</returns>
    public static IQueryable<T> ApplyOptimizations<T>(
        this IQueryable<T> queryable,
        bool trackChanges = false,
        bool useSplitQuery = false) where T : class
    {
        var optimizedQuery = queryable;

        // Disable change tracking for read-only scenarios
        if (!trackChanges)
        {
            optimizedQuery = optimizedQuery.AsNoTracking();
        }

        // Use split query for complex includes to avoid Cartesian explosion
        if (useSplitQuery)
        {
            optimizedQuery = optimizedQuery.AsSplitQuery();
        }

        return optimizedQuery;
    }

    /// <summary>
    /// Applies pagination with optimization
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="queryable">The queryable to paginate</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="trackChanges">Whether to track changes</param>
    /// <returns>Paginated and optimized queryable</returns>
    public static IQueryable<T> ApplyPagination<T>(
        this IQueryable<T> queryable,
        int pageNumber,
        int pageSize,
        bool trackChanges = false) where T : class
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100; // Limit max page size

        return queryable
            .ApplyOptimizations(trackChanges)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }

    /// <summary>
    /// Applies ordering with null handling
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <param name="queryable">The queryable to order</param>
    /// <param name="keySelector">Key selector expression</param>
    /// <param name="ascending">Whether to sort ascending (default: true)</param>
    /// <returns>Ordered queryable</returns>
    public static IQueryable<T> ApplyOrdering<T, TKey>(
        this IQueryable<T> queryable,
        Expression<Func<T, TKey>> keySelector,
        bool ascending = true) where T : class
    {
        return ascending
            ? queryable.OrderBy(keySelector)
            : queryable.OrderByDescending(keySelector);
    }

    /// <summary>
    /// Applies filtering with null-safe string operations
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="queryable">The queryable to filter</param>
    /// <param name="searchTerm">Search term to filter by</param>
    /// <param name="stringSelector">String property selector</param>
    /// <returns>Filtered queryable</returns>
    public static IQueryable<T> ApplyStringFilter<T>(
        this IQueryable<T> queryable,
        string? searchTerm,
        Expression<Func<T, string?>> stringSelector) where T : class
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return queryable;

        // Create a safe filter that handles null values
        var parameter = stringSelector.Parameters[0];
        var property = stringSelector.Body;

        // Create: x => x.Property != null && x.Property.ToLower().Contains(searchTerm.ToLower())
        var nullCheck = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));
        var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;

        var propertyToLower = Expression.Call(property, toLowerMethod);
        var searchTermLower = Expression.Constant(searchTerm.ToLowerInvariant());
        var containsCall = Expression.Call(propertyToLower, containsMethod, searchTermLower);

        var combined = Expression.AndAlso(nullCheck, containsCall);
        var lambda = Expression.Lambda<Func<T, bool>>(combined, parameter);

        return queryable.Where(lambda);
    }

    /// <summary>
    /// Applies date range filtering
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="queryable">The queryable to filter</param>
    /// <param name="dateSelector">Date property selector</param>
    /// <param name="startDate">Start date (inclusive, optional)</param>
    /// <param name="endDate">End date (inclusive, optional)</param>
    /// <returns>Filtered queryable</returns>
    public static IQueryable<T> ApplyDateRangeFilter<T>(
        this IQueryable<T> queryable,
        Expression<Func<T, DateTime>> dateSelector,
        DateTime? startDate = null,
        DateTime? endDate = null) where T : class
    {
        if (startDate.HasValue)
        {
            var startPredicate = CreateDateComparison(dateSelector, startDate.Value, true);
            queryable = queryable.Where(startPredicate);
        }

        if (endDate.HasValue)
        {
            var endPredicate = CreateDateComparison(dateSelector, endDate.Value.Date.AddDays(1).AddTicks(-1), false);
            queryable = queryable.Where(endPredicate);
        }

        return queryable;
    }

    /// <summary>
    /// Creates a date comparison expression
    /// </summary>
    private static Expression<Func<T, bool>> CreateDateComparison<T>(
        Expression<Func<T, DateTime>> dateSelector,
        DateTime compareDate,
        bool greaterThanOrEqual) where T : class
    {
        var parameter = dateSelector.Parameters[0];
        var property = dateSelector.Body;
        var constant = Expression.Constant(compareDate);

        var comparison = greaterThanOrEqual
            ? Expression.GreaterThanOrEqual(property, constant)
            : Expression.LessThanOrEqual(property, constant);

        return Expression.Lambda<Func<T, bool>>(comparison, parameter);
    }

    /// <summary>
    /// Applies include optimizations for navigation properties
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="queryable">The queryable to optimize</param>
    /// <param name="includeProperties">Navigation properties to include</param>
    /// <param name="useSplitQuery">Whether to use split query to avoid Cartesian explosion</param>
    /// <returns>Queryable with optimized includes</returns>
    public static IQueryable<T> ApplyIncludes<T>(
        this IQueryable<T> queryable,
        bool useSplitQuery = true,
        params Expression<Func<T, object>>[] includeProperties) where T : class
    {
        var result = queryable;

        // Apply split query if needed to avoid Cartesian explosion
        if (useSplitQuery && includeProperties.Length > 1)
        {
            result = result.AsSplitQuery();
        }

        // Apply includes
        foreach (var includeProperty in includeProperties)
        {
            result = result.Include(includeProperty);
        }

        return result;
    }
}
