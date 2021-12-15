using System;
using Whim.Core;

namespace Whim;

/// <summary>
/// <c>WhimManager</c> contains the <see cref="IConfigContext"/> and application logic (like how to
/// start up, etc.), which are not provided to the context.
/// </summary>
public class WhimManager : IDisposable
{
	private bool _disposedValue;
	public IConfigContext ConfigContext { get; }

	public WhimManager(IConfigContext configContext)
	{
		ConfigContext = configContext;
	}

	public void Initialize()
	{
		ConfigContext.Initialize();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				ConfigContext.Dispose();
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
