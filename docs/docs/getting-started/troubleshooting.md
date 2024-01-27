# Troubleshooting

## Managing troublesome windows

Some windows like to remember their size and position. This can be a problem for Whim, because it will try to manage the window's size and position, and the window will fight back.

The <xref:Whim.IWindowManager> exposes an <xref:Whim.IFilterManager> called <xref:Whim.IWindowManager.LocationRestoringFilterManager>. `LocationRestoringFilterManager` listens to <xref:Whim.IWindowManager.WindowMoved> events for these windows and will force their parent <xref:Whim.IWorkspace> to do a layout two seconds after their first `WindowMoved` event, attempting to restore the window to its correct position.

If this doesn't work, dragging a window's edge will force a layout, which should fix the window's position. This is an area which could use further improvement.

Examples of troublesome windows include Firefox and JetBrains Gateway.

## Window launch locations

Windows can launch windows in different locations. Additionally, interacting with some untracked windows like the Windows Taskbar can break focus tracking in Whim.

To counteract this, the <xref:Whim.IRouterManager> has a <xref:Whim.IRouterManager.RouterOptions> property which can configure how new windows are routed - see the <xref:Whim.RouterOptions> enum.
