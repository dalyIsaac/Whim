using Microsoft.UI.Xaml;
using System;

namespace Whim.Runner;

/// <summary>
/// Exposes the exception encountered during startup to the user.
/// </summary>
public sealed partial class StartupExceptionWindow : Microsoft.UI.Xaml.Window
{
	public string Exception { get; }

	/// <summary>
	/// Exposes the exception encountered during startup to the user.
	/// </summary>
	/// <param name="exception"></param>
	public StartupExceptionWindow(Exception exception)
	{
		InitializeComponent();
		Title = "Whim Startup Error";

		Exception = exception.ToString();
	}

	private void Quit_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}
}
