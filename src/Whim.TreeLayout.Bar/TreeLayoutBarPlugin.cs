using System;
using System.Collections.Generic;
using Whim.Bar;

namespace Whim.TreeLayout.Bar;

/// <summary>
/// This plugin contains the tree layout engine widget for the <see cref="IBarPlugin"/>.
/// </summary>
public class TreeLayoutBarPlugin : IPlugin
{
	/// <inheritdoc/>
	public string Name => "whim.tree_layout.bar";

	/// <inheritdoc/>
	public IEnumerable<CommandItem> Commands => Array.Empty<CommandItem>();

	/// <inheritdoc/>
	public void PreInitialize() { }

	/// <inheritdoc/>
	public void PostInitialize() { }

	/// <summary>
	/// Create the tree layout engine bar component.
	/// </summary>
	/// <param name="plugin"></param>
	/// <returns></returns>
	public static BarComponent CreateComponent(ITreeLayoutPlugin plugin)
	{
		return new BarComponent(
			(context, monitor, window) => new TreeLayoutEngineWidget(context, plugin, monitor, window)
		);
	}
}
