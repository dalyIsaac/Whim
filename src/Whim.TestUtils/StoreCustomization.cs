using AutoFixture;
using NSubstitute;

namespace Whim.TestUtils;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class StoreCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		IContext ctx = fixture.Freeze<IContext>();
		IInternalContext internalCtx = fixture.Freeze<IInternalContext>();

		Store store = new(ctx, internalCtx);
		ctx.Store.Returns(store);

		fixture.Inject(store._root);
		fixture.Inject(store._root.MutableRootSector);

		// First IsStaThread() returns true, then all further calls return false.
		// This is to ensure that the first Dispatch runs in a Task.
		// All further calls will run in the same thread.
		internalCtx.CoreNativeManager.IsStaThread().Returns(_ => true, _ => false);

		NativeManagerUtils.SetupTryEnqueue(ctx);
	}
}
