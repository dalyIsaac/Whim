using Xunit;

namespace Whim.Tests;

public class LayoutEngineIdentityTests
{
	[Fact]
	public void Equals_IsTrue()
	{
		// Given
		LayoutEngineIdentity identity = new();

		// When
		bool equals = identity.Equals(identity);

		// Then
		Assert.True(equals);
	}

	[Fact]
	public void Equals_IsFalse()
	{
		// Given
		LayoutEngineIdentity identity = new();

		// When
		bool equals = identity.Equals(new());

		// Then
		Assert.False(equals);
	}

	[Fact]
	public void Equals_WrongType()
	{
		// Given
		LayoutEngineIdentity identity = new();

		// When
		bool equals = identity.Equals(new object());

		// Then
		Assert.False(equals);
	}

	[Fact]
	public void GetHashCode_IsEqual()
	{
		// Given
		LayoutEngineIdentity identity = new();

		// When
		int hashCode = identity.GetHashCode();

		// Then
		Assert.Equal(identity.GetHashCode(), hashCode);
	}
}
