using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
[ExcludeFromCodeCoverage]
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
	public override IEnumerable<object[]>? GetData(MethodInfo testMethod)
	{
		ArgumentNullException.ThrowIfNull(testMethod);

		Type? type =
			(MemberType ?? testMethod.DeclaringType)
			?? throw new ArgumentException(
				string.Format(
					CultureInfo.CurrentCulture,
					"Could not determine member type for member '{0}'",
					MemberName
				)
			);

		Func<object?> accessor =
			(GetPropertyAccessor(type) ?? GetFieldAccessor(type) ?? GetMethodAccessor(type))
			?? throw new ArgumentException(
				string.Format(
					CultureInfo.CurrentCulture,
					"Could not find public static member (property, field, or method) named '{0}' on {1}{2}",
					MemberName,
					type.FullName,
					Parameters?.Length > 0
						? string.Format(
							CultureInfo.CurrentCulture,
							" with parameter types: {0}",
							string.Join(", ", Parameters.Select(p => p?.GetType().FullName ?? "(null)"))
						)
						: ""
				)
			);

		object? obj = accessor();
		IEnumerable<object[]>? accessedEnumerable = null;

		if (obj == null)
		{
			return null;
		}
		else if (obj is IEnumerable<object[]> arrayEnumerable)
		{
			accessedEnumerable = arrayEnumerable;
		}
		else if (obj is IEnumerable enumerable)
		{
			accessedEnumerable = (IEnumerable<object[]>?)enumerable.Cast<object>();
		}
		else
		{
			throw new ArgumentException(
				string.Format(
					CultureInfo.CurrentCulture,
					"Property {0} on {1} did not return IEnumerable",
					MemberName,
					type.FullName
				)
			);
		}

		return accessedEnumerable?.Select(item => ConvertDataItem(testMethod, item));
	}

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

	// Copied from https://github.com/xunit/xunit/blob/82543a6df6f5f13b5b70f8a9f9ccb41cd676084f/src/xunit.core/MemberDataAttributeBase.cs
	private Func<object?>? GetFieldAccessor(Type type)
	{
		FieldInfo? fieldInfo = null;
		for (
			Type? reflectionType = type;
			reflectionType != null;
			reflectionType = reflectionType.GetTypeInfo().BaseType
		)
		{
			fieldInfo = reflectionType.GetRuntimeField(MemberName);
			if (fieldInfo != null)
			{
				break;
			}
		}

		if (fieldInfo == null || !fieldInfo.IsStatic)
		{
			return null;
		}

		return () => fieldInfo.GetValue(null);
	}

	private Func<object?>? GetMethodAccessor(Type type)
	{
		MethodInfo? methodInfo = null;
		Type?[] parameterTypes = Parameters == null ? [] : Parameters.Select(p => p?.GetType()).ToArray();
		for (
			Type? reflectionType = type;
			reflectionType != null;
			reflectionType = reflectionType.GetTypeInfo().BaseType
		)
		{
			MethodInfo[] runtimeMethodsWithGivenName = reflectionType
				.GetRuntimeMethods()
				.Where(m => m.Name == MemberName)
				.ToArray();
			methodInfo = runtimeMethodsWithGivenName.FirstOrDefault(m =>
				ParameterTypesCompatible(m.GetParameters(), parameterTypes)
			);

			if (methodInfo != null)
			{
				break;
			}

			if (runtimeMethodsWithGivenName.Any(m => m.GetParameters().Any(p => p.IsOptional)))
			{
				throw new ArgumentException(
					string.Format(
						CultureInfo.CurrentCulture,
						"Method '{0}.{1}' contains optional parameters, which are not currently supported. Please use overloads if necessary.",
						type.FullName,
						MemberName
					)
				);
			}
		}

		if (methodInfo == null || !methodInfo.IsStatic)
		{
			return null;
		}

		return () => methodInfo.Invoke(null, Parameters);
	}

	private Func<object?>? GetPropertyAccessor(Type type)
	{
		PropertyInfo? propInfo = null;
		for (
			Type? reflectionType = type;
			reflectionType != null;
			reflectionType = reflectionType.GetTypeInfo().BaseType
		)
		{
			propInfo = reflectionType.GetRuntimeProperty(MemberName);
			if (propInfo != null)
			{
				break;
			}
		}

		if (propInfo == null || propInfo.GetMethod == null || !propInfo.GetMethod.IsStatic)
		{
			return null;
		}

		return () => propInfo.GetValue(null, null);
	}

	private static bool ParameterTypesCompatible(ParameterInfo[] parameters, Type?[] parameterTypes)
	{
		if (parameters?.Length != parameterTypes.Length)
		{
			return false;
		}

		for (int idx = 0; idx < parameters.Length; ++idx)
		{
			Type? currentType = parameterTypes[idx];

			if (
				currentType != null
				&& parameterTypes[idx] != null
				&& !parameters[idx].ParameterType.GetTypeInfo().IsAssignableFrom(currentType.GetTypeInfo())
			)
			{
				return false;
			}
		}

		return true;
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
public class MemberAutoSubstituteDataAttribute(string memberName, params object[] parameters)
	: BaseMemberAutoSubstituteDataAttribute(memberName, parameters) { }

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
