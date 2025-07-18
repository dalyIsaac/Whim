using System.Threading.Tasks;

namespace Whim.Tests;

public class MonitorEventListenerTests
{
	private static readonly WindowMessageMonitorEventArgs _windowMessageArgs = new()
	{
		MessagePayload = new()
		{
			HWnd = (HWND)1,
			LParam = (LPARAM)1,
			UMsg = 1,
			WParam = (WPARAM)1,
		},
	};

	[Theory, AutoSubstituteData]
	internal void Initialize(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		MonitorEventListener listener = new(ctx, internalCtx);

		// When the listener is initialized
		listener.Initialize();

		// Then
		internalCtx.WindowMessageMonitor.Received(1).DisplayChanged += Arg.Any<
			EventHandler<WindowMessageMonitorEventArgs>
		>();
		internalCtx.WindowMessageMonitor.Received(1).WorkAreaChanged += Arg.Any<
			EventHandler<WindowMessageMonitorEventArgs>
		>();
		internalCtx.WindowMessageMonitor.Received(1).DpiChanged += Arg.Any<
			EventHandler<WindowMessageMonitorEventArgs>
		>();
		internalCtx.WindowMessageMonitor.Received(1).SessionChanged += Arg.Any<
			EventHandler<WindowMessageMonitorEventArgs>
		>();
	}

	[Theory, AutoSubstituteData]
	internal void WindowMessageMonitor_DisplayChanged(IContext ctx, IInternalContext internalCtx)
	{
		// Given the listener is initialized
		MonitorEventListener listener = new(ctx, internalCtx);
		listener.Initialize();

		// When WindowMessageMonitor.DisplayChanged is triggered
		internalCtx.WindowMessageMonitor.DisplayChanged += Raise.Event<EventHandler<WindowMessageMonitorEventArgs>>(
			ctx.Store.MonitorEvents,
			_windowMessageArgs
		);

		// Then a dispatch to the store was triggered
		ctx.Store.Received(1).Dispatch(new MonitorsChangedTransform());
	}

	[Theory, AutoSubstituteData]
	internal void WindowMessageMonitor_WorkAreaChanged(IContext ctx, IInternalContext internalCtx)
	{
		// Given the listener is initialized
		MonitorEventListener listener = new(ctx, internalCtx);
		listener.Initialize();

		// When WindowMessageMonitor.WorkAreaChanged is triggered
		internalCtx.WindowMessageMonitor.WorkAreaChanged += Raise.Event<EventHandler<WindowMessageMonitorEventArgs>>(
			ctx.Store.MonitorEvents,
			_windowMessageArgs
		);

		// Then a dispatch to the store was triggered
		ctx.Store.Received(1).Dispatch(new MonitorsChangedTransform());
	}

	[Theory, AutoSubstituteData]
	internal void WindowMessageMonitor_DpiChanged(IContext ctx, IInternalContext internalCtx)
	{
		// Given the listener is initialized
		MonitorEventListener listener = new(ctx, internalCtx);
		listener.Initialize();

		// When WindowMessageMonitor.DpiChanged is triggered
		internalCtx.WindowMessageMonitor.DpiChanged += Raise.Event<EventHandler<WindowMessageMonitorEventArgs>>(
			ctx.Store.MonitorEvents,
			_windowMessageArgs
		);

		// Then a dispatch to the store was triggered
		ctx.Store.Received(1).Dispatch(new MonitorsChangedTransform());
	}

	[Theory, AutoSubstituteData]
	internal async Task WindowMessageMonitor_SessionChanged(IContext ctx, IInternalContext internalCtx)
	{
		// Given the listener is initialized
		MonitorEventListener listener = new(ctx, internalCtx, 500);
		listener.Initialize();

		NativeManagerUtils.SetupTryEnqueue(ctx);

		// When WindowMessageMonitor.SessionChanged is triggered
		internalCtx.WindowMessageMonitor.SessionChanged += Raise.Event<EventHandler<WindowMessageMonitorEventArgs>>(
			ctx.Store.MonitorEvents,
			_windowMessageArgs
		);
		await Task.Delay(2000);

		// Then a dispatch to the store was triggered
		ctx.Store.Received(1).Dispatch(new MonitorsChangedTransform());
	}

	[Theory, AutoSubstituteData]
	internal void Dispose_Success(IContext ctx, IInternalContext internalCtx)
	{
		// Given the listener is initialized
		MonitorEventListener listener = new(ctx, internalCtx);
		listener.Initialize();

		// When disposing
		listener.Dispose();

		// Then the listener unsubscribed to all events
		internalCtx.WindowMessageMonitor.Received(1).DisplayChanged -= Arg.Any<
			EventHandler<WindowMessageMonitorEventArgs>
		>();
		internalCtx.WindowMessageMonitor.Received(1).WorkAreaChanged -= Arg.Any<
			EventHandler<WindowMessageMonitorEventArgs>
		>();
		internalCtx.WindowMessageMonitor.Received(1).DpiChanged -= Arg.Any<
			EventHandler<WindowMessageMonitorEventArgs>
		>();
		internalCtx.WindowMessageMonitor.Received(1).SessionChanged -= Arg.Any<
			EventHandler<WindowMessageMonitorEventArgs>
		>();
	}
}
