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

<xref:Whim.IRouterManager> has a <xref:Whim.IRouterManager.RouterOptions> property which can configure how new windows are routed - see the <xref:Whim.RouterOptions> enum.