# Result

Whim uses the [`Result<T>`](https://dotnet.github.io/dotNext/api/DotNext.Result-1.html) type from the [`dotNext` library](https://dotnet.github.io/dotNext/index.html).

The idea of its usage throughout Whim is to avoid throwing exceptions, which can bubble up and cause the application to crash. Instead, the `Result<T>` type is used to represent the result of an operation that may fail, and the caller can check if the operation was successful or not.

## Returning a `Result<T>`

When a method returns a `Result<T>`, the caller can check if the operation was successful by calling the `TryGet` method.

```csharp
Result<IWindow> windowResult = GetWindow(ctx, internalCtx);
if (!windowResult.TryGet(out IWindow window))
{
    return windowResult;
}

// ...

return Result.FromValue(window);
```

## `Unit`

Sometimes an operation does not return a value, but the caller still needs to know if the operation was successful. In this case, the `Unit` type is used as the `T` type parameter of the `Result<T>`.

```csharp
Result<IMonitor> oldMonitorResult = ctx.Store.Pick(PickMonitorByWindow(windowHandle));
if (!oldMonitorResult.TryGet(out IMonitor oldMonitor))
{
    // Forward the exception to the caller.
    return Result.FromException<Unit>(oldMonitorResult.Error!);
}

if (oldMonitor.Handle == MonitorHandle)
{
    // An error did not occur, so indicate that the operation was successful by returning the result.
    return Unit.Result;
}

return ctx.Store.Dispatch(new MoveWindowToWorkspaceTransform(workspace.Id, windowHandle));
```

## Future

`Result` is a simulacrum of the Rust-style `Result`, which is used to represent the result of an operation that may fail. There is a proposal for [type unions in C#](https://github.com/dotnet/csharplang/blob/main/proposals/TypeUnions.md), which if accepted will replace the `Result<T>` type throughout Whim.
