using Microsoft.UI.Xaml;

namespace Whim.Runner;

/// <summary>
/// Exposes the exception encountered during startup to the user.
/// </summary>
public sealed partial class StartupExceptionWindow : Microsoft.UI.Xaml.Window
{
	private readonly App _app;

	/// <summary>
	/// The exception message.
	/// </summary>
	public string Message { get; }

	/// <summary>
	/// Exposes the exception encountered during startup to the user.
	/// </summary>
	/// <param name="app"></param>
	/// <param name="exitEventArgs"></param>
	public StartupExceptionWindow(App app, ExitEventArgs exitEventArgs)
	{
		InitializeComponent();
		_app = app;
		Title = "Whim Startup Error";

		Message = exitEventArgs.Message ?? "Unknown error occurred";

		this.SetSystemBackdrop();
	}

	private void Quit_Click(object sender, RoutedEventArgs e)
	{
		Close();
		_app.Exit();
	}
}
