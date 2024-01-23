# Routing

<xref:Whim.IRouterManager> is used by Whim to route windows to specific workspaces. For example, to route Discord to a workspace "Chat", you can do the following:

```csharp
context.RouterManager.Add((window) =>
{
    if (window.ProcessFileName == "Discord.exe")
    {
        return context.WorkspaceManager.TryGet("Chat");
    }

    // Continue routing.
    return null;
});
```

## Window launch locations

Outside of [windows managing their own side and location](window-manager.md#managing-troublesome-windows), Windows can launch windows in different locations. Additionally, interacting with some untracked windows like the Windows Taskbar can break focus tracking in Whim.

To counteract this, the <xref:Whim.IRouterManager> has a <xref:Whim.IRouterManager.RouterOptions> property which can configure how new windows are routed - see the <xref:Whim.RouterOptions> enum.
