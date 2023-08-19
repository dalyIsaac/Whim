using System.Runtime.InteropServices;

namespace Windows.Win32;

internal partial class PInvoke
{
	/// <summary>Creates a DispatcherQueueController on the caller's thread. Use the created DispatcherQueueController to create and manage the lifetime of a DispatcherQueue to run queued tasks in priority order on the Dispatcher queue's thread.</summary>
	/// <param name="options">The threading affinity and type of COM apartment for the created <a href="https://docs.microsoft.com/uwp/api/windows.system.dispatcherqueuecontroller">DispatcherQueueController</a>. See remarks for details.</param>
	/// <param name="dispatcherQueueController">
	/// <para>The created dispatcher queue controller. <div class="alert"><b>Important</b>  The <a href="https://docs.microsoft.com/uwp/api/windows.system.dispatcherqueuecontroller">DispatcherQueueController</a> is a WinRT object.</div> <div> </div></para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api/dispatcherqueue/nf-dispatcherqueue-createdispatcherqueuecontroller#parameters">Read more on docs.microsoft.com</see>.</para>
	/// </param>
	/// <returns><b>S_OK</b> for success; otherwise a failure code.</returns>
	/// <remarks>
	/// <para>Introduced in Windows 10, version 1709. If  <i>options.threadType</i> is <b>DQTYPE_THREAD_DEDICATED</b>, then this function  creates the dedicated thread and then creates the  <a href="https://docs.microsoft.com/uwp/api/windows.system.dispatcherqueuecontroller">DispatcherQueueController</a> on that thread. The dispatcher queue event loop runs on the new dedicated thread. An event loop runs asynchronously on a background thread to dispatch queued task items to the new dedicated thread. If <i>options.threadType</i> is  <b>DQTYPE_THREAD_CURRENT</b>, then the <a href="https://docs.microsoft.com/uwp/api/windows.system.dispatcherqueuecontroller">DispatcherQueueController</a> instance is created on the current thread. An error results if there is already a <b>IDispatcherQueueController</b> on the current thread. If you create a dispatcher queue on the current thread, ensure that there is a message pump running on the current thread so that the dispatcher queue can use it to dispatch tasks. This call does not return until the new thread and <a href="https://docs.microsoft.com/uwp/api/windows.system.dispatcherqueuecontroller">DispatcherQueueController</a> are created. The new thread will be initialized using the specified COM apartment. <div class="alert"><b>Important</b>  The <a href="https://docs.microsoft.com/uwp/api/windows.system.dispatcherqueuecontroller">DispatcherQueueController</a>, and its associated <a href="https://docs.microsoft.com/uwp/api/windows.system.dispatcherqueue">DispatcherQueue</a>, are WinRT objects. See their documentation for usage details.</div> <div> </div></para>
	/// <para><see href="https://docs.microsoft.com/windows/win32/api/dispatcherqueue/nf-dispatcherqueue-createdispatcherqueuecontroller#">Read more on docs.microsoft.com</see>.</para>
	/// </remarks>
	[DllImport("CoreMessaging.dll", ExactSpelling = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	internal static extern Foundation.HRESULT CreateDispatcherQueueController(
		System.WinRT.DispatcherQueueOptions options,
		out nint dispatcherQueueController
	);
}
