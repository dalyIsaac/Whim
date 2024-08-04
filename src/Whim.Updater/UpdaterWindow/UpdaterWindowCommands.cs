using System;

namespace Whim.Updater;

internal class SkipReleaseCommand(IUpdaterPlugin plugin, UpdaterWindowViewModel viewModel) : System.Windows.Input.ICommand
{
	private readonly IUpdaterPlugin _plugin = plugin;
	private readonly UpdaterWindowViewModel _viewModel = viewModel;

	public event EventHandler? CanExecuteChanged;

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

internal class InstallReleaseCommand(IUpdaterPlugin plugin, UpdaterWindowViewModel viewModel) : System.Windows.Input.ICommand
{
	private readonly IUpdaterPlugin _plugin = plugin;
	private readonly UpdaterWindowViewModel _viewModel = viewModel;

	public event EventHandler? CanExecuteChanged;

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

internal class CloseUpdaterWindowCommand(IUpdaterPlugin plugin) : System.Windows.Input.ICommand
{
	private readonly IUpdaterPlugin _plugin = plugin;

	public event EventHandler? CanExecuteChanged;

	public bool CanExecute(object? parameter) => true;

	public void Execute(object? parameter)
	{
		Logger.Debug("Closing updater window");
		_plugin.CloseUpdaterWindow();
	}
}
