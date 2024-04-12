using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Whim.SourceGenerators.Tests;

public class SliceGeneratorTests
{
	private const string SampleSliceString = """
		namespace Whim;

		[Slice(Path = "/Parent", Key = "Sample")]
		internal partial class SampleSlice
		{
			[Transformer]
			private void AddTestData(int a, int b)
			{
				int c = a + b;
			}

			private void OtherMethod(string s)
			{
				string lowerS = s.ToLower();
			}
		}
		""";

	public const string ExpectedGeneratedSlice = """
		using System;

		namespace Whim;

		public sealed record AddTestDataTransform(Int32 A, Int32 B) : Transform("/ParentSample/AddTestData");

		internal partial class SampleSlice : ISlice
		{
			public void Dispatch(Transform storeAction)
			{
				switch (storeAction)
				{
					case AddTestDataTransform transform:
						AddTestData(transform.A, transform.B);
						break;
					default:
						break;
				}
			}
		}
		""";

	[Fact]
	public void TestGenerator()
	{
		// directly create an instance of the generator
		// (Note: in the compiler this is loaded from an assembly, and created via reflection at runtime)
		SliceGenerator generator = new();

		// Create the driver that will control the generation, passing in our generator
		GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
		// Run the generation pass
		// (Note: the generator driver itself is immutable, and all calls return an updated version of the driver that you should use for subsequent calls)
		_ = driver.RunGeneratorsAndUpdateCompilation(
			CreateCompilation(),
			out var outputCompilation,
			out var diagnostics
		);

		Assert.True(diagnostics.IsEmpty);
		
		// We have three syntax trees:
		// 1. the original 'user' provided one
		// 2. the generated attributes
		// 3. the generated slice
		Assert.Equal(3, outputCompilation.SyntaxTrees.Count());

		// TODO: Remove
		var result = outputCompilation.GetDiagnostics();
		var s = outputCompilation.SyntaxTrees.ElementAt(2).ToString();

		Assert.True(outputCompilation.GetDiagnostics().IsEmpty);

		// Verify that the generated slice matches.
		Assert.Equal(ExpectedGeneratedSlice, outputCompilation.SyntaxTrees.ElementAt(2).ToString());
	}

	private static CSharpCompilation CreateCompilation() =>
		CSharpCompilation
			.Create("Whim.Tests.GenerateAssembly")
			.AddSyntaxTrees(CSharpSyntaxTree.ParseText(SampleSliceString))
			.AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
			.WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
}
