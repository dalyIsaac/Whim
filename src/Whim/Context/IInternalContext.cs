using System;
using System.Threading;

namespace Whim;

/// <summary>
/// This contains internal core functionality for Whim which should not be exposed to plugins.
/// </summary>
internal interface IInternalContext : IDisposable
{
	ICoreNativeManager CoreNativeManager { get; }

	IWindowMessageMonitor WindowMessageMonitor { get; }

	IKeybindHook KeybindHook { get; }

	IMouseHook MouseHook { get; }

	ReaderWriterLockSlim LayoutLock { get; }

	void PreInitialize();

	void PostInitialize();
}
