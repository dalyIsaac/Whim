# Store

The <xref:Whim.IStore> contains the state of windows, workspaces, and monitors in Whim. For a more in-depth look at the store, see the [Store](../architecture/store.md) page in the architecture section.

## Pickers and Transforms

To **retrieve values from the store**, pass a "picker" from the <xref:Whim.Pickers> static class to the <xref href="Whim.IStore.Pick``1(Whim.Picker{``0})" /> method. Pickers will typically take in [handles or IDs as arguments](#handles-and-ids).

For example:

```csharp
IMonitor primaryMonitor = context.Store.Pick(Pickers.PickPrimaryMonitor());
IMonitor thirdMonitor = context.Store.Pick(Pickers.PickMonitorByIndex(2)).Value;
```

To **update Whim's state**, pass a "transform" to the <xref href="Whim.IStore.Dispatch``1(Whim.Transform{``0})" /> method. Transforms can be found in the API documentation for the <xref:Whim> namespace.

For example:

```csharp
context.Store.Dispatch(new ActivateWorkspaceTransform(workspaceId, monitor.Handle));
```

As an example of using pickers and transforms together:

```csharp
Guid browserWorkspaceId = context.Store.Dispatch(new AddWorkspaceTransform("Browser")).Value;

context.CommandManager.Add(
    identifier: "last_focused_to_browser",
    title: "Move the last focused window to the browser workspace",
    callback: () =>
    {
        if (context.Store.Pick(Pickers.PickLastFocusedWindow()).TryGet(out IWindow window))
        {
            context.Store.Dispatch(new MoveWindowToWorkspaceTransform(browserWorkspaceId, window.Handle));
        }
    }
);
```

For examples on how to interact with the store, see the [Snippets](../snippets.md) page.

## Handles and IDs

The store is immutable. For example, if you retrieve a <xref:Whim.IWorkspace> using a picker and then rename the workspace, the reference will have the old name. As a result, most transforms and pickers use "handles" and "IDs" to reference windows, workspaces, and monitors.

| Type                                       | Description                          |
| ------------------------------------------ | ------------------------------------ |
| <xref:Windows.Win32.Foundation.HWND>       | A handle to a window.                |
| <xref:System.Guid>                         | A unique identifier for a workspace. |
| <xref:Windows.Win32.Graphics.Gdi.HMONITOR> | A handle to a monitor.               |

A handle is a token that represents a resource that is managed by the Windows kernel. The workspace ID is a concept specific to Whim.

## `Result<T>`

All transforms and some pickers return a `Result<T>`. This is a type that can either contain a value or an error.

Some transforms will return a `Result<Unit>` This represents an operation that does not return a value. For example:

```csharp
Result<Unit> result = context.Store.Dispatch(new MoveWindowToMonitorTransform(monitorHandle, windowHandle));
if (!result.IsSuccessful)
{
    // Handle the error
}
```

### Error Handling

To handle errors for reference types (classes, interfaces):

```csharp
// The return type of PickMonitorByIndex is Result<IMonitor>.

// If this fails, monitor1 will be null.
IMonitor? monitor1 = context.Store.Pick(Pickers.PickMonitorByIndex(0)).ValueOrDefault;

// If this fails, an exception will throw.
IMonitor monitor2 = context.Store.Pick(Pickers.PickMonitorByIndex(0)).Value;

// If this fails, the else block will execute.
if (context.Store.Pick(Pickers.PickMonitorByIndex(0)).TryGet(out IMonitor monitor3))
{
    // Do something with the monitor
}
else
{
    // Handle the error
}
```

To handle errors for value types (handles, IDs, etc.):

```csharp
// The return type of AddWorkspaceTransform is Result<Guid>.

// If this fails, workspaceId will be the default value, equivalent to all bits being 0.
Guid workspace1 = context.Store.Dispatch(new AddWorkspaceTransform("Workspace 1")).ValueOrDefault;

// If this fails, an exception will throw.
Guid workspace2 = context.Store.Dispatch(new AddWorkspaceTransform("Workspace 2")).Value;

// If this fails, the else block will execute.
if (context.Store.Dispatch(new AddWorkspaceTransform("Workspace 3")).TryGet(out Guid workspace3))
{
    // Do something with the workspace
}
else
{
    // Handle the error
}
```

Items which don't return a `Result` do not need to be handled in this way - they can be used directly.

```csharp
IMonitor primaryMonitor = context.Store.Pick(Pickers.PickPrimaryMonitor());
```

## Deprecated API

The store is also exposed using the deprecated interfaces <xref:Whim.IWindowManager>, <xref:Whim.IWorkspaceManager>, <xref:Whim.IMonitorManager> and <xref:Whim.IButler>. All functionality is now available as transforms and pickers on the store.

The <xref:Whim.IWorkspace> also exposes a variety of methods to interact with workspaces. Similarly, all functionality is now available as transforms and pickers on the store.
