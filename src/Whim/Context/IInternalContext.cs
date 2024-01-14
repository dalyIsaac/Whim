using System;

namespace Whim;

/// <summary>
/// This contains internal core functionality for Whim which should not be exposed to plugins.
/// </summary>
internal interface IInternalContext : IDisposable
{
	/// <inheritdoc cref="ICoreSavedStateManager" />
	ICoreSavedStateManager CoreSavedStateManager { get; }

	/// <inheritdoc cref="ICoreNativeManager" />
	ICoreNativeManager CoreNativeManager { get; }

	/// <inheritdoc cref="IWindowMessageMonitor" />
	IWindowMessageMonitor WindowMessageMonitor { get; }

	/// <inheritdoc cref="IInternalMonitorManager" />
	IInternalMonitorManager MonitorManager { get; }

	/// <inheritdoc cref="IInternalWindowManager" />
	IInternalWindowManager WindowManager { get; }

	/// <inheritdoc cref="IKeybindHook" />
	IKeybindHook KeybindHook { get; }

	/// <inheritdoc cref="IMouseHook" />
	IMouseHook MouseHook { get; }

	/// <inheritdoc cref="IDeferWindowPosManager" />
	IDeferWindowPosManager DeferWindowPosManager { get; }

	void PreInitialize();

	void PostInitialize();
}
