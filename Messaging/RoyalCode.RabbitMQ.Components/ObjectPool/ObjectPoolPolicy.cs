using System;

namespace RoyalCode.RabbitMQ.Components.ObjectPool;

/// <summary>
/// <para>
///     Police for a pool of objects.
/// </para>
/// </summary>
/// <typeparam name="T">The pooled object type.</typeparam>
public class ObjectPoolPolicy<T>
    where T : class
{
    /// <summary>
    /// Creates a new instance with the options and functions.
    /// </summary>
    /// <param name="maxSize">The maximum number of objects that the pool can create and control.</param>
    /// <param name="create">Function to create a new instance of <typeparamref name="T"/>.</param>
    /// <param name="initialize">Action to process the instance when it was requested to the pool.</param>
    /// <param name="return">Action to process the instance when returned to the pool.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     The <paramref name="maxSize"/> must be bigger then zero (0).
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     If any function is null.
    /// </exception>
    public ObjectPoolPolicy(int maxSize, Func<T> create, Action<T> initialize, Action<T> @return)
    {
        if (maxSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxSize));

        MaxSize = maxSize;
        Create = create ?? throw new ArgumentNullException(nameof(create));
        Return = @return ?? throw new ArgumentNullException(nameof(@return));
        Initialize = initialize ?? throw new ArgumentNullException(nameof(initialize));
    }

    /// <summary>
    /// The maximum number of objects that the pool can create and control.
    /// </summary>
    public int MaxSize { get; }

    /// <summary>
    /// Function to create a new instance of <typeparamref name="T"/>.
    /// </summary>
    public Func<T> Create { get; }

    /// <summary>
    /// Action to process the instance when it was requested to the pool.
    /// </summary>
    public Action<T> Initialize { get; }

    /// <summary>
    /// Action to process the instance when returned to the pool.
    /// </summary>
    public Action<T> Return { get; }
}