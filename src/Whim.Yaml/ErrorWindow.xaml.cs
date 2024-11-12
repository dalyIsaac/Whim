using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;

namespace Whim.Yaml;

/// <summary>
/// Exposes YAML errors encountered during parsing to the user.
/// </summary>
public sealed partial class ErrorWindow : Microsoft.UI.Xaml.Window, IDisposable, INotifyPropertyChanged
{
	private readonly IContext _ctx;
	private readonly WindowBackdropController _backdropController;
	private string _message = string.Empty;

	/// <summary>
	/// The errors.
	/// </summary>
	public string Message
	{
		get => _message;
		private set
		{
			if (_message != value)
			{
				_message = value;
				OnPropertyChanged();
			}
		}
	}

	/// <inheritdoc/>
	public event PropertyChangedEventHandler? PropertyChanged;

	private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

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

		ctx.FilterManager.AddTitleFilter(this.Title);
		Message = errors;

		_backdropController = new(this, new WindowBackdropConfig(BackdropType.Mica, AlwaysShowBackdrop: false));
	}

	private void Ignore_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}

	private void Quit_Click(object sender, RoutedEventArgs e)
	{
		Close();
		_ctx.Exit(new ExitEventArgs() { Reason = ExitReason.User, Message = Message });
	}

	/// <summary>
	/// Appends additional text to the error message.
	/// </summary>
	/// <param name="text">The text to append.</param>
	public void AppendMessage(string text)
	{
		Message = Message + Environment.NewLine + text;
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		_backdropController.Dispose();
	}
}
