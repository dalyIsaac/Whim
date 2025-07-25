using System;
using System.ComponentModel;

namespace Whim.Bar;

/// <summary>
/// Command for switching workspace.
/// </summary>
internal class SwitchWorkspaceCommand : System.Windows.Input.ICommand, IDisposable
{
	private readonly IContext _context;
	private readonly WorkspaceWidgetViewModel _viewModel;
	private readonly WorkspaceModel _workspace;
	private bool _disposedValue;

	/// <inheritdoc/>
	public event EventHandler? CanExecuteChanged;

	/// <summary>
	/// Creates a new instance of <see cref="SwitchWorkspaceCommand"/>.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="viewModel"></param>
	/// <param name="workspace"></param>
	public SwitchWorkspaceCommand(IContext context, WorkspaceWidgetViewModel viewModel, WorkspaceModel workspace)
	{
		_context = context;
		_viewModel = viewModel;
		_workspace = workspace;
		_workspace.PropertyChanged += Workspace_PropertyChanged;
	}

	private void Workspace_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(_workspace.ActiveOnMonitor))
		{
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	/// <inheritdoc/>
	public bool CanExecute(object? parameter) => !_workspace.ActiveOnMonitor;

	/// <inheritdoc/>
	public void Execute(object? parameter)
	{
		Logger.Debug("Executing...");
		if (parameter is WorkspaceModel)
		{
			Logger.Debug($"Activating workspace {_workspace.Workspace} on monitor {_viewModel.Monitor}");
			_context.Store.Dispatch(new ActivateWorkspaceTransform(_workspace.Workspace.Id, _viewModel.Monitor.Handle));
		}
	}

	/// <inheritdoc/>
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_workspace.PropertyChanged -= Workspace_PropertyChanged;
			}

			// free unmanaged resources (unmanaged objects) and override finalizer
			// set large fields to null
			_disposedValue = true;
		}
	}

	// // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
	// ~SwitchWorkspaceCommand()
	// {
	//     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
	//     Dispose(disposing: false);
	// }

	/// <inheritdoc/>
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
