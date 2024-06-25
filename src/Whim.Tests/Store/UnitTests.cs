namespace Whim.Tests;

public class UnitTests
{
	[Fact]
	public void UnitResult()
	{
		// Given
		Result<Unit> result = Unit.Result;

		// Then
		Assert.True(result.IsSuccessful);
		Assert.Equal(Unit.Result, result);
	}
}
