namespace Whim.Tests;

public class ResultTests
{
	[Fact]
	public void Result_Success_CanBeCreatedWithoutValue()
	{
		// Given
		WhimResult<int> result = new();

		// Then
		Assert.True(result.IsSuccessful);
		Assert.Null(result.Error);
		Assert.Equal(default, result.Value);
	}

	[Fact]
	public void Result_Success_CanBeCreatedWithValue()
	{
		// Given
		int expectedValue = 42;
		WhimResult<int> result = new(expectedValue);

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
		WhimResult<int> result = new(error);

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
		WhimResult<int> result = new(expectedValue);

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
		WhimResult<int> result = new(error);

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
		WhimResult<int> result = new(error);

		// Then
		Assert.Throws<InvalidOperationException>(() => _ = result.Value);
	}

	[Fact]
	public void Result_Error_ReturnsNullForSuccessfulResult()
	{
		// Given
		WhimResult<int> result = new(42);

		// Then
		Assert.Null(result.Error);
	}

	[Fact]
	public void Result_Error_ReturnsErrorForFailedResult()
	{
		// Given
		WhimError error = new("Test error");
		WhimResult<int> result = new(error);

		// Then
		Assert.Equal(error, result.Error);
	}
}
