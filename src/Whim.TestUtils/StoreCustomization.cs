using AutoFixture;
using NSubstitute;

namespace Whim.TestUtils;

internal class StoreCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		IContext ctx = fixture.Freeze<IContext>();
		IInternalContext internalCtx = fixture.Freeze<IInternalContext>();

		ctx.Store.Returns(new Store(ctx, internalCtx));
	}
}
