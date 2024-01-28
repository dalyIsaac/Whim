# Customizing Whim

When you run Whim for the first time, it will create the `C:\Users\{username}\.whim` directory populated with a [C# script](https://github.com/dalyIsaac/Whim/blob/main/src/Whim/Template/whim.config.csx) and resources.

`whim.config.csx` is a C# script file, and is reloaded every time Whim starts. On first run, it is populated with a pre-filled example which you can use as a starting point - see the config [here](https://github.com/dalyIsaac/Whim/blob/main/src/Whim/Template/whim.config.csx).

The core of the configuration is nested inside a <xref:Whim.DoConfig> function, which is written in typical C# code. The function takes an <xref:Whim.IContext> parameter. `DoConfig` is the entry point for your config. Prior to calling `DoConfig`, Whim will set up the `IContext` and its constituent parts, which exposes Whim's [API](/api) to be used in the configuration. For more, see the [Scripting](../customize/scripting.md) page.

The "Customize" section documents popular configuration options. Unless explicitly stated otherwise, all code snippets found in the "Customize" section are expected to be located inside the body of `DoConfig`.

To have the best development experience, you should have .NET tooling installed (Visual Studio Code will prompt you when you open `.whim`).

To customize the location of the `.whim` directory, set the `--dir` CLI argument when starting Whim.
