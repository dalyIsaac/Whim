using System.Diagnostics.CodeAnalysis;
using Corvus.Json;
using Whim.Bar;
using Whim.TreeLayout;
using Whim.TreeLayout.Bar;

namespace Whim.Yaml;

internal static class YamlBarPluginLoader
{
	[SuppressMessage(
		"Reliability",
		"CA2000:Dispose objects before losing scope",
		Justification = "Items will be disposed by the context where appropriate."
	)]
	public static void LoadBarPlugin(IContext ctx, Schema schema)
	{
		var bar = schema.Plugins.Bar;

		if (!bar.IsValid())
		{
			Logger.Debug("Bar plugin is not valid.");
			return;
		}

		if (bar.IsEnabled.AsOptional() is { } isEnabled && !isEnabled)
		{
			Logger.Debug("Bar plugin is not enabled.");
			return;
		}

		List<BarComponent> leftComponents = GetBarComponents(ctx, bar.LeftComponents);
		List<BarComponent> centerComponents = GetBarComponents(ctx, bar.CenterComponents);
		List<BarComponent> rightComponents = GetBarComponents(ctx, bar.RightComponents);

		BarConfig config = new(leftComponents, centerComponents, rightComponents);

		if (bar.Height.AsOptional() is { } height)
		{
			config.Height = (int)height;
		}

		if (bar.Backdrop.AsOptional() is { } backdrop)
		{
			config.Backdrop = YamlLoaderUtils.ParseWindowBackdropConfig(backdrop);
		}

		ctx.PluginManager.AddPlugin(new BarPlugin(ctx, config));
	}

	private static List<BarComponent> GetBarComponents(IContext ctx, Schema.DefsRequiredEntries componentsWrapper)
	{
		if (componentsWrapper.Entries.AsOptional() is not { } entries)
		{
			return [];
		}

		List<BarComponent> components = [];

		foreach (var entry in entries)
		{
			entry.Match<object?>(
				(in Schema.AWidgetToDisplayTheActiveLayout widget) =>
				{
					components.Add(CreateActiveLayoutBarWidget(ctx, widget));
					return null;
				},
				(in Schema.AWidgetToDisplayTheBatteryStatus widget) =>
				{
					components.Add(CreateBatteryBarWidget(ctx, widget));
					return null;
				},
				(in Schema.AWidgetToDisplayTheDateAndTime widget) =>
				{
					components.Add(CreateDateTimeBarWidget(ctx, widget));
					return null;
				},
				(in Schema.AWidgetToDisplayTheFocusedWindow widget) =>
				{
					components.Add(CreateFocusedWindowBarWidget(ctx, widget));
					return null;
				},
				(in Schema.AWidgetToDisplayTheWorkspace widget) =>
				{
					components.Add(CreateWorkspaceBarWidget(ctx, widget));
					return null;
				},
				(in Schema.DefsRequiredType3 widget) =>
				{
					components.Add(CreateTreeLayoutEngineBarWidget(ctx, widget));
					return null;
				},
				(in Schema.DefsRequiredType2 _) =>
				{
					return null;
				}
			);
		}

		return components;
	}

	private static BarComponent CreateActiveLayoutBarWidget(IContext ctx, Schema.AWidgetToDisplayTheActiveLayout widget)
	{
		return ActiveLayoutWidget.CreateComponent();
	}

	private static BarComponent CreateBatteryBarWidget(IContext ctx, Schema.AWidgetToDisplayTheBatteryStatus widget)
	{
		return BatteryWidget.CreateComponent();
	}

	private static BarComponent CreateDateTimeBarWidget(IContext ctx, Schema.AWidgetToDisplayTheDateAndTime widget)
	{
		int interval = (int?)widget.Interval.AsOptional() ?? 1000;
		string format = (string?)widget.Format.AsOptional() ?? "yyyy-MM-dd HH:mm:ss";

		return DateTimeWidget.CreateComponent(interval, format);
	}

	private static BarComponent CreateFocusedWindowBarWidget(
		IContext ctx,
		Schema.AWidgetToDisplayTheFocusedWindow widget
	)
	{
		bool shortenTitle = widget.ShortenTitle.AsOptional() ?? false;
		Func<IWindow, string> getTitle = shortenTitle
			? FocusedWindowWidget.GetShortTitle
			: FocusedWindowWidget.GetTitle;

		return FocusedWindowWidget.CreateComponent(getTitle);
	}

	private static BarComponent CreateWorkspaceBarWidget(IContext ctx, Schema.AWidgetToDisplayTheWorkspace widget)
	{
		return WorkspaceWidget.CreateComponent();
	}

	private static BarComponent CreateTreeLayoutEngineBarWidget(IContext ctx, Schema.DefsRequiredType3 widget)
	{
		if (
			ctx.PluginManager.LoadedPlugins.FirstOrDefault(p => p.Name == "whim.tree_layout")
			is not TreeLayoutPlugin treeLayoutPlugin
		)
		{
			treeLayoutPlugin = new TreeLayoutPlugin(ctx);
			ctx.PluginManager.AddPlugin(treeLayoutPlugin);
		}

		if (
			ctx.PluginManager.LoadedPlugins.FirstOrDefault(p => p.Name == "whim.tree_layout.bar")
			is not TreeLayoutBarPlugin treeLayoutBarPlugin
		)
		{
			treeLayoutBarPlugin = new TreeLayoutBarPlugin(treeLayoutPlugin);
			ctx.PluginManager.AddPlugin(treeLayoutBarPlugin);
		}

		return treeLayoutBarPlugin.CreateComponent();
	}
}
