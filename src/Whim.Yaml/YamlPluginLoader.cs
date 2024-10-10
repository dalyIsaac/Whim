using System.Diagnostics.CodeAnalysis;
using Corvus.Json;
using Microsoft.UI.Xaml.Media;
using Whim.CommandPalette;
using Whim.FocusIndicator;
using Whim.Gaps;
using Whim.LayoutPreview;
using Whim.SliceLayout;
using Whim.TreeLayout;
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
		LoadGapsPlugin(ctx, schema);
		LoadCommandPalettePlugin(ctx, schema);
		LoadFocusIndicatorPlugin(ctx, schema);
		LoadLayoutPreviewPlugin(ctx, schema);
		LoadUpdaterPlugin(ctx, schema);
		LoadSliceLayoutPlugin(ctx, schema);
		LoadTreeLayoutPlugin(ctx, schema);

		// Load the bar plugin last, since it has dependencies on the TreeLayout plugin.
		YamlBarPluginLoader.LoadBarPlugin(ctx, schema);
	}

	private static void LoadGapsPlugin(IContext ctx, Schema schema)
	{
		var gaps = schema.Plugins.Gaps;

		if (!gaps.IsValid())
		{
			Logger.Debug("Gaps plugin is not valid.");
			return;
		}

		if (gaps.IsEnabled.AsOptional() is { } isEnabled && !isEnabled)
		{
			Logger.Debug("Gaps plugin is not enabled.");
			return;
		}

		GapsConfig config = new();

		if (gaps.OuterGap.AsOptional() is { } outerGap)
		{
			config.OuterGap = (int)outerGap;
		}

		if (gaps.InnerGap.AsOptional() is { } innerGap)
		{
			config.InnerGap = (int)innerGap;
		}

		if (gaps.DefaultOuterDelta.AsOptional() is { } defaultOuterDelta)
		{
			config.DefaultOuterDelta = (int)defaultOuterDelta;
		}

		if (gaps.DefaultInnerDelta.AsOptional() is { } defaultInnerDelta)
		{
			config.DefaultInnerDelta = (int)defaultInnerDelta;
		}

		ctx.PluginManager.AddPlugin(new GapsPlugin(ctx, config));
	}

	private static void LoadCommandPalettePlugin(IContext ctx, Schema schema)
	{
		var commandPalette = schema.Plugins.CommandPalette;

		if (!commandPalette.IsValid())
		{
			Logger.Debug("CommandPalette plugin is not valid.");
			return;
		}

		if (commandPalette.IsEnabled.AsOptional() is { } isEnabled && !isEnabled)
		{
			Logger.Debug("CommandPalette plugin is not enabled.");
			return;
		}

		CommandPaletteConfig config = new(ctx);

		if (commandPalette.MaxHeightPercent.AsOptional() is { } maxHeightPercent)
		{
			config.MaxHeightPercent = (int)maxHeightPercent;
		}

		if (commandPalette.MaxWidthPixels.AsOptional() is { } maxWidthPixels)
		{
			config.MaxWidthPixels = (int)maxWidthPixels;
		}

		if (commandPalette.YPositionPercent.AsOptional() is { } yPositionPercent)
		{
			config.YPositionPercent = (int)yPositionPercent;
		}

		if (commandPalette.Backdrop.AsOptional() is { } backdrop)
		{
			config.Backdrop = YamlLoaderUtils.ParseWindowBackdropConfig(backdrop);
		}

		ctx.PluginManager.AddPlugin(new CommandPalettePlugin(ctx, config));
	}

	private static void LoadFocusIndicatorPlugin(IContext ctx, Schema schema)
	{
		var focusIndicator = schema.Plugins.FocusIndicator;

		if (!focusIndicator.IsValid())
		{
			Logger.Debug("FocusIndicator plugin is not valid.");
			return;
		}

		if (focusIndicator.IsEnabled.AsOptional() is { } isEnabled && !isEnabled)
		{
			Logger.Debug("FocusIndicator plugin is not enabled.");
			return;
		}

		FocusIndicatorConfig config = new();

		if (focusIndicator.Color.AsOptional() is { } color)
		{
			string colorStr = (string)color;
			var rawColor = colorStr.ParseColor();
			var winUiColor = Windows.UI.Color.FromArgb(rawColor.A, rawColor.R, rawColor.G, rawColor.B);

			config.Color = new SolidColorBrush(winUiColor);
		}

		if (focusIndicator.BorderSize.AsOptional() is { } borderSize)
		{
			config.BorderSize = (int)borderSize;
		}

		if (focusIndicator.IsFadeEnabled.AsOptional() is { } isFadeEnabled)
		{
			config.FadeEnabled = isFadeEnabled;
		}

		if (focusIndicator.FadeTimeout.AsOptional() is { } fadeTimeout)
		{
			config.FadeTimeout = TimeSpan.FromSeconds((int)fadeTimeout);
		}

		ctx.PluginManager.AddPlugin(new FocusIndicatorPlugin(ctx, config));
	}

	private static void LoadLayoutPreviewPlugin(IContext ctx, Schema schema)
	{
		var layoutPreview = schema.Plugins.LayoutPreview;

		if (!layoutPreview.IsValid())
		{
			Logger.Debug("LayoutPreview plugin is not valid.");
			return;
		}

		if (layoutPreview.IsEnabled.AsOptional() is { } isEnabled && !isEnabled)
		{
			Logger.Debug("LayoutPreview plugin is not enabled.");
			return;
		}

		ctx.PluginManager.AddPlugin(new LayoutPreviewPlugin(ctx));
	}

	private static void LoadUpdaterPlugin(IContext ctx, Schema schema)
	{
		var updater = schema.Plugins.Updater;

		if (!updater.IsValid())
		{
			Logger.Debug("Updater plugin is not valid.");
			return;
		}

		if (updater.IsEnabled.AsOptional() is { } isEnabled && !isEnabled)
		{
			Logger.Debug("Updater plugin is not enabled.");
			return;
		}

		UpdaterConfig config = new();

		if (updater.UpdateFrequency.AsOptional() is { } updateFrequency)
		{
			string updateFrequencyStr = ((string)updateFrequency).Capitalize();
			config.UpdateFrequency = Enum.TryParse<UpdateFrequency>(updateFrequencyStr, out var frequency)
				? frequency
				: UpdateFrequency.Never;
		}

		if (updater.ReleaseChannel.AsOptional() is { } releaseChannel)
		{
			string releaseChannelStr = ((string)releaseChannel).Capitalize();
			config.ReleaseChannel = Enum.TryParse<ReleaseChannel>(releaseChannelStr, out var channel)
				? channel
				: ReleaseChannel.Stable;
		}

		ctx.PluginManager.AddPlugin(new UpdaterPlugin(ctx, config));
	}

	private static void LoadSliceLayoutPlugin(IContext ctx, Schema schema)
	{
		if (!schema.Plugins.SliceLayout.IsValid())
		{
			Logger.Debug("SliceLayout plugin is not valid.");
			return;
		}

		if (schema.Plugins.SliceLayout.IsEnabled.AsOptional() is { } isEnabled && !isEnabled)
		{
			Logger.Debug("SliceLayout plugin is not enabled.");
			return;
		}

		ctx.PluginManager.AddPlugin(new SliceLayoutPlugin(ctx));
	}

	private static void LoadTreeLayoutPlugin(IContext ctx, Schema schema)
	{
		if (!schema.Plugins.TreeLayout.IsValid())
		{
			Logger.Debug("TreeLayout plugin is not valid.");
			return;
		}

		if (schema.Plugins.TreeLayout.IsEnabled.AsOptional() is { } isEnabled && !isEnabled)
		{
			Logger.Debug("TreeLayout plugin is not enabled.");
			return;
		}

		ctx.PluginManager.AddPlugin(new TreeLayoutPlugin(ctx));
	}
}
