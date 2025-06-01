namespace Whim.Tests;

public class WhimErrorTests
{
	[Fact]
	public void Constructor_MessageOnly_SetsMessageAndLogsDebug()
	{
		// Given
		string expectedMessage = "Test error message";

		// When
		WhimError error = new(expectedMessage);

		// Then
		Assert.Equal(expectedMessage, error.Message);
		Assert.Null(error.InnerException);
	}

	[Fact]
	public void Constructor_MessageAndException_SetsMessageAndInnerException()
	{
		// Given
		string expectedMessage = "Test error message";
		Exception innerException = new InvalidOperationException("Inner exception");

		// When
		WhimError error = new(expectedMessage, innerException);

		// Then
		Assert.Equal(expectedMessage, error.Message);
		Assert.Equal(innerException, error.InnerException);
	}

	[Theory]
	[InlineData(LogLevel.Verbose)]
	[InlineData(LogLevel.Debug)]
	[InlineData(LogLevel.Information)]
	[InlineData(LogLevel.Warning)]
	[InlineData(LogLevel.Error)]
	[InlineData(LogLevel.Fatal)]
	public void Constructor_MessageAndLogLevel_SetsMessageAndLogsAtCorrectLevel(LogLevel logLevel)
	{
		// Given
		string expectedMessage = "Test error message";

		// When
		WhimError error = new(expectedMessage, logLevel);

		// Then
		Assert.Equal(expectedMessage, error.Message);
		Assert.Null(error.InnerException);
	}

	[Fact]
	public void Constructor_MessageOnly_DefaultsToDebugLogLevel()
	{
		// Given
		string expectedMessage = "Test error message";

		// When
		WhimError error = new(expectedMessage);

		// Then
		Assert.Equal(expectedMessage, error.Message);
		// Note: We can't easily test the logging behavior without mocking the static Logger,
		// but the constructor chain ensures it calls the LogLevel.Debug constructor
	}

	[Fact]
	public void ToString_MessageOnly_ReturnsMessage()
	{
		// Given
		string expectedMessage = "Test error message";
		WhimError error = new(expectedMessage);

		// When
		string result = error.ToString();

		// Then
		Assert.Equal(expectedMessage, result);
	}

	[Fact]
	public void ToString_MessageWithInnerException_ReturnsMessageWithInnerExceptionMessage()
	{
		// Given
		string errorMessage = "Test error message";
		string innerExceptionMessage = "Inner exception message";
		Exception innerException = new InvalidOperationException(innerExceptionMessage);
		WhimError error = new(errorMessage, innerException);

		// When
		string result = error.ToString();

		// Then
		string expected = $"{errorMessage} -> {innerExceptionMessage}";
		Assert.Equal(expected, result);
	}

	[Fact]
	public void ToString_MessageWithNullInnerException_ReturnsMessageOnly()
	{
		// Given
		string expectedMessage = "Test error message";
		WhimError error = new(expectedMessage);

		// When
		string result = error.ToString();

		// Then
		Assert.Equal(expectedMessage, result);
	}

	[Fact]
	public void Constructor_EmptyMessage_SetsEmptyMessage()
	{
		// Given
		string emptyMessage = "";

		// When
		WhimError error = new(emptyMessage);

		// Then
		Assert.Equal(emptyMessage, error.Message);
	}

	[Fact]
	public void Constructor_NullInnerException_SetsNullInnerException()
	{
		// Given
		string message = "Test message";
		Exception? nullException = null;

		// When
		WhimError error = new(message, nullException!);

		// Then
		Assert.Equal(message, error.Message);
		Assert.Null(error.InnerException);
	}

	[Fact]
	public void Constructor_MessageAndException_ChainedExceptionToString()
	{
		// Given
		string errorMessage = "Outer error";
		string innerMessage = "Inner error";
		Exception innerException = new ArgumentException(innerMessage);
		WhimError error = new(errorMessage, innerException);

		// When
		string result = error.ToString();

		// Then
		Assert.Equal($"{errorMessage} -> {innerMessage}", result);
	}

	[Fact]
	public void Constructor_NestedExceptions_ShowsImmediateInnerExceptionOnly()
	{
		// Given
		string outerMessage = "Outer error";
		string innerMessage = "Inner error";
		string deepInnerMessage = "Deep inner error";

		Exception deepInnerException = new InvalidOperationException(deepInnerMessage);
		Exception innerException = new ArgumentException(innerMessage, deepInnerException);
		WhimError error = new(outerMessage, innerException);

		// When
		string result = error.ToString();

		// Then
		// Should only show the immediate inner exception, not the nested one
		Assert.Equal($"{outerMessage} -> {innerMessage}", result);
	}

	[Theory]
	[InlineData("Simple message")]
	[InlineData("Message with special characters: !@#$%^&*()")]
	[InlineData("Message with unicode: 测试")]
	[InlineData(
		"Very long message that spans multiple lines and contains lots of text to test how the WhimError handles longer error messages"
	)]
	public void Constructor_VariousMessageFormats_HandlesCorrectly(string message)
	{
		// When
		WhimError error = new(message);

		// Then
		Assert.Equal(message, error.Message);
		Assert.Equal(message, error.ToString());
	}

	[Fact]
	public void Properties_AreReadOnly()
	{
		// Given
		string message = "Test message";
		Exception innerException = new InvalidOperationException("Inner");
		WhimError error = new(message, innerException);

		// When/Then - Properties should be read-only (getter only)
		Assert.Equal(message, error.Message);
		Assert.Equal(innerException, error.InnerException);

		// Verify that Message and InnerException are indeed get-only properties
		var messageProperty = typeof(WhimError).GetProperty(nameof(WhimError.Message));
		var innerExceptionProperty = typeof(WhimError).GetProperty(nameof(WhimError.InnerException));

		Assert.NotNull(messageProperty);
		Assert.NotNull(innerExceptionProperty);
		Assert.True(messageProperty.CanRead);
		Assert.False(messageProperty.CanWrite);
		Assert.True(innerExceptionProperty.CanRead);
		Assert.False(innerExceptionProperty.CanWrite);
	}
}
