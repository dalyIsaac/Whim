# Result

Whim uses a custom `Result<T>` type for functional error handling, avoiding exceptions that could crash the application. This type represents the result of an operation that may fail, allowing callers to check success or failure explicitly.

The `Result<T>` struct contains either a successful value or a `WhimError` describing what went wrong.

## Returning a `Result<T>`

When a method returns a `Result<T>`, the caller can check if the operation was successful by calling the `TryGet` method.

```csharp
Result<IWindow> windowResult = context.Store.Pick(Pickers.PickWindowByHandle(windowHandle));
if (!windowResult.TryGet(out IWindow window))
{
    return windowResult;
}

// ...

return window;
```

## Creating Results

### Success Cases

```csharp
// Implicit conversion from value
return window;

// Explicit construction
return new Result<IWindow>(window);
```

### Error Cases

```csharp
// From WhimError
return new Result<IWindow>(new WhimError("Window not found"));

// From exception
return new Result<IWindow>(new WhimError("Operation failed", innerException));
```

## `Unit`

Sometimes an operation does not return a value, but the caller still needs to know if the operation was successful. In this case, the `Unit` type is used as the `T` type parameter of the `Result<T>`.

```csharp
Result<IMonitor> oldMonitorResult = context.Store.Pick(PickMonitorByWindow(windowHandle));
if (!oldMonitorResult.TryGet(out IMonitor oldMonitor))
{
    // Forward the error to the caller.
    return new(oldMonitorResult.Error!);
}

if (oldMonitor.Handle == MonitorHandle)
{
    // An error did not occur, so indicate that the operation was successful by returning the result.
    return Unit.Result;
}

return context.Store.Dispatch(new MoveWindowToWorkspaceTransform(workspace.Id, windowHandle));
```

## Future

`Result` is a simulacrum of the Rust-style `Result`, which is used to represent the result of an operation that may fail. There is a proposal for [type unions in C#](https://github.com/dotnet/csharplang/blob/main/proposals/TypeUnions.md), which if accepted will replace the `Result<T>` type throughout Whim.
