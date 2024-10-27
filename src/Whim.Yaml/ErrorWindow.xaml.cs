using System;
using Microsoft.UI.Xaml;

namespace Whim.Yaml;

/// <summary>
/// Exposes YAML errors encountered during parsing to the user.
/// </summary>
public sealed partial class ErrorWindow : Microsoft.UI.Xaml.Window, IDisposable
{
	private readonly IContext _ctx;
	private readonly WindowBackdropController _backdropController;

	/// <summary>
	/// The errors.
	/// </summary>
	public string Message { get; }

	/// <summary>
	/// Exposes YAML errors encountered during parsing to the user.
	/// </summary>
	/// <param name="ctx">The context.</param>
	/// <param name="errors">The YAML errors.</param>
	public ErrorWindow(IContext ctx, string errors)
	{
		UIElementExtensions.InitializeComponent(this, "Whim.Yaml", "ErrorWindow");

		_ctx = ctx;

		Title = "Whim YAML/JSON error";
		Message = errors;

		_backdropController = new(this, new WindowBackdropConfig(BackdropType.Mica, AlwaysShowBackdrop: false));
	}

	private void Quit_Click(object sender, RoutedEventArgs e)
	{
		Close();
		_ctx.Exit(new ExitEventArgs() { Reason = ExitReason.Error, Message = Message });
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		_backdropController.Dispose();
	}
}
