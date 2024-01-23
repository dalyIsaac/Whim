# Scripting

Whim uses the [Microsoft.CodeAnalysis.CSharp](https://learn.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.csharp) scripting engine to run your `.whim/whim.config.csx` file.

## Nullable

Your config will start with `#nullable enable`. This preprocessor directive tells your IDE that you want to use nullable reference types. You can read more about nullable reference types at [Nullable reference types](https://docs.microsoft.com/en-us/dotnet/csharp/nullable-references).

## Referencing External Assemblies

Following that will be a series of `#r` compiler directives. For example:

```csharp
#r "C:\Users\Isaac\AppData\Local\Programs\Whim\whim.dll"
#r "C:\Users\Isaac\AppData\Local\Programs\Whim\plugins\Whim.Bar\Whim.Bar.dll"
#r "C:\Users\Isaac\AppData\Local\Programs\Whim\plugins\Whim.CommandPalette\Whim.CommandPalette.dll"
// ...
```

These add references to external assemblies. Your IDE can use these to access code completion and documentation.

## The Code

The rest of the file is largely typical C# code. There are `using` statements, and a `DoConfig` function which takes an <xref:Whim.IContext> parameter. This is the entry point for your config. Prior to calling this function, Whim will set up the <xref:Whim.IContext> and its constituent parts.

After the `DoConfig` function is called, Whim will initialize the various managers and plugins and start the main loop.

The `DoConfig` function is returned from the script so Whim can call it when it loads.
