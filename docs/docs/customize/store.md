# Store

The <xref:Whim.IStore> contains the state of windows, workspaces, and monitors in Whim. For a more in-depth look at the store, see the [Store](../architecture/store.md) page in the architecture section.

## Core Concepts

- Use <xref:Whim.Pickers> to retrieve values from the store - see [Reading from the Store](#reading-from-the-store)
- Use <xref:Whim.Transform>s to update the store - see [Writing to the Store](#writing-to-the-store)
- Use handles and IDs to reference windows, workspaces, and monitors - see [Handles and IDs](#handles-and-ids)
- Use `Result<T>` to handle errors - see [Result<T>](#resultt)

## Reading from the Store

To retrieve values from the store, use the <xref href="Whim.IStore.Pick``1(Whim.Picker{``0})" /> method. For example:

```csharp
IMonitor primaryMonitor = context.Store.Pick(Pickers.PickPrimaryMonitor());
IMonitor? thirdMonitor = context.Store.Pick(Pickers.PickMonitorByIndex(2)).ValueOrDefault;
```

Pickers can be found in the <xref:Whim.Pickers> static class.

## Writing to the Store

To update Whim's state, use the <xref href="Whim.IStore.Dispatch``1(Whim.Transform{``0})" /> method. For example:

```csharp
context.Store.Dispatch(new ActivateWorkspaceTransform(workspaceId, monitor.Handle));
```

Transforms can be found in the API documentation for the <xref:Whim> namespace.

## Handles and IDs

The store is immutable. For example, if you retrieve a <xref:Whim.IWorkspace> using a picker and then rename the workspace, the reference will have the old name. As a result, most transforms and pickers use "handles" and "IDs" to reference windows, workspaces, and monitors.

| Type                                       | Description                          |
| ------------------------------------------ | ------------------------------------ |
| <xref:Windows.Win32.Foundation.HWND>       | A handle to a window.                |
| <xref:System.Guid>                         | A unique identifier for a workspace. |
| <xref:Windows.Win32.Graphics.Gdi.HMONITOR> | A handle to a monitor.               |

A handle is a token that represents a resource that is managed by the Windows kernel. The workspace ID is a concept specific to Whim.

## `Result<T>`

All transforms and some pickers return a `Result<T>`. This is a type that can either contain a value or an error. For example:

```csharp
Result<IMonitor> monitorResult = context.Store.Pick(Pickers.PickMonitorByIndex(2));
if (monitorResult.TryGet(out IMonitor monitor))
{
    // Do something with the monitor
}
else
{
    // Handle the error
}
```

If you're sure the operation will succeed, you can use the `ValueOrDefault` property. However, it is recommended to use the `TryGet` method to handle errors.

## `Unit`

Some transforms will return a `Result<Unit>` This represents an operation that does not return a value. For example:

```csharp
Result<Unit> result = context.Store.Dispatch(new MoveWindowToMonitorTransform(monitorHandle, windowHandle));
if (!result.IsSuccessful)
{
    // Handle the error
}
```

### `ValueOrDefault` Examples

The return type of `PickMonitorByIndex` is `Result<IMonitor>`. As the value returned is a complex
type, when you use `ValueOrDefault`, the return type will be nullable.

```csharp
IMonitor? monitor = context.Store.Pick(Pickers.PickMonitorByIndex(2)).ValueOrDefault;
```

The `AddWorkspaceTransform` implements `Transform<Guid>`, so the return type of dispatching it will be
`Result<Guid>`. As the value returned is a value type, when you use `ValueOrDefault`, the return type will be the value type itself.
However, if it fails, the return type will be the default value of the value type, which in this
case is `Guid.Empty`.

```csharp
Guid workspaceId = context.Store.Dispatch(new AddWorkspaceTransform("Workspace name")).ValueOrDefault;
```

The return type of `PickPrimaryMonitor` is `PurePicker<IMonitor>`, so there will always be a value.

```csharp
IMonitor primaryMonitor = context.Store.Pick(Pickers.PickPrimaryMonitor());
```

## Deprecated API

The store is also exposed using the deprecated interfaces <xref:Whim.IWindowManager>, <xref:Whim.IWorkspaceManager>, <xref:Whim.IMonitorManager> and <xref:Whim.IButler>. All functionality is now available as transforms and pickers on the store.

The <xref:Whim.IWorkspace> also exposes a variety of methods to interact with workspaces. Similarly, all functionality is now available as transforms and pickers on the store.
