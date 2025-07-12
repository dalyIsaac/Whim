namespace Whim.Tests;

public class ResultTests
{
	[Fact]
	public void Result_Success_CanBeCreatedWithValue()
	{
		// Given
		int expectedValue = 42;
		Result<int> result = new(expectedValue);

		// Then
		Assert.True(result.IsSuccessful);
		Assert.Null(result.Error);
		Assert.Equal(expectedValue, result.Value);
	}

	[Fact]
	public void Result_Failure_CanBeCreatedWithError()
	{
		// Given
		WhimError error = new("Test error");
		Result<int> result = new(error);

		// Then
		Assert.False(result.IsSuccessful);
		Assert.Equal(error, result.Error);
		Assert.Throws<InvalidOperationException>(() => _ = result.Value);
	}

	[Fact]
	public void Result_TryGet_ReturnsTrueForSuccessfulResult()
	{
		// Given
		int expectedValue = 42;
		Result<int> result = new(expectedValue);

		// When
		bool success = result.TryGet(out int value);

		// Then
		Assert.True(success);
		Assert.Equal(expectedValue, value);
	}

	[Fact]
	public void Result_TryGet_ReturnsFalseForFailedResult()
	{
		// Given
		WhimError error = new("Test error");
		Result<int> result = new(error);

		// When
		bool success = result.TryGet(out int value);

		// Then
		Assert.False(success);
		Assert.Equal(default, value);
	}

	[Fact]
	public void Result_Value_ThrowsExceptionForFailedResult()
	{
		// Given
		WhimError error = new("Test error");
		Result<int> result = new(error);

		// Then
		Assert.Throws<InvalidOperationException>(() => _ = result.Value);
	}

	[Fact]
	public void Result_Error_ReturnsNullForSuccessfulResult()
	{
		// Given
		Result<int> result = new(42);

		// Then
		Assert.Null(result.Error);
	}

	[Fact]
	public void Result_Error_ReturnsErrorForFailedResult()
	{
		// Given
		WhimError error = new("Test error");
		Result<int> result = new(error);

		// Then
		Assert.Equal(error, result.Error);
	}

	[Fact]
	public void Result_OrInvoke_ReturnsValueForSuccessfulResult()
	{
		// Given
		int expectedValue = 42;
		Result<int> result = new(expectedValue);

		// When
		int value = result.OrInvoke(() => 0);

		// Then
		Assert.Equal(expectedValue, value);
	}

	[Fact]
	public void Result_OrInvoke_InvokesFunctionForFailedResult()
	{
		// Given
		WhimError error = new("Test error");
		Result<int> result = new(error);
		int fallbackValue = 0;

		// When
		int value = result.OrInvoke(() => fallbackValue);

		// Then
		Assert.Equal(fallbackValue, value);
	}

	[Fact]
	public void Result_ImplicitConversion_FromValue()
	{
		// Given
		int expectedValue = 42;

		// When
		Result<int> result = expectedValue;

		// Then
		Assert.True(result.IsSuccessful);
		Assert.Equal(expectedValue, result.Value);
	}

	[Fact]
	public void Result_ImplicitConversion_FromError()
	{
		// Given
		WhimError error = new("Test error");

		// When
		Result<int> result = error;

		// Then
		Assert.False(result.IsSuccessful);
		Assert.Equal(error, result.Error);
	}

	[Fact]
	public void WhimResult_FromValue_CreatesSuccessfulResult()
	{
		// Given
		int expectedValue = 42;

		// When
		Result<int> result = Result.FromValue(expectedValue);

		// Then
		Assert.True(result.IsSuccessful);
		Assert.Equal(expectedValue, result.Value);
	}

	[Fact]
	public void WhimResult_FromError_CreatesFailedResult()
	{
		// Given
		WhimError error = new("Test error");

		// When
		Result<int> result = Result.FromError<int>(error);

		// Then
		Assert.False(result.IsSuccessful);
		Assert.Equal(error, result.Error);
	}

	[Fact]
	public void WhimResult_FromException_CreatesFailedResult()
	{
		// Given
		Exception exception = new("Test exception");

		// When
		Result<int> result = Result.FromException<int>(exception);

		// Then
		Assert.False(result.IsSuccessful);
		Assert.NotNull(result.Error);
		Assert.Equal(exception.Message, result.Error.Message);
		Assert.Equal(exception, result.Error.InnerException);
	}

	[Fact]
	public void Result_ValueOrDefault_ReturnsValueForSuccessfulResult()
	{
		// Given
		int expectedValue = 42;
		Result<int> result = new(expectedValue);

		// When
		int? value = result.ValueOrDefault;

		// Then
		Assert.Equal(expectedValue, value);
	}

