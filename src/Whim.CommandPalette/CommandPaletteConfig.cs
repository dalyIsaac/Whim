namespace Whim.CommandPalette;

/// <summary>
/// The configuration for the command palette plugin.
/// </summary>
public class CommandPaletteConfig
{
	/// <summary>
	/// The title of the command palette window.
	/// </summary>
	internal const string Title = "Whim Command Palette";

	/// <summary>
	/// Creates a new instance of <see cref="CommandPaletteConfig"/>.
	/// </summary>
	/// <param name="configContext"></param>
	public CommandPaletteConfig(IConfigContext configContext)
	{
		ActivationConfig = new MenuVariantConfig()
		{
			Commands = configContext.CommandManager,
			ConfirmButtonText = "Execute"
		};
	}

	/// <summary>
	/// The maximum height of the command palette window, as a percentage of the monitor height.
	/// </summary>
	public int MaxHeightPercent { get; set; } = 40;

	/// <summary>
	/// The maximum width of the command palette window, in pixels.
	/// </summary>
	public int MaxWidthPixels { get; set; } = 800;

	/// <summary>
	/// The top of the command palette window, as a percentage of the monitor height.
	/// </summary>
	public int YPositionPercent { get; set; } = 20;

	/// <summary>
	/// The configuration for command palette activation. <br/>
	///
	/// Defaults to a <see cref="MenuVariantConfig"/> where <see cref="MenuVariantConfig.Commands"/>
	/// are set to <see cref="IConfigContext.CommandManager"/>.
	/// </summary>
	public BaseVariantConfig ActivationConfig { get; set; }
}
