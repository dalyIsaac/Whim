using System;
using System.Linq;
using System.Reflection;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Kernel;
using AutoFixture.Xunit2;
using Xunit;

namespace Whim.TestUtils;

/// <summary>
/// Creates an AutoFixture fixture with NSubstitute support and injects the given arguments, to be
/// used like <c>MemberData</c> for an xunit <c>Theory</c>.
/// </summary>
public abstract class BaseMemberAutoSubstituteDataAttribute : MemberDataAttributeBase
{
	/// <summary>
	/// Creates a customization to be applied to the fixture.
	/// </summary>
	internal Func<ICustomization>? CreateCustomization { get; set; }

	/// <summary>
	/// Creates a new instance of <see cref="BaseMemberAutoSubstituteDataAttribute"/>.
	/// </summary>
	/// <param name="memberName"></param>
	/// <param name="parameters"></param>
	internal BaseMemberAutoSubstituteDataAttribute(string memberName, params object[] parameters)
		: base(memberName, parameters) { }

	/// <inheritdoc/>
	protected override object[] ConvertDataItem(MethodInfo testMethod, object item)
	{
		if (item is not object[] values)
		{
			throw new ArgumentException(
				$"Property {MemberName} on {MemberType.Name} yielded an item that is not an object[]",
				nameof(item)
			);
		}

		IFixture fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
		if (CreateCustomization is not null)
		{
			fixture.Customize(CreateCustomization());
		}

		return [.. values, .. testMethod.GetParameters().Skip(values.Length).Select(p => GetSpecimen(fixture, p))];
	}

	private static object GetSpecimen(IFixture fixture, ParameterInfo parameter)
	{
		IOrderedEnumerable<IParameterCustomizationSource> attributes = parameter
			.GetCustomAttributes()
			.OfType<IParameterCustomizationSource>()
			.OrderBy(x => x is FrozenAttribute);

		foreach (IParameterCustomizationSource? attribute in attributes)
		{
			attribute.GetCustomization(parameter).Customize(fixture);
		}

		return new SpecimenContext(fixture).Resolve(parameter);
	}
}

/// <summary>
/// Creates an AutoFixture fixture with NSubstitute support and injects the given arguments, to be
/// used like <c>MemberData</c> for an xunit <c>Theory</c>.
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="MemberAutoSubstituteDataAttribute"/>.
/// </remarks>
/// <param name="memberName"></param>
/// <param name="parameters"></param>
public class MemberAutoSubstituteDataAttribute(string memberName, params object[] parameters) : BaseMemberAutoSubstituteDataAttribute(memberName, parameters)
{
}

/// <summary>
/// Creates an AutoFixture fixture with NSubstitute support and injects the given arguments, to be
/// used like <c>MemberData</c> for an xunit <c>Theory</c>. An additional customization is applied.
/// </summary>
/// <typeparam name="TCustomization"></typeparam>
public class MemberAutoSubstituteData<TCustomization> : BaseMemberAutoSubstituteDataAttribute
	where TCustomization : ICustomization, new()
{
	/// <summary>
	/// Creates a new instance of <see cref="MemberAutoSubstituteData{TCustomization}"/>.
	/// </summary>
	/// <param name="memberName"></param>
	/// <param name="parameters"></param>
	public MemberAutoSubstituteData(string memberName, params object[] parameters)
		: base(memberName, parameters)
	{
		CreateCustomization = () => new TCustomization();
	}
}
