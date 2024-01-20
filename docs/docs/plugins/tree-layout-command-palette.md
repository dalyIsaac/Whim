# Tree Layout Command Palette Plugin

<xref:Whim.TreeLayout.CommandPalette> contains commands to interact with the tree layout via the command palette. The commands are automatically loaded in by <xref:Whim.IPluginManager.AddPlugin\*>.

## Example Config

```csharp
#r "WHIM_PATH\whim.dll"
#r "WHIM_PATH\plugins\Whim.CommandPalette\Whim.CommandPalette.dll"
#r "WHIM_PATH\plugins\Whim.TreeLayout\Whim.TreeLayout.dll"
#r "WHIM_PATH\plugins\Whim.TreeLayout.CommandPalette\Whim.TreeLayout.CommandPalette.dll"

using Whim;
using Whim.CommandPalette;
using Whim.TreeLayout;
using Whim.TreeLayout.CommandPalette;

void DoConfig(IContext context)
{
  // ...

  CommandPaletteConfig commandPaletteConfig = new(context);
  CommandPalettePlugin commandPalettePlugin = new(context, commandPaletteConfig);
  context.PluginManager.AddPlugin(commandPalettePlugin);

  // Tree layout plugin
  TreeLayoutPlugin treeLayoutPlugin = new(context);
  context.PluginManager.AddPlugin(treeLayoutPlugin);

  // ...
}

return DoConfig;
```

## Commands

| Identifier                                       | Title                                                                               | Keybind            |
| ------------------------------------------------ | ----------------------------------------------------------------------------------- | ------------------ |
| `whim.tree_layout.command_palette.set_direction` | Opens a new command palette where users can select the direction to add windows in. | No default keybind |
