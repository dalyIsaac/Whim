using System;
using Microsoft.UI.Xaml.Controls;

namespace Whim.TreeLayout.Bar;

/// <summary>
/// Interaction logic for TreeLayoutEngineWidget.
/// </summary>
public sealed partial class TreeLayoutEngineWidget : UserControl, IDisposable
{
	private bool _disposedValue;

	private readonly Microsoft.UI.Xaml.Window _window;

	/// <summary>
	/// The tree layout engine widget.
	/// </summary>
	public TreeLayoutEngineWidgetViewModel ViewModel { get; }

	internal TreeLayoutEngineWidget(
		IContext context,
		ITreeLayoutPlugin plugin,
		IMonitor monitor,
		Microsoft.UI.Xaml.Window window
	)
	{
		_window = window;
		ViewModel = new TreeLayoutEngineWidgetViewModel(context, plugin, monitor);
		window.Closed += Window_Closed;
		UIElementExtensions.InitializeComponent(this, "Whim.TreeLayout.Bar", "TreeLayoutEngineWidget");
	}

	private void Window_Closed(object sender, Microsoft.UI.Xaml.WindowEventArgs args)
	{
		ViewModel.Dispose();
	}

	/// <inheritdoc/>
	private void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_window.Closed -= Window_Closed;
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
