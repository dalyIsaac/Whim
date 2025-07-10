using NSubstitute;

namespace Whim.TreeLayout.Tests;

// TODO: Delete
[Obsolete]
internal sealed class LayoutEngineWrapper
{
	public IContext Context { get; } = Substitute.For<IContext>();
	public ITreeLayoutPlugin Plugin { get; } = Substitute.For<ITreeLayoutPlugin>();
	public IWorkspace Workspace { get; } = Substitute.For<IWorkspace>();
	public IMonitor Monitor { get; } = Substitute.For<IMonitor>();
	public LayoutEngineIdentity Identity { get; } = new();

	public LayoutEngineWrapper()
	{
		Monitor.WorkingArea.Returns(new Rectangle<int>() { Width = 100, Height = 100 });

		Context.WorkspaceManager.ActiveWorkspace.Returns(Workspace);
		Context.MonitorManager.ActiveMonitor.Returns(Monitor);

		SetAddWindowDirection(Direction.Right);
	}

	public LayoutEngineWrapper SetAsLastFocusedWindow(IWindow? window)
	{
		Workspace.LastFocusedWindow.Returns(window);
		return this;
	}

	public LayoutEngineWrapper SetAddWindowDirection(Direction direction)
	{
		Plugin.GetAddWindowDirection(Arg.Any<TreeLayoutEngine>()).Returns(direction);
		return this;
	}
}
