using System;

namespace Whim;

/// <summary>
/// This contains internal core functionality for Whim which should not be exposed to plugins.
/// </summary>
internal interface IInternalContext : IDisposable
{
	ICoreNativeManager CoreNativeManager { get; }

	IWindowMessageMonitor WindowMessageMonitor { get; }

	internal KeybindHook KeybindHook { get; }

	internal MouseHook MouseHook { get; }

	void PreInitialize();

	void PostInitialize();
}
