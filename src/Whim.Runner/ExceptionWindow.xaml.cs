using Microsoft.UI.Xaml;

namespace Whim.Runner;

/// <summary>
/// Exposes the exception encountered during startup to the user.
/// </summary>
public sealed partial class ExceptionWindow : Microsoft.UI.Xaml.Window
{
	private readonly App _app;
	private readonly WindowBackdropController _backdropController;

	/// <summary>
	/// The exception message.
	/// </summary>
	public string Message { get; }

	/// <summary>
	/// Exposes the exception encountered during startup to the user.
	/// </summary>
	/// <param name="app"></param>
	/// <param name="exitEventArgs"></param>
	public ExceptionWindow(App app, ExitEventArgs exitEventArgs)
	{
		InitializeComponent();
		_app = app;
		Title = "Whim Startup Error";

		Message = exitEventArgs.Message ?? "Unknown error occurred";

		_backdropController = new(this, new WindowBackdropConfig(BackdropType.Mica, AlwaysShowBackdrop: false));
	}

	private void Quit_Click(object sender, RoutedEventArgs e)
	{
		Close();
		_app.Exit();
	}
}
