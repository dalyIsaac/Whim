# Filtering

<xref:Whim.IFilterManager> tells Whim to ignore windows based on <xref:Whim.Filter> delegates. A common use case is for plugins to filter out windows they manage themselves and want Whim to not lay out. For example, the bars and command palette are filtered out.

```csharp
// Called by the bar plugin.
context.FilterManager.AddTitleMatchFilter("Whim Bar");
```
