using System;

namespace Whim.Bar;

/// <summary>
/// Command to switch to the next layout engine.
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="NextLayoutEngineCommand"/>.
/// </remarks>
/// <param name="context"></param>
/// <param name="viewModel"></param>
internal class NextLayoutEngineCommand(IContext context, ActiveLayoutWidgetViewModel viewModel) : System.Windows.Input.ICommand
{
	private readonly IContext _context = context;
	private readonly ActiveLayoutWidgetViewModel _viewModel = viewModel;

	/// <inheritdoc/>
	public event EventHandler? CanExecuteChanged { add { } remove { } }

	/// <inheritdoc/>
	public bool CanExecute(object? parameter) => true;

	/// <inheritdoc/>
	public void Execute(object? parameter)
	{
		Logger.Debug("Switching to next layout engine");
		if (_context.Butler.Pantry.GetWorkspaceForMonitor(_viewModel.Monitor) is IWorkspace workspace)
		{
			workspace.CycleLayoutEngine(false);
		}
	}
}
