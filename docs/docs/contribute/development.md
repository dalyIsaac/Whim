# Development

> [!NOTE]
> Make sure to read the [environment setup](../environment-setup.md) guide before continuing.

## Visual Studio

Visual Studio 2022 is the easiest way to get started with working on Whim. Check the following:

- `nuget.org` is added to the [Package Sources](https://learn.microsoft.com/en-us/nuget/consume-packages/install-use-packages-visual-studio#package-sources)
- Each project's platform matches the current target architecture.
- `Whim.Runner` is set as the startup project.
- The **Configuration Manager** is set to `Debug` and your target architecture is correct (e.g. `x64`).
- The **green Start arrow** is labeled `Whim.Runner (Unpackaged)`.

![Visual Studio setup](../../images/visual-studio-setup.gif)

**Recommended Extensions:**

- [CSharpier](https://marketplace.visualstudio.com/items?itemName=csharpier.CSharpier)
- [XAML Styler for Visual Studio 2022](https://marketplace.visualstudio.com/items?itemName=TeamXavalon.XAMLStyler2022)

> [!WARNING]
>
> Windows App SDK 1.4 introduced a bug which causes Visual Studio to crash Whim when debugging. Make sure to apply the workaround from <https://github.com/microsoft/microsoft-ui-xaml/issues/9008#issuecomment-1773734685/>.

## Visual Studio Code

The Whim repository includes a `.vscode` directory with a [`launch.json`](.vscode/launch.json) file. This file contains a `Launch Whim.Runner` configuration which can be used to debug Whim in Visual Studio Code. Unfortunately tests do not appear in Visual Studio Code's Test Explorer.

Check that `nuget.org` is added to the [Package Sources](https://learn.microsoft.com/en-us/nuget/consume-packages/install-use-packages-visual-studio#package-sources).

Tasks to build, test, and format XAML can be found in [`tasks.json`](.vscode/tasks.json).

To see the recommended extensions, open the Command Palette and run `Extensions: Show Recommended Extensions`.

## Unhandled Exception Handling

<xref:Whim.IContext> has an <Whim.IContext.UncaughtExceptionHandling> property to specify how to handle uncaught exceptions. When developing, it's recommended to set this to <Whim.UncaughtExceptionHandling.Shutdown> to shutdown Whim when an uncaught exception occurs. This will make it easier to debug the exception.

All uncaught exceptions will be logged as `Fatal`.

## Tests

Tests have not been written for all of Whim's code, but are encouraged. Tests have not been written for UI code-behind files, as I committed to xUnit before I realized that Windows App SDK isn't easily compatible with xUnit. I'm open to suggestions on how to test UI code-behind files.

## Updating `#r` directives

To use your existing configuration, make sure to update the `#r` directives to point to your newly compiled DLLs. For example, replace `C:\Users\<USERNAME>\AppData\Local\Programs\Whim` with `C:\path\to\repo\Whim`:

```csharp
#r "C:\Users\dalyisaac\Repos\Whim\src\Whim.Runner\bin\x64\Debug\net7.0-windows10.0.19041.0\whim.dll"
#r "C:\Users\dalyisaac\Repos\Whim\src\Whim.Runner\bin\x64\Debug\net7.0-windows10.0.19041.0\plugins\Whim.Bar\Whim.Bar.dll"
#r "C:\Users\dalyisaac\Repos\Whim\src\Whim.Runner\bin\x64\Debug\net7.0-windows10.0.19041.0\plugins\Whim.CommandPalette\Whim.CommandPalette.dll"
#r "C:\Users\dalyisaac\Repos\Whim\src\Whim.Runner\bin\x64\Debug\net7.0-windows10.0.19041.0\plugins\Whim.FloatingLayout\Whim.FloatingLayout.dll"
#r "C:\Users\dalyisaac\Repos\Whim\src\Whim.Runner\bin\x64\Debug\net7.0-windows10.0.19041.0\plugins\Whim.FocusIndicator\Whim.FocusIndicator.dll"
#r "C:\Users\dalyisaac\Repos\Whim\src\Whim.Runner\bin\x64\Debug\net7.0-windows10.0.19041.0\plugins\Whim.Gaps\Whim.Gaps.dll"
#r "C:\Users\dalyisaac\Repos\Whim\src\Whim.Runner\bin\x64\Debug\net7.0-windows10.0.19041.0\plugins\Whim.LayoutPreview\Whim.LayoutPreview.dll"
#r "C:\Users\dalyisaac\Repos\Whim\src\Whim.Runner\bin\x64\Debug\net7.0-windows10.0.19041.0\plugins\Whim.TreeLayout\Whim.TreeLayout.dll"
#r "C:\Users\dalyisaac\Repos\Whim\src\Whim.Runner\bin\x64\Debug\net7.0-windows10.0.19041.0\plugins\Whim.TreeLayout.Bar\Whim.TreeLayout.Bar.dll"
#r "C:\Users\dalyisaac\Repos\Whim\src\Whim.Runner\bin\x64\Debug\net7.0-windows10.0.19041.0\plugins\Whim.TreeLayout.CommandPalette\Whim.TreeLayout.CommandPalette.dll"

// Old references:
// #r "C:\Users\dalyisaac\AppData\Local\Programs\Whim\whim.dll"
// #r "C:\Users\dalyisaac\AppData\Local\Programs\Whim\plugins\Whim.Bar\Whim.Bar.dll"
// #r "C:\Users\dalyisaac\AppData\Local\Programs\Whim\plugins\Whim.CommandPalette\Whim.CommandPalette.dll"
// #r "C:\Users\dalyisaac\AppData\Local\Programs\Whim\plugins\Whim.FloatingLayout\Whim.FloatingLayout.dll"
// #r "C:\Users\dalyisaac\AppData\Local\Programs\Whim\plugins\Whim.FocusIndicator\Whim.FocusIndicator.dll"
// #r "C:\Users\dalyisaac\AppData\Local\Programs\Whim\plugins\Whim.Gaps\Whim.Gaps.dll"
// #r "C:\Users\dalyisaac\AppData\Local\Programs\Whim\plugins\Whim.LayoutPreview\Whim.LayoutPreview.dll"
// #r "C:\Users\dalyisaac\AppData\Local\Programs\Whim\plugins\Whim.TreeLayout\Whim.TreeLayout.dll"
// #r "C:\Users\dalyisaac\AppData\Local\Programs\Whim\plugins\Whim.TreeLayout.Bar\Whim.TreeLayout.Bar.dll"
// #r "C:\Users\dalyisaac\AppData\Local\Programs\Whim\plugins\Whim.TreeLayout.CommandPalette\Whim.TreeLayout.CommandPalette.dll"
```

Alternatively, the `#r` directives can be specified using a magic path prefix `WHIM_PATH` that is automatically replaced by the assembly's path when reading the config file:

```csharp
#r "WHIM_PATH\whim.dll"
#r "WHIM_PATH\plugins\Whim.Bar\Whim.Bar.dll"
#r "WHIM_PATH\plugins\Whim.CommandPalette\Whim.CommandPalette.dll"
#r "WHIM_PATH\plugins\Whim.FloatingLayout\Whim.FloatingLayout.dll"
#r "WHIM_PATH\plugins\Whim.FocusIndicator\Whim.FocusIndicator.dll"
#r "WHIM_PATH\plugins\Whim.Gaps\Whim.Gaps.dll"
#r "WHIM_PATH\plugins\Whim.LayoutPreview\Whim.LayoutPreview.dll"
#r "WHIM_PATH\plugins\Whim.TreeLayout\Whim.TreeLayout.dll"
#r "WHIM_PATH\plugins\Whim.TreeLayout.Bar\Whim.TreeLayout.Bar.dll"
#r "WHIM_PATH\plugins\Whim.TreeLayout.CommandPalette\Whim.TreeLayout.CommandPalette.dll"
```

However, this will break the IDE code completion when editing the config file.
