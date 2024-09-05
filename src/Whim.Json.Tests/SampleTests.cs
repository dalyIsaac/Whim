using Whim.TestUtils;
using Xunit;

namespace Whim.Json.Tests;

public class SampleTests
{
	[Fact]
	public void TestSample()
	{
		var schema = Sample.GetSample();
		Assert.NotNull(schema);
	}
}
