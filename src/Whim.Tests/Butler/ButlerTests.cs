using System.Diagnostics.CodeAnalysis;
using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

public class ButlerTests
{
	[Theory, AutoSubstituteData]
	[SuppressMessage("Style", "IDE0017:Simplify object initialization", Justification = "Testing the setter")]
	internal void SetPantry_BeforeInitialize(IContext ctx, IInternalContext internalContext, IButlerPantry pantry)
	{
		// Given the pantry is not set
		Butler sut = new(ctx, internalContext);

		// When we attempt to set the pantry
		sut.Pantry = pantry;

		// Then the pantry is set
		Assert.Equal(pantry, sut.Pantry);
	}

	[Theory, AutoSubstituteData]
	internal void SetPantry_AfterInitialize(IContext ctx, IInternalContext internalContext, IButlerPantry pantry)
	{
		// Given the pantry is not set
		Butler sut = new(ctx, internalContext);
		sut.Initialize();

		// When we attempt to set the pantry
		sut.Pantry = pantry;

		// Then the pantry is not set
		Assert.NotEqual(pantry, sut.Pantry);
	}

	[Theory, AutoSubstituteData]
	internal void TriggerWindowRouted(
		IContext ctx,
		IInternalContext internalContext,
		IWindow window,
		IWorkspace workspace
	)
	{
		// Given
		Butler sut = new(ctx, internalContext);

		// When we call TriggerWindowRouted, then the event is triggered
		Assert.Raises<RouteEventArgs>(
			h => sut.WindowRouted += h,
			h => sut.WindowRouted -= h,
			() => sut.TriggerWindowRouted(RouteEventArgs.WindowAdded(window, workspace))
		);
	}

	[Theory, AutoSubstituteData]
	internal void TriggerMonitorWorkspaceChanged(
		IContext ctx,
		IInternalContext internalContext,
		IMonitor monitor,
		IWorkspace workspace
	)
	{
		// Given
		Butler sut = new(ctx, internalContext);

		// When we call TriggerMonitorWorkspaceChanged, then the event is triggered
		Assert.Raises<MonitorWorkspaceChangedEventArgs>(
			h => sut.MonitorWorkspaceChanged += h,
			h => sut.MonitorWorkspaceChanged -= h,
			() =>
				sut.TriggerMonitorWorkspaceChanged(
					new MonitorWorkspaceChangedEventArgs() { CurrentWorkspace = workspace, Monitor = monitor }
				)
		);
	}
}
