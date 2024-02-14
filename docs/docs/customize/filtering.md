# Filtering

<xref:Whim.IFilterManager> tells Whim to ignore windows based on <xref:Whim.Filter> delegates. A common use case is for plugins to filter out windows they manage themselves and want Whim to not lay out. For example, the bars and command palette are filtered out.

```csharp
// Called by the bar plugin.
context.FilterManager.AddTitleMatchFilter("Whim Bar");
```
Besides by their `Title`, windows can also be matched by their `WindowClass`, by their `ProcessFileName` or via custom rules -- see <xref:Whim.IFilterManager>.

## Built-in filters

By default, Whim ignores a built-in list of windows that are known to cause problems with dynamic tiling window manager. Behind the scenes, Whim automatically updates the built-in list of ignored windows based on a subset of the rules from the community-driven [collection of application rules](https://github.com/LGUG2Z/komorebi-application-specific-configuration) managed by komorebi.

To disable the built-in list of ignored windows, add the following _before_ defining any filters:

```csharp
context.FilterManager.Clear();
```

### Caveats

- Turning off individual komorebi rules is not currently supported but is planned - see [dalyIsaac/Whim#702](https://github.com/dalyIsaac/Whim/issues/702).
- Whim does not support komorebi rules defined using regular expressions, as komorebi is written in Rust and the regular expressions are likely to have compatibility issues with .NET - see [dalyIsaac/Whim#690](https://github.com/dalyIsaac/Whim/issues/690). In practice, this is not much of a concern; as of February 2024, there are no such rules defined by komorebi.
