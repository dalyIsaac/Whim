using System;

namespace Whim;

/// <summary>
/// This contains internal core functionality for Whim which should not be exposed to plugins.
/// </summary>
internal interface IInternalContext : IDisposable
{
	ICoreNativeManager CoreNativeManager { get; }

	IWindowMessageMonitor WindowMessageMonitor { get; }

	IInternalWindowManager WindowManager { get; }

	IKeybindHook KeybindHook { get; }

	IMouseHook MouseHook { get; }

	void PreInitialize();

	void PostInitialize();
}
