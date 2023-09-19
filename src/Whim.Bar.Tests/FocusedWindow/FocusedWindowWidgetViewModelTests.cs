using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Bar.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class FocusedWindowWidgetViewModelTests
{
	private static FocusedWindowWidgetViewModel CreateSut(IContext context, IMonitor monitor) =>
		new(context, monitor, FocusedWindowWidget.GetTitle);

	[Theory, AutoSubstituteData]
	public void Title_SameMonitor(IContext context, IMonitor monitor, IWindow window)
	{
		// Given
		FocusedWindowWidgetViewModel viewModel = CreateSut(context, monitor);

		window.Title.Returns("title");

		context.WorkspaceManager.GetMonitorForWindow(window).Returns(monitor);

		// When
		Assert.PropertyChanged(
			viewModel,
			nameof(viewModel.Title),
			() =>
			{
				context.WindowManager.WindowFocused += Raise.Event<EventHandler<WindowFocusedEventArgs>>(
					context.WindowManager,
					new WindowFocusedEventArgs() { Window = window }
				);
			}
		);

		// Then
		Assert.Equal("title", viewModel.Title);
	}

	[Theory, AutoSubstituteData]
	public void Title_DifferentMonitor(IContext context, IMonitor monitor, IWindow window, IWindow otherWindow)
	{
		// Given
		FocusedWindowWidgetViewModel viewModel = CreateSut(context, monitor);

		window.Title.Returns("title");
		otherWindow.Title.Returns("other title");

		context.WorkspaceManager.GetMonitorForWindow(window).Returns(monitor);

		// When
		context.WindowManager.WindowFocused += Raise.Event<EventHandler<WindowFocusedEventArgs>>(
			context.WindowManager,
			new WindowFocusedEventArgs() { Window = window }
		);
		Assert.PropertyChanged(
			viewModel,
			nameof(viewModel.Title),
			() =>
			{
				context.WindowManager.WindowFocused += Raise.Event<EventHandler<WindowFocusedEventArgs>>(
					context.WindowManager,
					new WindowFocusedEventArgs() { Window = otherWindow }
				);
			}
		);

		// Then
		Assert.Null(viewModel.Title);
	}

	[Theory, AutoSubstituteData]
	[System.Diagnostics.CodeAnalysis.SuppressMessage(
		"Usage",
		"NS5000:Received check.",
		Justification = "The analyzer is wrong"
	)]
	public void Dispose(IContext context, IMonitor monitor)
	{
		// Given
		FocusedWindowWidgetViewModel viewModel = CreateSut(context, monitor);

		// When
		viewModel.Dispose();

		// Then
		context.WindowManager.Received(1).WindowFocused -= Arg.Any<EventHandler<WindowFocusedEventArgs>>();
	}

	#region WindowManager_WindowFocused
	[Theory, AutoSubstituteData]
	public void WindowManager_WindowFocused_SameMonitor(IContext context, IMonitor monitor, IWindow window)
	{
		// Given
		FocusedWindowWidgetViewModel viewModel = CreateSut(context, monitor);

		window.Title.Returns("title");
		context.WorkspaceManager.GetMonitorForWindow(window).Returns(monitor);

		// When
		Assert.PropertyChanged(
			viewModel,
			nameof(viewModel.Title),
			() =>
				context.WindowManager.WindowFocused += Raise.Event<EventHandler<WindowFocusedEventArgs>>(
					context.WindowManager,
					new WindowFocusedEventArgs() { Window = window }
				)
		);

		// Then
		Assert.Equal("title", viewModel.Title);
	}

	[Theory, AutoSubstituteData]
	public void WindowManager_WindowFocused_DifferentMonitor(
		IContext context,
		IMonitor monitor,
		IWindow window,
		IWindow otherWindow
	)
	{
		// Given
		FocusedWindowWidgetViewModel viewModel = CreateSut(context, monitor);

		context.WorkspaceManager.GetMonitorForWindow(window).Returns(monitor);

		window.Title.Returns("title");
		otherWindow.Title.Returns("other title");

		// When
		context.WindowManager.WindowFocused += Raise.Event<EventHandler<WindowFocusedEventArgs>>(
			context.WindowManager,
			new WindowFocusedEventArgs() { Window = window }
		);
		Assert.PropertyChanged(
			viewModel,
			nameof(viewModel.Title),
			() =>
				context.WindowManager.WindowFocused += Raise.Event<EventHandler<WindowFocusedEventArgs>>(
					context.WindowManager,
					new WindowFocusedEventArgs() { Window = otherWindow }
				)
		);

		// Then
		Assert.Null(viewModel.Title);
	}

	[Theory, AutoSubstituteData]
	public void WindowManager_WindowFocused_WindowIsNull(IContext context, IMonitor monitor, IWindow window)
	{
		// Given
		FocusedWindowWidgetViewModel viewModel = CreateSut(context, monitor);

		window.Title.Returns("title");
		context.WorkspaceManager.GetMonitorForWindow(window).Returns(monitor);

		// When
		Assert.PropertyChanged(
			viewModel,
			nameof(viewModel.Title),
			() =>
				context.WindowManager.WindowFocused += Raise.Event<EventHandler<WindowFocusedEventArgs>>(
					context.WindowManager,
					new WindowFocusedEventArgs() { Window = window }
				)
		);

		Assert.Equal("title", viewModel.Title);

		Assert.PropertyChanged(
			viewModel,
			nameof(viewModel.Title),
			() =>
				context.WindowManager.WindowFocused += Raise.Event<EventHandler<WindowFocusedEventArgs>>(
					context.WindowManager,
					new WindowFocusedEventArgs() { Window = null }
				)
		);

		// Then
		Assert.Null(viewModel.Title);
	}
	#endregion
}
