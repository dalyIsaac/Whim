namespace Whim;

/// <summary>
/// A custom WhimResult type that matches the interface of DotNext's WhimResult but uses WhimError for error handling.
/// This provides a more functional approach to error handling without exceptions.
/// </summary>
/// <typeparam name="T">The type of the value contained in the result.</typeparam>
public readonly struct WhimResult<T>
{
	private readonly T? _value;
	private readonly WhimError? _error;

	/// <summary>
	/// Gets a value indicating whether the result represents a successful operation.
	/// </summary>
	public bool IsSuccessful { get; }

	/// <summary>
	/// Gets the value if the operation was successful, otherwise returns the default value for T.
	/// </summary>
	public T? ValueOrDefault => IsSuccessful ? _value : default;

	/// <summary>
	/// Gets the value if the operation was successful, otherwise throws an exception.
	/// </summary>
	/// <exception cref="InvalidOperationException">Thrown when the result represents a failure.</exception>
	public T Value
	{
		get
		{
			if (!IsSuccessful)
			{
				throw new InvalidOperationException(
					$"Cannot access value of failed result: {_error?.Message ?? "Unknown error"}"
				);
			}
			return _value!;
		}
	}

	/// <summary>
	/// Gets the error if the operation failed, otherwise returns null.
	/// </summary>
	public WhimError? Error => IsSuccessful ? null : _error;

	/// <summary>
	/// Creates a new WhimResult instance representing a successful operation with no value.
	/// </summary>
	public WhimResult()
	{
		_value = default;
		_error = null;
		IsSuccessful = true;
	}

	/// <summary>
	/// Creates a successful result with the specified value.
	/// </summary>
	/// <param name="value">The value to wrap in the result.</param>
	public WhimResult(T value)
	{
		_value = value;
		_error = null;
		IsSuccessful = true;
	}

	/// <summary>
	/// Creates a failed result with the specified error.
	/// </summary>
	/// <param name="error">The error to wrap in the result.</param>
	public WhimResult(WhimError error)
	{
		_value = default;
		_error = error;
		IsSuccessful = false;
	}

	/// <summary>
	/// Attempts to get the value from the result.
	/// </summary>
	/// <param name="value">When this method returns, contains the value if the operation was successful; otherwise, the default value for T.</param>
	/// <returns>true if the operation was successful; otherwise, false.</returns>
	public bool TryGet(out T value)
	{
		if (IsSuccessful)
		{
			value = _value!;
			return true;
		}
		else
		{
			value = default!;
			return false;
		}
	}

	/// <summary>
	/// If the result is successful, returns the value; otherwise, invokes the specified function.
	/// </summary>
	/// <param name="func">The function to invoke if the result is not successful.</param>
	/// <returns>The value if the result is successful; otherwise, the result of invoking the specified function.</returns>
	public T OrInvoke(Func<T> func)
	{
		return IsSuccessful ? _value! : func();
	}

	/// <summary>
	/// Implicitly converts a value to a successful result.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	public static implicit operator WhimResult<T>(T value) => new(value);

	/// <summary>
	/// Implicitly converts an error to a failed result.
	/// </summary>
	/// <param name="error">The error to convert.</param>
	public static implicit operator WhimResult<T>(WhimError error) => new(error);
}

/// <summary>
/// Provides static methods for creating WhimResult instances.
/// </summary>
public static class WhimResult
{
	/// <summary>
	/// Creates a successful result from a value.
	/// </summary>
	/// <typeparam name="T">The type of the value.</typeparam>
	/// <param name="value">The value to wrap in the result.</param>
	/// <returns>A successful result containing the value.</returns>
	public static WhimResult<T> FromValue<T>(T value) => new(value);

	/// <summary>
	/// Creates a failed result from an error.
	/// </summary>
	/// <typeparam name="T">The type of the value.</typeparam>
	/// <param name="error">The error to wrap in the result.</param>
	/// <returns>A failed result containing the error.</returns>
	public static WhimResult<T> FromError<T>(WhimError error) => new(error);

	/// <summary>
	/// Creates a failed result from an exception (for backward compatibility).
	/// </summary>
	/// <typeparam name="T">The type of the value.</typeparam>
	/// <param name="exception">The exception to convert to an error.</param>
	/// <returns>A failed result containing the error.</returns>
	public static WhimResult<T> FromException<T>(Exception exception)
	{
		WhimError error = new(exception.Message, exception);
		return new WhimResult<T>(error);
	}
}
