namespace Whim.FocusIndicator;

/// <summary>
/// The commands for the focus indicator plugin.
/// </summary>
public class FocusIndicatorCommands : PluginCommands
{
	private readonly IFocusIndicatorPlugin _focusIndicatorPlugin;

	/// <summary>
	/// Creates a new instance of the focus indicator commands.
	/// </summary>
	public FocusIndicatorCommands(IFocusIndicatorPlugin focusIndicatorPlugin)
		: base(focusIndicatorPlugin.Name)
	{
		_focusIndicatorPlugin = focusIndicatorPlugin;

		Add(identifier: "show", title: "Show focus indicator", () => _focusIndicatorPlugin.Show())
			.Add(identifier: "toggle", title: "Toggle focus indicator", _focusIndicatorPlugin.Toggle)
			.Add(
				identifier: "toggle_fade",
				title: "Toggle whether the focus indicator fades",
				_focusIndicatorPlugin.ToggleFade
			)
			.Add(
				identifier: "toggle_enabled",
				title: "Toggle whether the focus indicator is enabled",
				_focusIndicatorPlugin.ToggleEnabled
			);
	}
}
