using System;

namespace Whim;

/// <summary>
/// This contains internal core functionality for Whim which should not be exposed to plugins.
/// </summary>
internal interface IInternalContext : IDisposable
{
	IInternalButler Butler { get; }

	IButlerEventHandlers ButlerEventHandlers { get; }

	ICoreSavedStateManager CoreSavedStateManager { get; }

	ICoreNativeManager CoreNativeManager { get; }

	IWindowMessageMonitor WindowMessageMonitor { get; }

	IInternalWindowManager WindowManager { get; }

	IKeybindHook KeybindHook { get; }

	IMouseHook MouseHook { get; }

	IDeferWindowPosManager DeferWindowPosManager { get; }

	IDeferWorkspacePosManager DeferWorkspacePosManager { get; }

	void PreInitialize();

	void PostInitialize();
}
