# Plugins

Whim is build around plugins. Plugins are referenced using `#r` and `using` statements at the top of the config file. Each plugin generally has a `Config` class, and a `Plugin` class, and needs to be added to the <xref:Whim.IPluginManager>. For example:

```csharp
BarConfig barConfig = new(leftComponents, centerComponents, rightComponents);
BarPlugin barPlugin = new(context, barConfig);
context.PluginManager.AddPlugin(barPlugin);
```
