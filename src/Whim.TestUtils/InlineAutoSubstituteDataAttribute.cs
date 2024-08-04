using AutoFixture;
using AutoFixture.Xunit2;

namespace Whim.TestUtils;

/// <summary>
/// Creates an AutoFixture fixture with NSubstitute support and injects the given arguments, to be
/// used like <c>InlineData</c> for an xunit <c>Theory</c>.
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="InlineAutoSubstituteDataAttribute"/>.
/// </remarks>
/// <param name="arguments"></param>
public class InlineAutoSubstituteDataAttribute(params object[] arguments) : InlineAutoDataAttribute(new AutoSubstituteDataAttribute(), arguments)
{
}

/// <summary>
/// Creates an AutoFixture fixture with NSubstitute support and injects the given arguments, to be
/// used like <c>InlineData</c> for an xunit <c>Theory</c>. An additional customization is applied.
/// </summary>
/// <typeparam name="TCustomization"></typeparam>
/// <remarks>
/// Creates a new instance of <see cref="InlineAutoSubstituteDataAttribute{TCustomization}"/>.
/// </remarks>
/// <param name="arguments"></param>
public class InlineAutoSubstituteDataAttribute<TCustomization>(params object[] arguments) : InlineAutoDataAttribute(new AutoSubstituteDataAttribute<TCustomization>(), arguments)
	where TCustomization : ICustomization, new()
{
}
