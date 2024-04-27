using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim.Tests;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class MonitorEventListenerTests
{
	private static readonly WindowMessageMonitorEventArgs _windowMessageArgs =
		new()
		{
			MessagePayload = new()
			{
				HWnd = (HWND)1,
				LParam = (LPARAM)1,
				UMsg = 1,
				WParam = (WPARAM)1
			}
		};

	[Theory, AutoSubstituteData]
	[SuppressMessage("Usage", "NS5000:Received check.")]
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
			ctx.Store.Monitors,
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
			ctx.Store.Monitors,
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
			ctx.Store.Monitors,
			_windowMessageArgs
		);

		// Then a dispatch to the store was triggered
		ctx.Store.Received(1).Dispatch(new MonitorsChangedTransform());
	}

	[Theory, AutoSubstituteData]
	internal async void WindowMessageMonitor_SessionChanged(IContext ctx, IInternalContext internalCtx)
	{
		// Given the listener is initialized
		MonitorEventListener listener = new(ctx, internalCtx);
		listener.Initialize();

		NativeManagerUtils.SetupTryEnqueue(ctx);

		// When WindowMessageMonitor.SessionChanged is triggered
		internalCtx.WindowMessageMonitor.SessionChanged += Raise.Event<EventHandler<WindowMessageMonitorEventArgs>>(
			ctx.Store.Monitors,
			_windowMessageArgs
		);
		await Task.Delay(5100);

		// Then a dispatch to the store was triggered
		ctx.Store.Received(1).Dispatch(new MonitorsChangedTransform());
	}

	[Theory, AutoSubstituteData]
	internal void MouseHook_MouseLeftButtonUp(IContext ctx, IInternalContext internalCtx)
	{
		// Given the listener is initialized
		MonitorEventListener listener = new(ctx, internalCtx);
		listener.Initialize();

		// When MouseHook.MouseLeftButtonUp is triggered
		internalCtx.MouseHook.MouseLeftButtonUp += Raise.Event<EventHandler<MouseEventArgs>>(
			ctx.Store.Monitors,
			new MouseEventArgs(new Point<int>(0, 0))
		);

		// Then a dispatch to the store was triggered
		ctx.Store.Received(1).Dispatch(Arg.Any<MouseLeftButtonUpTransform>());
	}

	[SuppressMessage("Usage", "NS5000:Received check.")]
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
