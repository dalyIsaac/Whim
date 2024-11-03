# Scripting

Whim uses the [Microsoft.CodeAnalysis.CSharp](https://learn.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.csharp) scripting engine to run your `.whim/whim.config.csx` file.

Various snippets can be found on the [Snippets](snippets.md) page.

## Tooling

If you plan to customize your Whim configuration using C#, it's recommended to install dedicated C# tooling.

To get code completion, it's recommended to install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) for Windows. Whim will install a self-contained .NET 8 instance as part of the installation process, but installing the SDK will give you access to the full .NET ecosystem.

The following editors can be used to write C# scripts:

- [Visual Studio Code](https://code.visualstudio.com/download) with the [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) extension
- [Visual Studio](https://visualstudio.microsoft.com/downloads/)
- [Rider](https://www.jetbrains.com/rider/)

## Directives

The top of the config contains several preprocessor, compiler and using directives. There is usually no need to change any of these from their defaults.

```csharp
#nullable enable
#r "C:\Users\Isaac\AppData\Local\Programs\Whim\whim.dll"
#r "C:\Users\Isaac\AppData\Local\Programs\Whim\plugins\Whim.Bar\Whim.Bar.dll"
#r "C:\Users\Isaac\AppData\Local\Programs\Whim\plugins\Whim.CommandPalette\Whim.CommandPalette.dll"
// ...

using System;
using System.Collections.Generic;
using Whim;
// ...
```

- The `#nullable enable` preprocessor directive tells your IDE that you want to use [nullable reference types](https://docs.microsoft.com/en-us/dotnet/csharp/nullable-references).
- The `#r` compiler directives add references to external assemblies. Your IDE can use these to access code completion and documentation.
- The `using` directives allow you to use types defined in namespaces without specifying the fully qualified namespaces.

## The DoConfig Function

Following the various directives goes the `DoConfig` function which takes an <xref:Whim.IContext> parameter and is written in largely typically C# code.

```csharp
void DoConfig(IContext context)
{
    // Your core config goes here
}

return DoConfig;
```

`DoConfig` is the entry point for your config. Prior to calling this function, Whim will set up the <xref:Whim.IContext> and its constituent parts. After `DoConfig` is called, Whim will initialize the various managers and plugins and start the main loop.

The `DoConfig` function is returned at the end of the script so Whim can call it when it loads.
