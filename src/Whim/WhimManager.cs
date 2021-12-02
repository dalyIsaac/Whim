using System;
using Whim.Core.ConfigContext;

namespace Whim;

public class WhimManager : IDisposable
{
	private bool _disposedValue;
	private readonly IConfigContext _configContext;

	public WhimManager(IConfigContext configContext)
	{
		_configContext = configContext;
	}

	public bool Initialize()
	{
		// Initialize the window manager
		if (!_configContext.WindowManager.Initialize())
		{
			return false;
		}

		return true;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_configContext.WindowManager.Dispose();
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
