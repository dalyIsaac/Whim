using System.Diagnostics.CodeAnalysis;
using Corvus.Json;
using Microsoft.UI.Xaml.Media;
using Whim.CommandPalette;
using Whim.FloatingWindow;
using Whim.FocusIndicator;
using Whim.Gaps;
using Whim.LayoutPreview;
using Whim.SliceLayout;
using Whim.TreeLayout;
using Whim.TreeLayout.CommandPalette;
using Whim.Updater;

namespace Whim.Yaml;

/// <summary>
/// Loads the SliceLayout plugin based on the provided context and schema.
/// /// Loads plugins from the YAML configuration.
/// </summary>
[SuppressMessage(
	"Reliability",
	"CA2000:Dispose objects before losing scope",
	Justification = "Items will be disposed by the context where appropriate."
)]
internal static class YamlPluginLoader
{
	public static void LoadPlugins(IContext ctx, Schema schema)
	{
		if (schema.Plugins is not { } plugins)
		{
			Logger.Debug("No plugins configuration found.");
			return;
		}

		LoadFocusIndicatorPlugin(ctx, plugins);
		LoadLayoutPreviewPlugin(ctx, plugins);
		LoadUpdaterPlugin(ctx, plugins);
		LoadSliceLayoutPlugin(ctx, plugins);
		LoadTreeLayoutPlugin(ctx, plugins);

		// Load the command palette after the TreeLayoutPlugin, as it has dependencies on the TreeLayout plugin.
		LoadCommandPalettePlugin(ctx, plugins);

		// Load the bar plugin after the TreeLayoutPlugin, as it has dependencies on the TreeLayout plugin.
		YamlBarPluginLoader.LoadBarPlugin(ctx, plugins);

		// It's important for FloatingWindowPlugin to immediately precede GapsPlugin in the plugin loading order.
		// This ensures that the GapsLayoutEngine will immediately contain the ProxyFloatingLayoutEngine, which
		// is required for preventing gaps for floating windows.
		LoadFloatingWindowPlugin(ctx, plugins);
		LoadGapsPlugin(ctx, plugins);
	}

	private static void LoadGapsPlugin(IContext ctx, Schema.PluginsEntity plugins)
	{
		if (plugins.Gaps is not { } gaps)
		{
			Logger.Debug("Gaps plugin configuration not found.");
			return;
		}

		if (!gaps.IsValid())
		{
			Logger.Debug("Gaps plugin is not valid.");
			return;
		}

		if (gaps.IsEnabled is { } isEnabled && !isEnabled)
		{
			Logger.Debug("Gaps plugin is not enabled.");
			return;
		}

		GapsConfig config = new();

		if (gaps.OuterGap is { } outerGap)
		{
			config.OuterGap = (int)outerGap;
		}

		if (gaps.InnerGap is { } innerGap)
		{
			config.InnerGap = (int)innerGap;
		}

		if (gaps.DefaultOuterDelta is { } defaultOuterDelta)
		{
			config.DefaultOuterDelta = (int)defaultOuterDelta;
		}

		if (gaps.DefaultInnerDelta is { } defaultInnerDelta)
		{
			config.DefaultInnerDelta = (int)defaultInnerDelta;
		}

		ctx.PluginManager.AddPlugin(new GapsPlugin(ctx, config));
	}

	private static void LoadCommandPalettePlugin(IContext ctx, Schema.PluginsEntity plugins)
	{
		if (plugins.CommandPalette is not { } commandPalette)
		{
			Logger.Debug("CommandPalette plugin configuration not found.");
			return;
		}

		if (!commandPalette.IsValid())
		{
			Logger.Debug("CommandPalette plugin is not valid.");
			return;
		}

		if (commandPalette.IsEnabled is { } isEnabled && !isEnabled)
		{
			Logger.Debug("CommandPalette plugin is not enabled.");
			return;
		}

		CommandPaletteConfig config = new(ctx);

		if (commandPalette.MaxHeightPercent is { } maxHeightPercent)
		{
			config.MaxHeightPercent = (int)maxHeightPercent;
		}

		if (commandPalette.MaxWidthPixels is { } maxWidthPixels)
		{
			config.MaxWidthPixels = (int)maxWidthPixels;
		}

		if (commandPalette.YPositionPercent is { } yPositionPercent)
		{
			config.YPositionPercent = (int)yPositionPercent;
		}

		if (commandPalette.Backdrop is { } backdrop)
		{
			config.Backdrop = YamlLoaderUtils.ParseWindowBackdropConfig(backdrop);
		}

		CommandPalettePlugin commandPalettePlugin = new(ctx, config);
		ctx.PluginManager.AddPlugin(commandPalettePlugin);

		// Load the TreeLayoutCommandPalettePlugin if the TreeLayoutPlugin is loaded.
		if (
			ctx.PluginManager.LoadedPlugins.FirstOrDefault(p => p.Name == "whim.tree_layout")
				is TreeLayoutPlugin treeLayoutPlugin
			&& ctx.PluginManager.LoadedPlugins.FirstOrDefault(p => p.Name == "whim.whim.tree_layout.command_palette")
				is null
		)
		{
			ctx.PluginManager.AddPlugin(
				new TreeLayoutCommandPalettePlugin(ctx, treeLayoutPlugin, commandPalettePlugin)
			);
		}
	}

