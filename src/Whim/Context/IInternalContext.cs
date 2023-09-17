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

	/// <summary>
	/// This lock is used to prevent <see cref="IWindowManager"/> events from modifying the
	/// <see cref="MonitorManager"/> and <see cref="WorkspaceManager"/> while a
	/// <see cref="Workspace.DoLayout"/> layout is occurring.
	/// </summary>
	ReaderWriterLockSlim LayoutLock { get; }

	void PreInitialize();

	void PostInitialize();
}
