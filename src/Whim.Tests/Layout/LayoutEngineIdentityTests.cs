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
	public void Equals_Operator_IsTrue()
	{
		// Given
		LayoutEngineIdentity identity = new();

		// When
#pragma warning disable CS1718 // Comparison made to same variable
		bool equals = identity == identity;
#pragma warning restore CS1718 // Comparison made to same variable

		// Then
		Assert.True(equals);
	}

	[Fact]
	public void NotEquals_Operator_Success()
	{
		// Given
		LayoutEngineIdentity identity = new();

		// When
		bool equals = identity != new LayoutEngineIdentity();

		// Then
		Assert.True(equals);
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

	[Fact]
	public void GetHashCode_IsNotEqual()
	{
		// Given
		LayoutEngineIdentity identity = new();
		LayoutEngineIdentity identity2 = new();

		// When
		int hashCode = identity.GetHashCode();

		// Then
		Assert.NotEqual(identity2.GetHashCode(), hashCode);
	}
}
