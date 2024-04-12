using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Whim.SourceGenerators;

internal readonly record struct SliceTransformParam(string Name, string Type, bool Nullable);

internal readonly record struct SliceTransform(string Name, SliceTransformParam[] Parameters)
{
	public string RecordName => Name + "Transform";
}

internal readonly record struct SliceToGenerate(
	string Name,
	string Path,
	string Key,
	string Accessibility,
	List<SliceTransform> Transforms
);

[Generator]
public class SliceGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		// Add the marker attributes to the compilation.
		context.RegisterPostInitializationOutput(ctx =>
			ctx.AddSource("SliceAttribute.g.cs", SourceText.From(SliceGeneratorHelper.Attributes, Encoding.UTF8))
		);

		// Do a filter for classes.
		IncrementalValuesProvider<SliceToGenerate?> sliceToGenerate = context
			.SyntaxProvider.ForAttributeWithMetadataName(
				"Whim.SliceAttribute",
				predicate: static (s, _) => s is ClassDeclarationSyntax,
				transform: static (ctx, _) => GetSliceToGenerate(ctx.SemanticModel, ctx.TargetNode)
			)
			.Where(static m => m is not null);

		// Generate source code for each class found.
		context.RegisterSourceOutput(sliceToGenerate, Execute);
	}

	private static SliceToGenerate? GetSliceToGenerate(SemanticModel semanticModel, SyntaxNode classDeclarationSyntax)
	{
		// Get the semantic representation of the class syntax.
		if (semanticModel.GetDeclaredSymbol(classDeclarationSyntax) is not INamedTypeSymbol classSymbol)
		{
			return null;
		}

		// Get the full type of the class, e.g., WindowSlice.
		if (classSymbol.ToString() is not string className)
		{
			return null;
		}

		ImmutableArray<ISymbol> classMembers = classSymbol.GetMembers();

		// Get the Path and Key of the slice.
		string path = GetSliceAttributeArgument(classSymbol, "Path");
		string key = GetSliceAttributeArgument(classSymbol, "Key");

		// Get all the members in the enum with the Whim.TransformerAttribute.
		ISymbol[] transformerMembers = classMembers
			.Where(m => m.GetAttributes().Any(a => a.Equals("Whim.TransformerAttribute")))
			.ToArray();
		List<SliceTransform> transforms = [];

		// Get all the methods and add them to the list.
		for (int mIdx = 0; mIdx < classMembers.Length; mIdx++)
		{
			if (classMembers[mIdx] is not IMethodSymbol methodSymbol)
			{
				continue;
			}

			ImmutableArray<AttributeData> methodAttributes = methodSymbol.GetAttributes();
			if (methodAttributes.Any(m => m.AttributeClass?.Name == "TransformerAttribute") == false)
			{
				continue;
			}

			SliceTransformParam[] parameters = new SliceTransformParam[methodSymbol.Parameters.Length];
			for (int pIdx = 0; pIdx < methodSymbol.Parameters.Length; pIdx++)
			{
				IParameterSymbol current = methodSymbol.Parameters[pIdx];
				parameters[pIdx] = new SliceTransformParam(
					current.Name,
					current.Type.Name,
					current.NullableAnnotation == NullableAnnotation.Annotated
				);
			}

			transforms.Add(new(methodSymbol.Name, parameters));
		}

		return new(classSymbol.Name, path, key, classSymbol.DeclaredAccessibility.ToString().ToLower(), transforms);
	}

	private static string GetSliceAttributeArgument(INamedTypeSymbol symbol, string fieldName)
	{
		foreach (AttributeData attribute in symbol.GetAttributes())
		{
			if (attribute.AttributeClass?.Name != "SliceAttribute")
			{
				continue;
			}

			foreach (KeyValuePair<string, TypedConstant> arg in attribute.NamedArguments)
			{
				if (arg.Key != fieldName)
				{
					continue;
				}

				string? value = arg.Value.Value?.ToString();
				if (string.IsNullOrEmpty(value))
				{
					throw new Exception($"Field {fieldName} must be defined");
				}

				return value!;
			}
		}

		throw new ArgumentException($"Could not find a valid string field for the name '{fieldName}'");
	}

	private static void Execute(SourceProductionContext context, SliceToGenerate? sliceToGenerate)
	{
		if (sliceToGenerate is not { } value)
		{
			return;
		}

		// Generate the source code and add it to the output.
		string result = SliceGeneratorHelper.GenerateExtensionClass(value);

		// Create a separate partial class file for each class.
		context.AddSource($"{value.Name}.g.cs", SourceText.From(result, Encoding.UTF8));
	}
}

