# YAML/JSON Configuration (WIP)

Whim is in the process of adding YAML configuration support - this is being tracked in [#1009](https://github.com/dalyIsaac/Whim/issues/1009).

This has been implemented in the `Whim.Yaml` plugin, and may have breaking changes for the lifetime of the GitHub issue.

## Setup

The YAML/JSON configuration is not yet included in the default Whim configuration.

Add to your `whim.config.csx`:

```csharp
// ...previous references

// NOTE: Replace WHIM_PATH with the path to your Whim installation
#r "WHIM_PATH\plugins\Whim.Yaml\Whim.Yaml.dll"

using System;
// ...usings...
using Whim.Yaml;

// ...

void DoConfig(IContext context)
{
    // Make sure to place this at the top of your config
    YamlLoader.Load(context);
}
```

Whim will then look for a `whim.config.yaml` or `whim.config.json` file in the root of your `.whim` directory. If it finds one, it will use that file to configure Whim.

## Schema

The schema for the YAML and JSON configuration is available [here](https://raw.githubusercontent.com/dalyIsaac/Whim/main/src/Whim.Yaml/schema.json). This schema will eventually be bundled with the Whim installation.

To use the schema in your YAML file, add the following line at the top of your file:

```yaml
# yaml-language-server: $schema=https://raw.githubusercontent.com/dalyIsaac/Whim/main/src/Whim.Yaml/schema.json
```

To use the schema in your JSON file, add the following line at the top of your file:

```json
{
  "$schema": "https://raw.githubusercontent.com/dalyIsaac/Whim/main/src/Whim.Yaml/schema.json",
  ...
}
```