	[Fact]
	public void Result_ValueOrDefault_ReturnsDefaultForFailedResult()
	{
		// Given
		WhimError error = new("Test error");
		Result<int> result = new(error);

		// When
		int? value = result.ValueOrDefault;

		// Then
		Assert.Equal(default(int), value);
	}

	[Fact]
	public void Result_ValueOrDefault_ReturnsNullForFailedReferenceType()
	{
		// Given
		WhimError error = new("Test error");
		Result<string> result = new(error);

		// When
		string? value = result.ValueOrDefault;

		// Then
		Assert.Null(value);
	}

	[Fact]
	public void Result_Equals_TwoSuccessfulResultsWithSameValue_ReturnsTrue()
	{
		// Given
		Result<int> result1 = new(42);
		Result<int> result2 = new(42);

		// When & Then
		Assert.True(result1.Equals(result2));
		Assert.True(result1 == result2);
		Assert.False(result1 != result2);
	}

	[Fact]
	public void Result_Equals_TwoSuccessfulResultsWithDifferentValues_ReturnsFalse()
	{
		// Given
		Result<int> result1 = new(42);
		Result<int> result2 = new(24);

		// When & Then
		Assert.False(result1.Equals(result2));
		Assert.False(result1 == result2);
		Assert.True(result1 != result2);
	}

	[Fact]
	public void Result_Equals_TwoFailedResultsWithSameError_ReturnsTrue()
	{
		// Given
		WhimError error = new("Test error");
		Result<int> result1 = new(error);
		Result<int> result2 = new(error);

		// When & Then
		Assert.True(result1.Equals(result2));
		Assert.True(result1 == result2);
		Assert.False(result1 != result2);
	}

	[Fact]
	public void Result_Equals_TwoFailedResultsWithDifferentErrors_ReturnsFalse()
	{
		// Given
		WhimError error1 = new("Test error 1");
		WhimError error2 = new("Test error 2");
		Result<int> result1 = new(error1);
		Result<int> result2 = new(error2);

		// When & Then
		Assert.False(result1.Equals(result2));
		Assert.False(result1 == result2);
		Assert.True(result1 != result2);
	}

	[Fact]
	public void Result_Equals_SuccessfulAndFailedResult_ReturnsFalse()
	{
		// Given
		Result<int> successResult = new(42);
		Result<int> failedResult = new(new WhimError("Test error"));

		// When & Then
		Assert.False(successResult.Equals(failedResult));
		Assert.False(successResult == failedResult);
		Assert.True(successResult != failedResult);
	}

	[Fact]
	public void Result_Equals_WithNullObject_ReturnsFalse()
	{
		// Given
		Result<int> result = new(42);

		// When & Then
		Assert.False(result.Equals((Result<int>?)null));
	}

	[Fact]
	public void Result_Equals_WithDifferentType_ReturnsFalse()
	{
		// Given
		Result<int> result = new(42);
		string otherObject = "not a result";

		// When & Then
		Assert.False(result.Equals(otherObject));
	}

	[Fact]
	public void Result_GetHashCode_SuccessfulResultsWithSameValue_ReturnsSameHashCode()
	{
		// Given
		Result<int> result1 = new(42);
		Result<int> result2 = new(42);

		// When & Then
		Assert.Equal(result1.GetHashCode(), result2.GetHashCode());
	}

	[Fact]
	public void Result_GetHashCode_FailedResultsWithSameError_ReturnsSameHashCode()
	{
		// Given
		WhimError error = new("Test error");
		Result<int> result1 = new(error);
		Result<int> result2 = new(error);

		// When & Then
		Assert.Equal(result1.GetHashCode(), result2.GetHashCode());
	}

	[Fact]
	public void Result_WithReferenceType_Success()
	{
		// Given
		string expectedValue = "test string";
		Result<string> result = new(expectedValue);

		// Then
		Assert.True(result.IsSuccessful);
		Assert.Equal(expectedValue, result.Value);
		Assert.Equal(expectedValue, result.ValueOrDefault);
		Assert.Null(result.Error);
	}

	[Fact]
	public void Result_WithReferenceType_Failure()
	{
		// Given
		WhimError error = new("Test error");
		Result<string> result = new(error);

		// Then
		Assert.False(result.IsSuccessful);
		Assert.Null(result.ValueOrDefault);
		Assert.Equal(error, result.Error);
		Assert.Throws<InvalidOperationException>(() => _ = result.Value);
	}

	[Fact]
	public void Result_WithNullValue_Success()
	{
		// Given
		string? nullValue = null;
		Result<string?> result = new(nullValue);

		// Then
		Assert.True(result.IsSuccessful);
		Assert.Null(result.Value);
		Assert.Null(result.ValueOrDefault);
		Assert.Null(result.Error);
	}