	private static void LoadFloatingWindowPlugin(IContext ctx, Schema.PluginsEntity plugins)
	{
		if (plugins.FloatingWindow is not { } floatingWindow)
		{
			Logger.Debug("FloatingWindow plugin configuration not found.");
			return;
		}

		if (!floatingWindow.IsValid())
		{
			Logger.Debug("FloatingWindow plugin is not valid.");
			return;
		}

		if (floatingWindow.IsEnabled is { } isEnabled && !isEnabled)
		{
			Logger.Debug("FloatingWindow plugin is not enabled.");
			return;
		}

		ctx.PluginManager.AddPlugin(new FloatingWindowPlugin(ctx));
	}

	private static void LoadFocusIndicatorPlugin(IContext ctx, Schema.PluginsEntity plugins)
	{
		if (plugins.FocusIndicator is not { } focusIndicator)
		{
			Logger.Debug("FocusIndicator plugin configuration not found.");
			return;
		}

		if (!focusIndicator.IsValid())
		{
			Logger.Debug("FocusIndicator plugin is not valid.");
			return;
		}

		if (focusIndicator.IsEnabled is { } isEnabled && !isEnabled)
		{
			Logger.Debug("FocusIndicator plugin is not enabled.");
			return;
		}

		FocusIndicatorConfig config = new();

		if (focusIndicator.Color is { } color)
		{
			string colorStr = (string)color;
			var rawColor = colorStr.ParseColor();
			var winUiColor = Windows.UI.Color.FromArgb(rawColor.A, rawColor.R, rawColor.G, rawColor.B);

			config.Color = new SolidColorBrush(winUiColor);
		}

		if (focusIndicator.BorderSize is { } borderSize)
		{
			config.BorderSize = (int)borderSize;
		}

		if (focusIndicator.IsFadeEnabled is { } isFadeEnabled)
		{
			config.FadeEnabled = isFadeEnabled;
		}

		if (focusIndicator.FadeTimeout is { } fadeTimeout)
		{
			config.FadeTimeout = TimeSpan.FromSeconds((int)fadeTimeout);
		}

		ctx.PluginManager.AddPlugin(new FocusIndicatorPlugin(ctx, config));
	}

	private static void LoadLayoutPreviewPlugin(IContext ctx, Schema.PluginsEntity plugins)
	{
		if (plugins.LayoutPreview is not { } layoutPreview)
		{
			Logger.Debug("LayoutPreview plugin configuration not found.");
			return;
		}

		if (!layoutPreview.IsValid())
		{
			Logger.Debug("LayoutPreview plugin is not valid.");
			return;
		}

		if (layoutPreview.IsEnabled is { } isEnabled && !isEnabled)
		{
			Logger.Debug("LayoutPreview plugin is not enabled.");
			return;
		}

		ctx.PluginManager.AddPlugin(new LayoutPreviewPlugin(ctx));
	}

	private static void LoadUpdaterPlugin(IContext ctx, Schema.PluginsEntity plugins)
	{
		if (plugins.Updater is not { } updater)
		{
			Logger.Debug("Updater plugin configuration not found.");
			return;
		}

		if (!updater.IsValid())
		{
			Logger.Debug("Updater plugin is not valid.");
			return;
		}

		if (updater.IsEnabled is { } isEnabled && !isEnabled)
		{
			Logger.Debug("Updater plugin is not enabled.");
			return;
		}

		UpdaterConfig config = new();

		if (updater.UpdateFrequency is { } updateFrequency)
		{
			string updateFrequencyStr = ((string)updateFrequency).Capitalize();
			config.UpdateFrequency = Enum.TryParse<UpdateFrequency>(updateFrequencyStr, out var frequency)
				? frequency
				: UpdateFrequency.Never;
		}

		if (updater.ReleaseChannel is { } releaseChannel)
		{
			string releaseChannelStr = ((string)releaseChannel).Capitalize();
			config.ReleaseChannel = Enum.TryParse<ReleaseChannel>(releaseChannelStr, out var channel)
				? channel
				: ReleaseChannel.Stable;
		}

		ctx.PluginManager.AddPlugin(new UpdaterPlugin(ctx, config));
	}

	private static void LoadSliceLayoutPlugin(IContext ctx, Schema.PluginsEntity plugins)
	{
		if (plugins.SliceLayout is not { } sliceLayout)
		{
			Logger.Debug("SliceLayout plugin configuration not found.");
			return;
		}

		if (!sliceLayout.IsValid())
		{
			Logger.Debug("SliceLayout plugin is not valid.");
			return;
		}

		if (sliceLayout.IsEnabled is { } isEnabled && !isEnabled)
		{
			Logger.Debug("SliceLayout plugin is not enabled.");
			return;
		}

		ctx.PluginManager.AddPlugin(new SliceLayoutPlugin(ctx));
	}

	private static void LoadTreeLayoutPlugin(IContext ctx, Schema.PluginsEntity plugins)
	{
		if (plugins.TreeLayout is not { } treeLayout)
		{
			Logger.Debug("TreeLayout plugin configuration not found.");
			return;
		}

		if (!treeLayout.IsValid())
		{
			Logger.Debug("TreeLayout plugin is not valid.");
			return;
		}

		if (treeLayout.IsEnabled is { } isEnabled && !isEnabled)
		{
			Logger.Debug("TreeLayout plugin is not enabled.");
			return;
		}

		ctx.PluginManager.AddPlugin(new TreeLayoutPlugin(ctx));
	}
}
