# Customizing Whim

Whim can be configured using [YAML/JSON](#yamljson) or [C# scripting](#c-scripting).

When you run Whim for the first time, it will create the `C:\Users\{username}\.whim` directory populated with:

- [`whim.config.yaml`](https://github.com/dalyIsaac/Whim/blob/main/src/Whim/Template/whim.config.yaml) - the YAML configuration file
- [`whim.config.csx`](https://github.com/dalyIsaac/Whim/blob/main/src/Whim/Template/whim.config.csx) - the C# configuration script

To customize the location of the `.whim` directory, set the `--dir` command-line argument when starting Whim.

## YAML/JSON

Files named `whim.config.yaml` or `whim.config.json` in the `.whim` directory can be used to configure Whim. The configuration file can be used to set up plugins, commands, and keybindings. The configuration file is loaded by the `Whim.Yaml` plugin, which is included with Whim.

`whim.config.yaml` and `whim.config.json` can be edited in any text editor with YAML/JSON support, like [Visual Studio Code](https://code.visualstudio.com/download).

**For more, see the [YAML/JSON Configuration](../configure/yaml-json-configuration.md) page**.

## C# Scripting

`whim.config.csx` is a C# script file, and is reloaded every time Whim starts. On first run, it is populated with a pre-filled example which you can use as a starting point - see the config [here](https://github.com/dalyIsaac/Whim/blob/main/src/Whim/Template/whim.config.csx).

The core of the configuration is nested inside a <xref:Whim.DoConfig> function, which is written in typical C# code. The function takes an <xref:Whim.IContext> parameter. `DoConfig` is the entry point for your config. Prior to calling `DoConfig`, Whim will set up the `IContext` and its constituent parts, which exposes Whim's [API](/api) to be used in the configuration.

**For more, see the [Scripting](../script/scripting.md) page**.
