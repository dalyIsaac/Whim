# Routing

<xref:Whim.IRouterManager> is used by Whim to route windows to specific workspaces. For example, to route Discord to a workspace "Chat", you can do the following:

```csharp
context.RouterManager.AddProcessFileNameRoute("Discord.exe", "Chat");
```

Besides by their `ProcessFileName`, windows can also be matched by their `WindowClass`, by their `Title` or via custom rules -- see <xref:Whim.IRouterManager>.

