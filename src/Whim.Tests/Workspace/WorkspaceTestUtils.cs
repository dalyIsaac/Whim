using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Win32.Foundation;

namespace Whim.Tests;

internal static class WorkspaceTestUtils
{
	public static void SetupDoLayout(this IInternalContext internalContext) =>
		internalContext.CoreNativeManager
			.When(
				_ =>
					_.RunTask(
						Arg.Any<Func<Dictionary<HWND, IWindowState>>>(),
						Arg.Any<Action<Task<Dictionary<HWND, IWindowState>>>>(),
						Arg.Any<CancellationToken>()
					)
			)
			.Do(_ =>
			{
				Func<Dictionary<HWND, IWindowState>> work = _.Arg<Func<Dictionary<HWND, IWindowState>>>();
				Action<Task<Dictionary<HWND, IWindowState>>> cleanup = _.Arg<
					Action<Task<Dictionary<HWND, IWindowState>>>
				>();
				CancellationToken cancellationToken = _.Arg<CancellationToken>();

				// Run the work on the current thread.
				Dictionary<HWND, IWindowState> result = work();
				cleanup(Task.FromResult(result));
			});
}
