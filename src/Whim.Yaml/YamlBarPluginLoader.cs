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
	public static void LoadBarPlugin(IContext ctx, Schema.PluginsEntity plugins)
	{
		if (plugins.Bar is not { } bar)
		{
			Logger.Debug("Bar plugin configuration not found.");
			return;
		}

		if (!bar.IsValid())
		{
			Logger.Debug("Bar plugin is not valid.");
			return;
		}

		if (bar.IsEnabled is { } isEnabled && !isEnabled)
		{
			Logger.Debug("Bar plugin is not enabled.");
			return;
		}

		List<BarComponent> leftComponents = GetBarComponents(ctx, bar.LeftComponents);
		List<BarComponent> centerComponents = GetBarComponents(ctx, bar.CenterComponents);
		List<BarComponent> rightComponents = GetBarComponents(ctx, bar.RightComponents);

		BarConfig config = new(leftComponents, centerComponents, rightComponents);

		if (bar.Height is { } height)
		{
			config.Height = (int)height;
		}

		if (bar.Backdrop is { } backdrop)
		{
			config.Backdrop = YamlLoaderUtils.ParseWindowBackdropConfig(backdrop);
		}

		ctx.PluginManager.AddPlugin(new BarPlugin(ctx, config));
	}

	private static List<BarComponent> GetBarComponents(IContext ctx, Schema.SchemaRequiredEntries? componentsWrapper)
	{
		if (componentsWrapper is not { } components)
		{
			return [];
		}

		List<BarComponent> barComponents = [];

		foreach (var entry in components.Entries)
		{
			entry.Match<object?>(
				(in Schema.SchemaRequiredType4 widget) =>
				{
					barComponents.Add(CreateActiveLayoutBarWidget(ctx, widget));
					return null;
				},
				(in Schema.SchemaRequiredType5 widget) =>
				{
					barComponents.Add(CreateBatteryBarWidget(ctx, widget));
					return null;
				},
				(in Schema.SchemaRequiredType6 widget) =>
				{
					barComponents.Add(CreateDateTimeBarWidget(ctx, widget));
					return null;
				},
				(in Schema.SchemaRequiredType7 widget) =>
				{
					barComponents.Add(CreateFocusedWindowBarWidget(ctx, widget));
					return null;
				},
				(in Schema.SchemaRequiredType9 widget) =>
				{
					barComponents.Add(CreateWorkspaceBarWidget(ctx, widget));
					return null;
				},
				(in Schema.SchemaRequiredType8 widget) =>
				{
					barComponents.Add(CreateTreeLayoutEngineBarWidget(ctx, widget));
					return null;
				},
				(in Schema.SchemaRequiredType3 _) =>
				{
					return null;
				}
			);
		}

		return barComponents;
	}

	private static BarComponent CreateActiveLayoutBarWidget(IContext ctx, Schema.SchemaRequiredType4 widget)
	{
		return ActiveLayoutWidget.CreateComponent();
	}

	private static BarComponent CreateBatteryBarWidget(IContext ctx, Schema.SchemaRequiredType5 widget)
	{
		return BatteryWidget.CreateComponent();
	}

	private static BarComponent CreateDateTimeBarWidget(IContext ctx, Schema.SchemaRequiredType6 widget)
	{
		int interval = widget.Interval is { } i ? (int)i : 1000;
		string format = widget.Format is { } f ? (string)f : "yyyy-MM-dd HH:mm:ss";

		return DateTimeWidget.CreateComponent(interval, format);
	}

	private static BarComponent CreateFocusedWindowBarWidget(IContext ctx, Schema.SchemaRequiredType7 widget)
	{
		bool shortenTitle = widget.ShortenTitle is { } st && st;
		Func<IWindow, string> getTitle = shortenTitle
			? FocusedWindowWidget.GetShortTitle
			: FocusedWindowWidget.GetTitle;

		return FocusedWindowWidget.CreateComponent(getTitle);
	}

	private static BarComponent CreateWorkspaceBarWidget(IContext ctx, Schema.SchemaRequiredType9 widget)
	{
		return WorkspaceWidget.CreateComponent();
	}

	private static BarComponent CreateTreeLayoutEngineBarWidget(IContext ctx, Schema.SchemaRequiredType8 widget)
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
