using System;

namespace Whim.Updater;

internal class SkipReleaseCommand : System.Windows.Input.ICommand
{
	private readonly IUpdaterPlugin _plugin;
	private readonly UpdaterWindowViewModel _viewModel;

	public event EventHandler? CanExecuteChanged;

	public SkipReleaseCommand(IUpdaterPlugin plugin, UpdaterWindowViewModel viewModel)
	{
		_plugin = plugin;
		_viewModel = viewModel;
	}

	public bool CanExecute(object? parameter) => true;

	public void Execute(object? parameter)
	{
		Logger.Debug("Skipping release");
		if (_viewModel.LastRelease != null)
		{
			_plugin.SkipRelease(_viewModel.LastRelease.Release);
		}
	}
}

internal class InstallReleaseCommand : System.Windows.Input.ICommand
{
	private readonly IUpdaterPlugin _plugin;
	private readonly UpdaterWindowViewModel _viewModel;

	public event EventHandler? CanExecuteChanged;

	public InstallReleaseCommand(IUpdaterPlugin plugin, UpdaterWindowViewModel viewModel)
	{
		_plugin = plugin;
		_viewModel = viewModel;
	}

	public bool CanExecute(object? parameter) => true;

	public void Execute(object? parameter)
	{
		Logger.Debug("Installing release");
		if (_viewModel.LastRelease != null)
		{
			_plugin.InstallRelease(_viewModel.LastRelease.Release);
			_plugin.CloseUpdaterWindow();
		}
	}
}

internal class CloseUpdaterWindowCommand : System.Windows.Input.ICommand
{
	private readonly IUpdaterPlugin _plugin;

	public event EventHandler? CanExecuteChanged;

	public CloseUpdaterWindowCommand(IUpdaterPlugin plugin)
	{
		_plugin = plugin;
	}

	public bool CanExecute(object? parameter) => true;

	public void Execute(object? parameter)
	{
		Logger.Debug("Closing updater window");
		_plugin.CloseUpdaterWindow();
	}
}
