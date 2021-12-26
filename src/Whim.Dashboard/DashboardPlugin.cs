
namespace Whim.Dashboard;

public class DashboardPlugin : IPlugin
{
	private readonly IConfigContext _configContext;
	private MainWindow? _mainWindow;

	public DashboardPlugin(IConfigContext configContext)
	{
		_configContext = configContext;
		_configContext.Shutdown += ConfigContext_Shutdown;
	}

	public void Initialize()
	{
		_mainWindow = new MainWindow(_configContext);
		_mainWindow.Show();
	}

	private void ConfigContext_Shutdown(object? sender, ShutdownEventArgs e)
	{
		_mainWindow?.Dispose();
	}

	public void Show()
	{
		_mainWindow?.Show();
	}

	public void Hide()
	{
		_mainWindow?.Hide();
	}

	public void Toggle()
	{
		if (_mainWindow?.IsVisible == true)
		{
			Hide();
		}
		else
		{
			Show();
		}
	}
}