internal static class SliceGeneratorHelper
{
	public const string Attributes = """
		namespace Whim;

		[System.AttributeUsage(System.AttributeTargets.Class)]
		public sealed class SliceAttribute : System.Attribute
		{
			public string Path { get; set; } = "";
			public string Key { get; set; } = "";
		}

		[System.AttributeUsage(System.AttributeTargets.Method)]
		public sealed class TransformerAttribute : System.Attribute { }

		""";

	public static string GenerateExtensionClass(SliceToGenerate slice)
	{
		string path = slice.Path + slice.Key;

		StringBuilder sb = new();

		sb.Append(
			"""
			using System;

			namespace Whim;

			"""
		);

		CreateTransformsRecords(slice, path, sb);
		CreatePartialClass(slice, sb);

		return sb.ToString();
	}

	/// <summary>
	/// Create a record for each transformer.
	/// </summary>
	/// <param name="slice"></param>
	/// <param name="path"></param>
	/// <param name="sb"></param>
	private static void CreateTransformsRecords(SliceToGenerate slice, string path, StringBuilder sb)
	{
		foreach (SliceTransform transform in slice.Transforms)
		{
			sb.AppendLine().Append("public sealed record ")
				.Append(transform.RecordName)
				.Append('(');

			AppendTransformParams(sb, transform);

			// Add the base transform.
			sb.Append(@") : Transform(""").Append(path).Append('/').Append(transform.Name).Append(@""");");
		}
	}

	private static void AppendTransformParams(StringBuilder sb, SliceTransform transform)
	{
		for (int pIdx = 0; pIdx < transform.Parameters.Length; pIdx++)
		{
			SliceTransformParam parameter = transform.Parameters[pIdx];

			sb.Append(parameter.Type);
			sb.Append(parameter.Nullable ? "?" : string.Empty);
			sb.Append(" ");
			sb.Append(parameter.Name.Capitalize());

			if (pIdx != transform.Parameters.Length - 1)
			{
				sb.Append(", ");
			}
		}
	}

	/// <summary>
	/// Create the partial class with the Dispatch and Pick methods.
	/// </summary>
	/// <param name="slice"></param>
	/// <param name="sb"></param>
	private static void CreatePartialClass(SliceToGenerate slice, StringBuilder sb)
	{
		sb.AppendLine().AppendLine()
			.Append(slice.Accessibility)
			.Append(" partial class ")
			.Append(slice.Name)
			.Append(" : ISlice")
			.AppendLine()
			.Append('{');

		CreateDispatch(slice, sb);
		// CreatePick(slice, sb);

		sb.AppendLine().Append('}');
	}

	/// <summary>
	/// Create the Dispatch method.
	/// </summary>
	/// <param name="slice"></param>
	/// <param name="sb"></param>
	private static void CreateDispatch(SliceToGenerate slice, StringBuilder sb)
	{
		sb.AppendLine()
			.Append(
				"""
					public void Dispatch(Transform storeAction)
					{
						switch (storeAction)
						{
				"""
			);

		foreach (SliceTransform transform in slice.Transforms)
		{
			sb.AppendLine()
				.Append("\t\t\tcase ")
				.Append(transform.RecordName)
				.Append(
					" transform:"
				)
				.AppendLine()
				.Append("\t\t\t\t")
				.Append(transform.Name)
				.Append('(');

			AppendDispatchParams(sb, transform);

			sb.Append(
				"""
				);
								break;

				"""
			);
		}

		sb.Append(
			"""
						default:
							break;
					}
				}
			"""
		);
	}

	private static void AppendDispatchParams(StringBuilder sb, SliceTransform transform)
	{
		for (int pIdx = 0; pIdx < transform.Parameters.Length; pIdx++)
		{
			SliceTransformParam parameter = transform.Parameters[pIdx];

			sb.Append("transform.");
			sb.Append(parameter.Name.Capitalize());
			if (pIdx != transform.Parameters.Length - 1)
			{
				sb.Append(", ");
			}
		}
	}

	private static string Capitalize(this string source)
	{
		if (string.IsNullOrEmpty(source))
		{
			return string.Empty;
		}

		char[] letters = source.ToCharArray();
		letters[0] = char.ToUpper(letters[0]);
		return new string(letters);
	}
}
