using System;

namespace Whim;

/// <summary>
/// This contains internal core functionality for Whim which should not be exposed to plugins.
/// </summary>
internal interface IInternalContext : IDisposable
{
	ICoreNativeManager CoreNativeManager { get; }

	IWindowMessageMonitor WindowMessageMonitor { get; }

	internal IKeybindHook KeybindHook { get; }

	internal IMouseHook MouseHook { get; }

	void PreInitialize();

	void PostInitialize();
}
