using AutoFixture;
using AutoFixture.Xunit2;

namespace Whim.TestUtils;

/// <summary>
/// Creates an AutoFixture fixture with NSubstitute support and injects the given arguments, to be
/// used like <c>InlineData</c> for an xunit <c>Theory</c>.
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

/// <summary>
/// Creates an AutoFixture fixture with NSubstitute support and injects the given arguments, to be
/// used like <c>InlineData</c> for an xunit <c>Theory</c>. An additional customization is applied.
/// </summary>
/// <typeparam name="TCustomization"></typeparam>
public class InlineAutoSubstituteDataAttribute<TCustomization> : InlineAutoDataAttribute
	where TCustomization : ICustomization, new()
{
	/// <summary>
	/// Creates a new instance of <see cref="InlineAutoSubstituteDataAttribute{TCustomization}"/>.
	/// </summary>
	/// <param name="arguments"></param>
	public InlineAutoSubstituteDataAttribute(params object[] arguments)
		: base(new AutoSubstituteDataAttribute<TCustomization>(), arguments) { }
}
