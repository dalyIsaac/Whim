using AutoFixture.AutoNSubstitute;
using AutoFixture;
using AutoFixture.Xunit2;

namespace Whim.TestUtils;

/// <summary>
/// Creates an AutoFixture fixture with NSubstitute support.
/// </summary>
public class AutoSubstituteDataAttribute : AutoDataAttribute
{
	/// <summary>
	/// Creates a new instance of <see cref="AutoSubstituteDataAttribute"/>.
	/// </summary>
	public AutoSubstituteDataAttribute()
		: base(() => new Fixture().Customize(new AutoNSubstituteCustomization())) { }
}

/// <summary>
/// Creates an AutoFixture fixture with NSubstitute support and injects the given arguments, to be
/// used like `InlineData` for an xunit `Theory`.
/// </summary>
public class InlineAutoSubstituteDataAttribute : InlineAutoDataAttribute
{
	/// <summary>
	/// Creates a new instance of <see cref="InlineAutoSubstituteDataAttribute"/>.
	/// </summary>
	/// <param name="arguments"></param>
	public InlineAutoSubstituteDataAttribute(params object[] arguments)
		: base(new AutoSubstituteDataAttribute(), arguments) { }
}
