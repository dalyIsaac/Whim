# Customizing Whim

## Scripting

When you run Whim for the first time, it will create the `C:\Users\{username}\.whim` directory populated with a [C# script](https://github.com/dalyIsaac/Whim/blob/main/src/Whim/Template/whim.config.csx) and resources.

`whim.config.csx` is a C# script file, and is reloaded every time Whim starts. On first run, it is populated with a pre-filled example which you can use as a starting point - see the config [here](https://github.com/dalyIsaac/Whim/blob/main/src/Whim/Template/whim.config.csx).

The core of the configuration is nested inside a <xref:Whim.DoConfig> function, which is written in typical C# code. The function takes an <xref:Whim.IContext> parameter. `DoConfig` is the entry point for your config. Prior to calling `DoConfig`, Whim will set up the `IContext` and its constituent parts, which exposes Whim's [API](/api) to be used in the configuration.
**For more, see the [Scripting](../script/scripting.md) page**.

YAML and JSON configuration is in the works - see [#1009](https://github.com/dalyIsaac/Whim/issues/1009) for more information.

## Recommended tooling

Whim will install a self-contained .NET 8 instance as part of the installation process. To have the best experience while configuring Whim, it's recommended to install the following tools:

- [Visual Studio Code](https://code.visualstudio.com/download)
- [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) extension for Visual Studio Code
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) for Windows

## Script location

To customize the location of the `.whim` directory, set the `--dir` CLI argument when starting Whim.
