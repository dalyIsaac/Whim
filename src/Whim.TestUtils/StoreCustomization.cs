using AutoFixture;
using NSubstitute;

namespace Whim.TestUtils;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
internal class StoreCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		IContext ctx = fixture.Freeze<IContext>();
		IInternalContext internalCtx = fixture.Freeze<IInternalContext>();

		ctx.Store.Returns(new Store(ctx, internalCtx));
	}
}
