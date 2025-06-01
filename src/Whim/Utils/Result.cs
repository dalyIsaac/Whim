using System.Diagnostics.CodeAnalysis;

namespace Whim;

/// <summary>
/// <c>Result</c> is similar to Rust's <c>Result&lt;T, E&gt;</c> type, and DotNext's <c>Result&lt;T, Exception&gt;</c>.
/// It represents the outcome of an operation that can either succeed with a value of type T or fail with an error.
/// This type is used to encapsulate the result of operations in a way that allows for error handling without exceptions.
/// It provides methods to check success, retrieve values, and handle errors.
/// </summary>
/// <typeparam name="T">The type of the value contained in the result.</typeparam>
public readonly struct Result<T> : IEquatable<Result<T>>
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
	public T? Value
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
	/// Creates a successful result with the specified value.
	/// </summary>
	/// <param name="value">The value to wrap in the result.</param>
	public Result(T value)
	{
		_value = value;
		_error = null;
		IsSuccessful = true;
	}

	/// <summary>
	/// Creates a failed result with the specified error.
	/// </summary>
	/// <param name="error">The error to wrap in the result.</param>
	public Result(WhimError error)
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
	[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates")]
	public static implicit operator Result<T>(T value) => new(value);

	/// <summary>
	/// Implicitly converts an error to a failed result.
	/// </summary>
	/// <param name="error">The error to convert.</param>
	[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates")]
	public static implicit operator Result<T>(WhimError error) => new(error);

	/// <summary>
	/// Determines whether the specified object is equal to the current result.
	/// </summary>
	/// <param name="obj">The object to compare with the current result.</param>
	/// <returns>true if the specified object is equal to the current result; otherwise, false.</returns>
	public override bool Equals(object? obj)
	{
		if (obj is Result<T> other)
		{
			return Equals(other);
		}
		return false;
	}

	/// <summary>
	/// Determines whether the current result is equal to another result of the same type.
	/// </summary>
	/// <param name="other">The result to compare with the current result.</param>
	public bool Equals(Result<T> other)
	{
		if (IsSuccessful != other.IsSuccessful)
		{
			return false;
		}

		if (IsSuccessful)
		{
			return EqualityComparer<T>.Default.Equals(_value!, other._value!);
		}

		return EqualityComparer<WhimError>.Default.Equals(_error, other._error);
	}

	/// <summary>
	/// Returns the hash code for this result.
	/// </summary>
	/// <returns>A hash code for the current result.</returns>
	public override int GetHashCode()
	{
		if (IsSuccessful)
		{
			return HashCode.Combine(IsSuccessful, _value);
		}
		else
		{
			return HashCode.Combine(IsSuccessful, _error);
		}
	}

	/// <summary>
	/// Determines whether two specified results are equal.
	/// </summary>
	/// <param name="left">The first result to compare.</param>
	/// <param name="right">The second result to compare.</param>
	/// <returns>true if the results are equal; otherwise, false.</returns>
	public static bool operator ==(Result<T> left, Result<T> right)
	{
		return left.Equals(right);
	}

	/// <summary>
	/// Determines whether two specified results are not equal.
	/// </summary>
	/// <param name="left">The first result to compare.</param>
	/// <param name="right">The second result to compare.</param>
	/// <returns>true if the results are not equal; otherwise, false.</returns>
	public static bool operator !=(Result<T> left, Result<T> right)
	{
		return !(left == right);
	}
}

/// <summary>
/// Provides static methods for creating Result instances.
/// </summary>
public static class Result
{
	/// <summary>
	/// Creates a successful result from a value.
	/// </summary>
	/// <typeparam name="T">The type of the value.</typeparam>
	/// <param name="value">The value to wrap in the result.</param>
	/// <returns>A successful result containing the value.</returns>
	public static Result<T> FromValue<T>(T value) => new(value);

	/// <summary>
	/// Creates a failed result from an error.
	/// </summary>
	/// <typeparam name="T">The type of the value.</typeparam>
	/// <param name="error">The error to wrap in the result.</param>
	/// <returns>A failed result containing the error.</returns>
	public static Result<T> FromError<T>(WhimError error) => new(error);

	/// <summary>
	/// Creates a failed result from an exception (for backward compatibility).
	/// </summary>
	/// <typeparam name="T">The type of the value.</typeparam>
	/// <param name="exception">The exception to convert to an error.</param>
	/// <returns>A failed result containing the error.</returns>
	public static Result<T> FromException<T>(Exception exception)
	{
		WhimError error = new(exception.Message, exception);
		return new Result<T>(error);
	}
}
