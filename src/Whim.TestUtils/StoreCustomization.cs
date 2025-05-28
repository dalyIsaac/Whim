using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AutoFixture;
using DotNext;
using NSubstitute;
using Windows.Win32.Foundation;

namespace Whim.TestUtils;

internal class StoreWrapper(IContext ctx, IInternalContext internalCtx) : Store(ctx, internalCtx)
{
	/// <summary>
	/// All the transforms that have been executed.
	/// </summary>
	public List<object> Transforms { get; } = [];

	protected override Result<TResult> DispatchFn<TResult>(Transform<TResult> transform)
	{
		Transforms.Add(transform);
		return base.DispatchFn(transform);
	}

	protected override Result<TResult> WhimDispatchFn<TResult>(WhimTransform<TResult> transform)
	{
		Transforms.Add(transform);
		return base.WhimDispatchFn(transform);
	}
}

[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable")]
public class StoreCustomization : ICustomization
{
#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	protected IContext _ctx;
	private protected IInternalContext _internalCtx;
	private protected StoreWrapper _store;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CA1051 // Do not declare visible instance fields

	public void Customize(IFixture fixture)
	{
		_ctx = fixture.Freeze<IContext>();
		_internalCtx = fixture.Freeze<IInternalContext>();

		_store = new(_ctx, _internalCtx);
		_ctx.Store.Returns(_store);

		fixture.Inject(_store._root);
		fixture.Inject(_store._root.MutableRootSector);
		fixture.Inject(_store.Transforms);

		_store._root.MutableRootSector.MonitorSector.MonitorsChangedDelay = 0;

		// First IsStaThread() returns true, then all further calls return false.
		// This is to ensure that the first Dispatch runs in a Task.
		// All further calls will run in the same thread.
		_internalCtx.CoreNativeManager.IsStaThread().Returns(_ => true, _ => false);
		DeferWindowPosHandle.ParallelOptions = new() { MaxDegreeOfParallelism = 1 };

		// Assume that all windows are windows.
		_internalCtx.CoreNativeManager.IsWindow(Arg.Any<HWND>()).Returns(true);

		NativeManagerUtils.SetupTryEnqueue(_ctx);

		PostCustomize(fixture);
	}

	/// <summary>
	/// A method to allow child classes to customize the setup.
	/// </summary>
	protected virtual void PostCustomize(IFixture fixture) { }
}
