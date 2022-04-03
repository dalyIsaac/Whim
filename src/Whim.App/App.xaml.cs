using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Whim.App;
/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
	private readonly IConfigContext _configContext;

	public App(IConfigContext configContext)
	{
		Logger.Debug("Starting application...");

		//Exit += Application_Exit;

		InitializeComponent();

		Logger.Debug("Initializing Whim");

		_configContext = configContext;
		_configContext.Initialize();
	}

	// Add when Windows App SDK supports the application exit event.
	// https://docs.microsoft.com/en-us/windows/winui/api/microsoft.ui.xaml.application?view=winui-3.0
	//private void Application_Exit(object sender, ExitEventArgs e)
	//{
	//	Logger.Information("Application exiting");
	//	_configContext.Quit();
	//	Logger.Information("Application disposed");
	//}
}
