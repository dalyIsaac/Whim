# Layout Preview Plugin

The <xref:Whim.LayoutPreview.LayoutPreviewPlugin> adds a preview of the next layout when the user drags a window.

The <xref:Whim.LayoutPreview.LayoutPreviewPlugin> has no configuration options.

![Layout preview demo](../../images/layout-preview-demo.gif)

## Example Usage

```csharp
#r "WHIM_PATH\plugins\Whim.LayoutPreview\Whim.LayoutPreview.dll"

using Whim;
using Whim.LayoutPreview;

void DoConfig(IContext context)
{
  // ...

  LayoutPreviewPlugin layoutPreviewPlugin = new(context);
  context.PluginManager.AddPlugin(layoutPreviewPlugin);

  // ...
}

return DoConfig;

```

## Commands

N/A
