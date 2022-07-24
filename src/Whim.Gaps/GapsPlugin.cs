using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim.Gaps;

/// <summary>
/// GapsPlugin is a plugin to add gaps between windows in the layout.
/// </summary>
public class GapsPlugin : IPlugin
{
	private readonly IConfigContext _configContext;
	private readonly GapsConfig _gapsConfig;

	/// <summary>
	/// Creates a new instance of the gaps plugin.
	/// </summary>
	/// <param name="configContext"></param>
	/// <param name="gapsConfig"></param>
	public GapsPlugin(IConfigContext configContext, GapsConfig gapsConfig)
	{
		_configContext = configContext;
		_gapsConfig = gapsConfig;
	}

	/// <inheritdoc />
	public void PreInitialize()
	{
		_configContext.WorkspaceManager.AddProxyLayoutEngine(layout => new GapsLayoutEngine(_gapsConfig, layout));
	}

	/// <inheritdoc />
	public void PostInitialize() { }

	/// <summary>
	/// Update the gap between the parent layout engine and the area where windows are placed by
	/// the <paramref name="delta"/>.
	/// </summary>
	/// <param name="delta"></param>
	public void UpdateOuterGap(int delta)
	{
		_gapsConfig.OuterGap += delta;
		_configContext.WorkspaceManager.LayoutAllActiveWorkspaces();
	}

	/// <summary>
	/// Update the gap between windows by the <paramref name="delta"/>.
	/// </summary>
	/// <param name="delta"></param>
	public void UpdateInnerGap(int delta)
	{
		_gapsConfig.InnerGap += delta;
		_configContext.WorkspaceManager.LayoutAllActiveWorkspaces();
	}

	/// <inheritdoc />
	public (ICommand, IKeybind?)[] GetCommands() => new (ICommand, IKeybind?)[]
	{
		// Increase outer gap
		(
			new Command(
				identifier: "gaps.outer.increase",
				title: "Increase outer gap",
				callback: () => UpdateOuterGap(_gapsConfig.DefaultOuterDelta)
			),
			new Keybind(DefaultCommands.WinCtrlShift, VIRTUAL_KEY.VK_L)
		),

		// Decrease outer gap
		(
			new Command(
				identifier: "gaps.outer.decrease",
				title: "Decrease outer gap",
				callback: () => UpdateOuterGap(-_gapsConfig.DefaultOuterDelta)
			),
			new Keybind(DefaultCommands.WinCtrlShift, VIRTUAL_KEY.VK_H)
		),

		// Increase inner gap
		(
			new Command(
				identifier: "gaps.inner.increase",
				title: "Increase inner gap",
				callback: () => UpdateInnerGap(_gapsConfig.DefaultInnerDelta)
			),
			new Keybind(DefaultCommands.WinCtrlShift, VIRTUAL_KEY.VK_K)
		),

		// Decrease inner gap
		(
			new Command(
				identifier: "gaps.inner.decrease",
				title: "Decrease inner gap",
				callback: () => UpdateInnerGap(-_gapsConfig.DefaultInnerDelta)
			),
			new Keybind(DefaultCommands.WinCtrlShift, VIRTUAL_KEY.VK_J)
		)
	};
}
