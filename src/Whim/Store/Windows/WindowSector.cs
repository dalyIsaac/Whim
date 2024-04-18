using System;
using System.Collections.Immutable;
using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// The sector containing windows.
/// </summary>
internal class WindowSector : SectorBase, IWindowSector, IDisposable, IWindowSectorEvents
{
	private readonly IContext _ctx;
	private readonly IInternalContext _internalCtx;
	private readonly WindowEventListener _listener;
	private bool _disposedValue;

	public ImmutableDictionary<HWND, IWindow> Windows { get; set; } = ImmutableDictionary<HWND, IWindow>.Empty;

	internal WindowSector(IContext ctx, IInternalContext internalCtx)
	{
		_ctx = ctx;
		_internalCtx = internalCtx;
		_listener = new WindowEventListener(ctx, internalCtx);
	}

	// TODO: Add to StoreTests
	public override void Initialize() { }

	public override void DispatchEvents()
	{
		foreach (EventArgs eventArgs in _events)
		{
			switch (eventArgs)
			{
				// TODO
				default:
					break;
			}
		}

		_events.Clear();
	}

	/// <inheritdoc/>
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_listener.Dispose();
			}

			// free unmanaged resources (unmanaged objects) and override finalizer
			// set large fields to null
			_disposedValue = true;
		}
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
