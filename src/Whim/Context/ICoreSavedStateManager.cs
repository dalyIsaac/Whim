using System;
using System.Collections.Generic;

namespace Whim;

internal record SavedWindow(long Handle, Rectangle<double> Rectangle);

internal record SavedWorkspace(string Name, List<SavedWindow> Windows);

internal record CoreSavedState(List<SavedWorkspace> Workspaces);

internal interface ICoreSavedStateManager : IDisposable
{
	/// <summary>
	/// The loaded saved state.
	/// </summary>
	CoreSavedState? SavedState { get; }

	/// <summary>
	/// Load the saved state. Each manager is responsible for loading its own state.
	/// </summary>
	void PreInitialize();

	/// <summary>
	/// Clears the saved state.
	/// </summary>
	void PostInitialize();
}
