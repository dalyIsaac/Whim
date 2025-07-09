using AutoFixture;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Xunit;
using static Whim.TestUtils.StoreTestUtils;

namespace Whim.Bar.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class FocusedWindowWidgetViewModelTests
{
	private class Customization : StoreCustomization
	{
		protected override void PostCustomize(IFixture fixture)
		{
			IWindow window = CreateWindow((HWND)100);
			window.Title.Returns("title");
			fixture.Inject(window);

			IMonitor monitor = CreateMonitor((HMONITOR)100);
			fixture.Inject(monitor);
		}
	}

	private static IWindow CreateOtherWindow()
	{
		IWindow otherWindow = CreateWindow((HWND)200);
		otherWindow.Title.Returns("other title");
		return otherWindow;
	}

	private static FocusedWindowWidgetViewModel CreateSut(IContext ctx, IMonitor monitor) =>
		new(ctx, monitor, FocusedWindowWidget.GetTitle);

	[Theory, AutoSubstituteData<Customization>]
	internal void Title_SameMonitor(IContext ctx, MutableRootSector root, IMonitor monitor, IWindow window)
	{
		// Given
		FocusedWindowWidgetViewModel viewModel = CreateSut(ctx, monitor);
		PopulateThreeWayMap(ctx, root, monitor, CreateWorkspace(ctx), window);

		// When
		root.WindowSector.QueueEvent(new WindowFocusedEventArgs() { Window = window });
		Assert.PropertyChanged(viewModel, nameof(viewModel.Title), root.DispatchEvents);

		// Then
		Assert.Equal("title", viewModel.Title);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void Title_DifferentMonitor(IContext ctx, MutableRootSector root, IMonitor monitor, IWindow window)
	{
		// Given
		FocusedWindowWidgetViewModel viewModel = CreateSut(ctx, monitor);

		IWindow otherWindow = CreateOtherWindow();
		IMonitor otherMonitor = CreateMonitor((HMONITOR)200);

		PopulateThreeWayMap(ctx, root, monitor, CreateWorkspace(ctx), window);

		// Setup the initial title
		root.WindowSector.QueueEvent(new WindowFocusedEventArgs() { Window = window });
		root.DispatchEvents();

		// When
		root.WindowSector.QueueEvent(new WindowFocusedEventArgs() { Window = otherWindow });
		Assert.PropertyChanged(viewModel, nameof(viewModel.Title), root.DispatchEvents);

		// Then
		Assert.Null(viewModel.Title);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void Title_WindowIsNull(IContext ctx, MutableRootSector root, IMonitor monitor, IWindow window)
	{
		// Given
		FocusedWindowWidgetViewModel viewModel = CreateSut(ctx, monitor);
		PopulateThreeWayMap(ctx, root, monitor, CreateWorkspace(ctx), window);

		// When
		root.WindowSector.QueueEvent(new WindowFocusedEventArgs() { Window = window });
		Assert.PropertyChanged(viewModel, nameof(viewModel.Title), root.DispatchEvents);

		Assert.Equal("title", viewModel.Title);

		root.WindowSector.QueueEvent(new WindowFocusedEventArgs() { Window = null });
		Assert.PropertyChanged(viewModel, nameof(viewModel.Title), root.DispatchEvents);

		// Then
		Assert.Null(viewModel.Title);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void Dispose(IContext ctx, MutableRootSector root, IMonitor monitor, IWindow window)
	{
		// Given
		FocusedWindowWidgetViewModel viewModel = CreateSut(ctx, monitor);

		// When
		viewModel.Dispose();

		// Then implicitly check that the event handler is removed
		CustomAssert.DoesNotPropertyChange(
			h => viewModel.PropertyChanged += h,
			h => viewModel.PropertyChanged -= h,
			() =>
			{
				PopulateThreeWayMap(ctx, root, monitor, CreateWorkspace(ctx), window);
				root.WindowSector.QueueEvent(new WindowFocusedEventArgs() { Window = window });
				root.DispatchEvents();
			}
		);
	}
}
