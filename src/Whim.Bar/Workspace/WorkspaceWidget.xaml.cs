using System;
using Microsoft.UI.Xaml.Controls;

namespace Whim.Bar;

/// <summary>
/// Interaction logic for WorkspaceWidget.xaml
/// </summary>
public partial class WorkspaceWidget : UserControl, IDisposable
{
	private readonly Microsoft.UI.Xaml.Window _window;
	private bool _disposedValue;

	/// <summary>
	/// The workspace view model.
	/// </summary>
	internal WorkspaceWidgetViewModel ViewModel { get; }

	internal WorkspaceWidget(IContext context, IMonitor monitor, Microsoft.UI.Xaml.Window window)
	{
		_window = window;
		ViewModel = new WorkspaceWidgetViewModel(context, monitor);
		window.Closed += Window_Closed;
		UIElementExtensions.InitializeComponent(this, "Whim.Bar", "Workspace/WorkspaceWidget");
	}

	private void Window_Closed(object? sender, Microsoft.UI.Xaml.WindowEventArgs e)
	{
		ViewModel.Dispose();
	}

	/// <summary>
	/// Create the workspace widget bar component.
	/// </summary>
	public static BarComponent CreateComponent()
	{
		return new BarComponent((context, monitor, window) => new WorkspaceWidget(context, monitor, window));
	}

	/// <inheritdoc/>
	protected virtual void Dispose(bool disposing)
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
