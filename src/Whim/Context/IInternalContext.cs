using System;
using System.Threading.Tasks;

namespace Whim;

/// <summary>
/// This contains internal core functionality for Whim which should not be exposed to plugins.
/// </summary>
internal interface IInternalContext : IDisposable
{
	ParallelOptions ParallelOptions { get; }

	IInternalButler Butler { get; }

	IButlerEventHandlers ButlerEventHandlers { get; }

	ICoreSavedStateManager CoreSavedStateManager { get; }

	ICoreNativeManager CoreNativeManager { get; }

	IWindowMessageMonitor WindowMessageMonitor { get; }

	IInternalMonitorManager MonitorManager { get; }

	IInternalWindowManager WindowManager { get; }

	IKeybindHook KeybindHook { get; }

	IMouseHook MouseHook { get; }

	IDeferWorkspacePosManager DeferWorkspacePosManager { get; }

	ISubscriber NotificationManager { get; }

	void PreInitialize();

	void PostInitialize();
}
