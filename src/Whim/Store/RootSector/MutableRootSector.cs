using System;

namespace Whim;

internal class MutableRootSector : SectorBase, IDisposable
{
	private bool _disposedValue;

	public MutableRootSector(IContext ctx, IInternalContext internalCtx) { }

	public override void Initialize() { }

	public override void DispatchEvents()
	{
		Logger.Debug("Dispatching events");
		// TODO
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				// TODO
			}

			// free unmanaged resources (unmanaged objects) and override finalizer
			// set large fields to null
			_disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
