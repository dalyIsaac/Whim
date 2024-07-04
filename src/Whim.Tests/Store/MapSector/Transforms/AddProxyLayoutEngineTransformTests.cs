using FluentAssertions;

namespace Whim.Tests;

public class AddProxyLayoutEngineTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success(IContext ctx, MutableRootSector rootSector)
	{
		// Given
		ProxyLayoutEngineCreator proxyLayoutEngineCreator = Substitute.For<ProxyLayoutEngineCreator>();
		AddProxyLayoutEngineTransform sut = new(proxyLayoutEngineCreator);

		// When
		var result = ctx.Store.Dispatch(sut);

		// Then
		Assert.True(result.IsSuccessful);
		rootSector.WorkspaceSector.ProxyLayoutEngineCreators.Should().Contain(proxyLayoutEngineCreator);
	}
}
