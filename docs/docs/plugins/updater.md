# Updater Plugin

The <xref:Whim.Updater.UpdaterPlugin> plugin is in `alpha` (especially as Whim hasn't started releasing non-`alpha` builds). If the updater fails, you can manually update Whim by downloading the latest release from the [releases page](https://github.com/dalyIsaac/Whim/releases).

The updater will show a notification when a new version is available. Clicking on the notification will show the changelog for the delta between the current version and the latest version.

The <xref:Whim.Updater.UpdaterConfig> supports specifying the <xref:Whim.Updater.ReleaseChannel> and <xref:Whim.Updater.UpdateFrequency>.

## Example Config

```csharp
#r "WHIM_PATH\whim.dll"
#r "WHIM_PATH\plugins\Whim.Updater\Whim.Updater.dll"

using Whim;
using Whim.Updater;

void DoConfig(IContext context)
{
  // ...

  UpdaterConfig updaterConfig = new() { ReleaseChannel = ReleaseChannel.Alpha };
  UpdaterPlugin updaterPlugin = new(context, updaterConfig);
  context.PluginManager.AddPlugin(updaterPlugin);

  // ...
}

return DoConfig;
```

## Commands

| Identifier           | Title             | Keybind            |
| -------------------- | ----------------- | ------------------ |
| `whim.updater.check` | Check for updates | No default keybind |
