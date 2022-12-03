using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestGetAdjacentNode
{
	private readonly TestTreeEngine _testEngine = new();

	private readonly Mock<IMonitor> _monitor = new();
	private readonly Mock<IMonitorManager> _monitorManager = new();
	private readonly Mock<IConfigContext> _configContext = new();

	public TestGetAdjacentNode()
	{
		_monitor.Setup(m => m.Width).Returns(1920);
		_monitor.Setup(m => m.Height).Returns(1080);

		_monitorManager.Setup(m => m.FocusedMonitor).Returns(_monitor.Object);
		_configContext.Setup(c => c.MonitorManager).Returns(_monitorManager.Object);
	}

	#region GetAdjacentNode
	/// <summary>
	/// There is no node to the left of Left.
	/// </summary>
	[Fact]
	public void GetAdjacentNode_Left()
	{
		// The root should be a split node, with two children.
		Assert.Null(_testEngine.Engine.GetAdjacentNode(_testEngine.LeftNode, Direction.Left));
	}

	/// <summary>
	/// The node to the left of RightBottom is Left.
	/// </summary>
	[Fact]
	public void GetAdjacentNode_RightBottom_Left()
	{
		Assert.Equal(
			_testEngine.LeftNode,
			_testEngine.Engine.GetAdjacentNode(_testEngine.RightBottomNode, Direction.Left)
		);
	}

	/// <summary>
	/// The node above RightBottom should be RightTopLeftBottomLeft.
	/// </summary>
	[Fact]
	public void GetAdjacentNode_RightBottom_Up()
	{
		Assert.Equal(
			_testEngine.RightTopLeftBottomLeftNode,
			_testEngine.Engine.GetAdjacentNode(_testEngine.RightBottomNode, Direction.Up)
		);
	}

	/// <summary>
	/// There is no node to the right or below RightBottom.
	/// </summary>
	[Fact]
	public void GetAdjacentNode_RightBottom_Null()
	{
		Assert.Null(_testEngine.Engine.GetAdjacentNode(_testEngine.RightBottomNode, Direction.Right));
		Assert.Null(_testEngine.Engine.GetAdjacentNode(_testEngine.RightBottomNode, Direction.Down));
	}

	/// <summary>
	/// The node to the left of RightTopRight3 is RightTopLeftBottomRightTop.
	/// </summary>
	[Fact]
	public void GetAdjacentNode_RightTopRight3_Left()
	{
		Assert.Equal(
			_testEngine.RightTopLeftBottomRightTopNode,
			_testEngine.Engine.GetAdjacentNode(_testEngine.RightTopRight3Node, Direction.Left)
		);
	}

	/// <summary>
	/// The node to the right of RightTopLeftBottomRightTop is RightTopRight2.
	/// </summary>
	[Fact]
	public void GetAdjacentNode_RightTopLeftBottomRightTop_Right()
	{
		Assert.Equal(
			_testEngine.RightTopRight2Node,
			_testEngine.Engine.GetAdjacentNode(_testEngine.RightTopLeftBottomRightTopNode, Direction.Right)
		);
	}

	/// <summary>
	/// The node to the right of RightTopLeftBottomRightTop is RightTopLeftBottomRightBottom.
	/// </summary>
	[Fact]
	public void GetAdjacentNode_RightTopLeftBottomRightTop_Down()
	{
		Assert.Equal(
			_testEngine.RightTopLeftBottomRightBottomNode,
			_testEngine.Engine.GetAdjacentNode(_testEngine.RightTopLeftBottomRightTopNode, Direction.Down)
		);
	}

	/// <summary>
	/// The node to the left of RightTopLeftBottomRightTop is RightTopLeftBottomLeft.
	/// </summary>
	[Fact]
	public void GetAdjacentNode_RightTopLeftBottomRightTop_Left()
	{
		Assert.Equal(
			_testEngine.RightTopLeftBottomLeftNode,
			_testEngine.Engine.GetAdjacentNode(_testEngine.RightTopLeftBottomRightTopNode, Direction.Left)
		);
	}

	/// <summary>
	/// The node above RightTopLeftBottomRightTop is RightTopLeftTop.
	/// </summary>
	[Fact]
	public void GetAdjacentNode_RightTopLeftBottomRightTop_Up()
	{
		Assert.Equal(
			_testEngine.RightTopLeftTopNode,
			_testEngine.Engine.GetAdjacentNode(_testEngine.RightTopLeftBottomRightTopNode, Direction.Up)
		);
	}
	#endregion
}
