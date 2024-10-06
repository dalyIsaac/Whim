# Updater Plugin

The updater will show a notification when a new version is available. Clicking on the notification will show the changelog for the delta between the current version and the latest version.
When an update is available, the notification will provide options to:

- Open the `Releases` page on GitHub in the default browser.
- Skip the version (i.e., do not show the notification for this version again).
- Defer the update (i.e., show the notification again when the check is run next).

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

[!INCLUDE [Commands](../../_common/plugins/updater.md)]
