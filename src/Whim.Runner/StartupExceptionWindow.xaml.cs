using Microsoft.UI.Xaml;

namespace Whim.Runner;

/// <summary>
/// Exposes the exception encountered during startup to the user.
/// </summary>
public sealed partial class StartupExceptionWindow : Microsoft.UI.Xaml.Window
{
	/// <summary>
	/// The exception message.
	/// </summary>
	public string Message { get; }

	/// <summary>
	/// Exposes the exception encountered during startup to the user.
	/// </summary>
	/// <param name="exitEventArgs"></param>
	public StartupExceptionWindow(ExitEventArgs exitEventArgs)
	{
		InitializeComponent();
		Title = "Whim Startup Error";

		Message = exitEventArgs.Message ?? "Unknown error occured";
	}

	private void Quit_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}
}
