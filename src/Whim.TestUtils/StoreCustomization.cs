using System.Collections.Generic;
using AutoFixture;
using DotNext;
using NSubstitute;

namespace Whim.TestUtils;

internal class StoreWrapper : Store
{
	/// <summary>
	/// All the transforms that have been executed.
	/// </summary>
	public List<object> Transforms { get; } = new();

	public StoreWrapper(IContext ctx, IInternalContext internalCtx)
		: base(ctx, internalCtx) { }

	protected override Result<TResult> DispatchFn<TResult>(Transform<TResult> transform)
	{
		Transforms.Add(transform);
		return base.DispatchFn(transform);
	}
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class StoreCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		IContext ctx = fixture.Freeze<IContext>();
		IInternalContext internalCtx = fixture.Freeze<IInternalContext>();

		StoreWrapper store = new(ctx, internalCtx);
		ctx.Store.Returns(store);

		fixture.Inject(store._root);
		fixture.Inject(store._root.MutableRootSector);

		// First IsStaThread() returns true, then all further calls return false.
		// This is to ensure that the first Dispatch runs in a Task.
		// All further calls will run in the same thread.
		internalCtx.CoreNativeManager.IsStaThread().Returns(_ => true, _ => false);
		DeferWindowPosHandle.ParallelOptions = new() { MaxDegreeOfParallelism = 1 };

		NativeManagerUtils.SetupTryEnqueue(ctx);
	}
}
