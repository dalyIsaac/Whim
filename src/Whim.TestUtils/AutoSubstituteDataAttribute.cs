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
/// Creates an AutoFixture fixture with NSubstitute support and applies the given customization.
/// </summary>
/// <typeparam name="TCustomization"></typeparam>
public class AutoSubstituteDataAttribute<TCustomization> : AutoDataAttribute
	where TCustomization : ICustomization, new()
{
	/// <summary>
	/// Creates a new instance of <see cref="AutoSubstituteDataAttribute{TCustomization}"/>.
	/// </summary>
	public AutoSubstituteDataAttribute()
		: base(CreateFixture) { }

	private static IFixture CreateFixture()
	{
		IFixture fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
		fixture.Customize(new TCustomization());
		return fixture;
	}
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

/// <summary>
/// Creates an AutoFixture fixture with NSubstitute support and injects the given arguments, to be
/// used like `InlineData` for an xunit `Theory`, and applies the given customization.
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
