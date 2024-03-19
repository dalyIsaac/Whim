using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Whim;

// Despite what the compiler says, `_lockWasTaken` is reassigned by `Monitor.Enter`.
[SuppressMessage("Style", "IDE0044:Make field readonly")]
internal class Lock : IDisposable
{
	private readonly object _lockObj;
	private bool _lockWasTaken;

	public Lock(object lockObj)
	{
		_lockObj = lockObj;
		System.Threading.Monitor.Enter(_lockObj, ref _lockWasTaken);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Dispose()
	{
		if (_lockWasTaken)
		{
			System.Threading.Monitor.Exit(_lockObj);
			Console.WriteLine("Freed");
		}
	}
}
