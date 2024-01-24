# Layout Engines

A "layout engine" or <xref:Whim.ILayoutEngine> in Whim is responsible for arranging windows in a workspace. Each workspace has a single active layout engine, and can cycle through different layout engines.

There are two different types of layout engines: proxy layout engines, and leaf layout engines. Proxy layout engines wrap other engines, and can be used to modify the behavior of other engines. For example, the [`Gaps` plugin](../plugins/gaps.md) will add gaps between windows - normally layout engines won't leave gaps between windows. Leaf layout engines are the lowest level layout engines, and are responsible for actually arranging windows.

To see the available layout engines, see [Layout Engines](../layout-engines.md).
