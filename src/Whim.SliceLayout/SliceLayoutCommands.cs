namespace Whim.SliceLayout;

/// <summary>
/// Commands for the <see cref="SliceLayoutPlugin"/>.
/// </summary>
public class SliceLayoutCommands : PluginCommands
{
	private readonly ISliceLayoutPlugin _sliceLayoutPlugin;

	// TODO: Document in README
	/// <summary>
	/// Creates a new instance of the slice layout commands.
	/// </summary>
	/// <param name="sliceLayoutPlugin"></param>
	public SliceLayoutCommands(ISliceLayoutPlugin sliceLayoutPlugin)
		: base(sliceLayoutPlugin.Name)
	{
		_sliceLayoutPlugin = sliceLayoutPlugin;

		_ = Add(
				identifier: "set_insertion_type.swap",
				title: "Set slice insertion type to swap",
				callback: () => _sliceLayoutPlugin.WindowInsertionType = WindowInsertionType.Swap
			)
			.Add(
				identifier: "set_insertion_type.rotate",
				title: "Set slice insertion type to rotate",
				callback: () => _sliceLayoutPlugin.WindowInsertionType = WindowInsertionType.Rotate
			)
			.Add(
				identifier: "window.promote",
				title: "Promote window in stack",
				callback: () => _sliceLayoutPlugin.PromoteWindowInStack()
			)
			.Add(
				identifier: "window.demote",
				title: "Demote window in stack",
				callback: () => _sliceLayoutPlugin.DemoteWindowInStack()
			)
			.Add(
				identifier: "focus.promote",
				title: "Promote focus in stack",
				callback: () => _sliceLayoutPlugin.PromoteFocusInStack()
			)
			.Add(
				identifier: "focus.demote",
				title: "Demote focus in stack",
				callback: () => _sliceLayoutPlugin.DemoteFocusInStack()
			);
	}
}
