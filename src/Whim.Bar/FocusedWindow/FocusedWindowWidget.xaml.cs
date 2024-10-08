using System;
using Microsoft.UI.Xaml.Controls;

namespace Whim.Bar;

/// <summary>
/// Interaction logic for FocusedWindowWidget.xaml
/// </summary>
public partial class FocusedWindowWidget : UserControl
{
	private static readonly char[] _titleSeparators = ['-', '—', '|'];

	/// <summary>
	/// The focused window view model.
	/// </summary>
	internal FocusedWindowWidgetViewModel ViewModel { get; private set; }

	internal FocusedWindowWidget(IContext context, IMonitor monitor, Func<IWindow, string> getTitle)
	{
		ViewModel = new FocusedWindowWidgetViewModel(context, monitor, getTitle);
		UIElementExtensions.InitializeComponent(this, "Whim.Bar", "FocusedWindow/FocusedWindowWidget");
	}

	/// <summary>
	/// Create the focused window widget bar component.
	/// </summary>
	/// <param name="getTitle">
	/// The function to get the title of the window. Defaults to <see cref="GetTitle(IWindow)"/>.
	/// </param>
	public static BarComponent CreateComponent(Func<IWindow, string>? getTitle = null) =>
		new FocusedWindowComponent(getTitle);

	/// <summary>
	/// Gets the full title of the window.
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	public static string GetTitle(IWindow window) => window.Title;

	/// <summary>
	/// Tries to split the title of the window and return the last part.
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	public static string GetShortTitle(IWindow window)
	{
		string[] parts = window.Title.Split(_titleSeparators, StringSplitOptions.RemoveEmptyEntries);
		return parts.Length > 0 ? parts[0].Trim() : window.Title;
	}

	/// <summary>
	/// Returns the process name of the window - e.g., <c>SnippingTool</c>.
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	public static string? GetProcessName(IWindow window) => window.ProcessFileName?.Replace(".exe", "");
}

/// <summary>
/// The bar component for the focused window widget.
/// </summary>
/// <param name="GetTitle">
/// The function to get the title of the window. Defaults to <see cref="FocusedWindowWidget.GetTitle(IWindow)"/>.
/// </param>
public record FocusedWindowComponent(Func<IWindow, string>? GetTitle) : BarComponent
{
	/// <inheritdoc/>
	public override UserControl CreateWidget(IContext context, IMonitor monitor, Microsoft.UI.Xaml.Window window) =>
		new FocusedWindowWidget(context, monitor, GetTitle ?? FocusedWindowWidget.GetTitle);
}