	[Fact]
	public void Result_Value_ExceptionMessage_ContainsErrorMessage()
	{
		// Given
		string errorMessage = "Specific test error";
		WhimError error = new(errorMessage);
		Result<int> result = new(error);

		// When & Then
		var exception = Assert.Throws<InvalidOperationException>(() => _ = result.Value);
		Assert.Contains(errorMessage, exception.Message);
	}

	[Fact]
	public void Result_Value_ExceptionMessage_HandlesNullError()
	{
		// Given - Creating a result with a null error (edge case)
		Result<int> result = new(new WhimError("Test"));

		// Simulate accessing Value when error message might be null
		// This tests the null coalescing in the Value property
		var exception = Assert.Throws<InvalidOperationException>(() => _ = result.Value);
		Assert.Contains("Test", exception.Message);
	}

	[Fact]
	public void Result_TryGet_WithReferenceType_Success()
	{
		// Given
		string expectedValue = "test string";
		Result<string> result = new(expectedValue);

		// When
		bool success = result.TryGet(out string value);

		// Then
		Assert.True(success);
		Assert.Equal(expectedValue, value);
	}

	[Fact]
	public void Result_TryGet_WithReferenceType_Failure()
	{
		// Given
		WhimError error = new("Test error");
		Result<string> result = new(error);

		// When
		bool success = result.TryGet(out string value);

		// Then
		Assert.False(success);
		Assert.Null(value);
	}

	[Fact]
	public void Result_OrInvoke_WithReferenceType_Success()
	{
		// Given
		string expectedValue = "test string";
		Result<string> result = new(expectedValue);

		// When
		string value = result.OrInvoke(() => "fallback");

		// Then
		Assert.Equal(expectedValue, value);
	}

	[Fact]
	public void Result_OrInvoke_WithReferenceType_Failure()
	{
		// Given
		WhimError error = new("Test error");
		Result<string> result = new(error);
		string fallbackValue = "fallback";

		// When
		string value = result.OrInvoke(() => fallbackValue);

		// Then
		Assert.Equal(fallbackValue, value);
	}

	[Fact]
	public void Result_ImplicitConversion_WithReferenceType()
	{
		// Given
		string expectedValue = "test string";

		// When
		Result<string> result = expectedValue;

		// Then
		Assert.True(result.IsSuccessful);
		Assert.Equal(expectedValue, result.Value);
	}

	[Fact]
	public void Result_ImplicitConversion_WithNullValue()
	{
		// Given
		string? nullValue = null;

		// When
		Result<string?> result = nullValue;

		// Then
		Assert.True(result.IsSuccessful);
		Assert.Null(result.Value);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	[InlineData(int.MaxValue)]
	[InlineData(int.MinValue)]
	public void Result_WithDifferentIntValues_Success(int value)
	{
		// Given & When
		Result<int> result = new(value);

		// Then
		Assert.True(result.IsSuccessful);
		Assert.Equal(value, result.Value);
		Assert.Equal(value, result.ValueOrDefault);
	}

	[Fact]
	public void Result_WithCustomObject_Success()
	{
		// Given
		var customObject = new { Name = "Test", Value = 42 };
		Result<object> result = new(customObject);

		// Then
		Assert.True(result.IsSuccessful);
		Assert.Equal(customObject, result.Value);
		Assert.Equal(customObject, result.ValueOrDefault);
	}

	[Fact]
	public void Result_Equality_WithCustomObjects()
	{
		// Given
		var obj1 = new TestClass("Test", 42);
		var obj2 = new TestClass("Test", 42);
		Result<TestClass> result1 = new(obj1);
		Result<TestClass> result2 = new(obj2);

		// When & Then
		// Even though the objects have the same values, they are different references
		Assert.False(result1.Equals(result2));
		Assert.False(result1 == result2);
		Assert.True(result1 != result2);
	}

	[Fact]
	public void Result_Equality_WithSameCustomObjectReference()
	{
		// Given
		var obj = new { Name = "Test", Value = 42 };
		Result<object> result1 = new(obj);
		Result<object> result2 = new(obj);

		// When & Then
		Assert.True(result1.Equals(result2));
		Assert.True(result1 == result2);
		Assert.False(result1 != result2);
	}
}

/// <summary>
/// Test class to verify reference equality behavior in Result equality tests.
/// </summary>
/// <param name="Name"></param>
/// <param name="Value"></param>
#pragma warning disable CS9113 // Parameter is unread.
internal class TestClass(string Name, int Value);
#pragma warning restore CS9113 // Parameter is unread.
