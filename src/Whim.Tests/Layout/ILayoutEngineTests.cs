using Moq;
using Xunit;

namespace Whim.Tests;

public class ILayoutEngineTests
{
	[Fact]
	public void GetLayoutEngine_Immediate()
	{
		ILayoutEngine columnLayoutEngine = new ColumnLayoutEngine();
		Assert.Equal(columnLayoutEngine, columnLayoutEngine.GetLayoutEngine<ColumnLayoutEngine>());
	}

	[Fact]
	public void GetLayoutEngine_Proxy()
	{
		ILayoutEngine columnLayoutEngine = new ColumnLayoutEngine();
		ILayoutEngine proxyLayoutEngine = new ProxyLayoutEngine(engine => engine)(columnLayoutEngine);
		Assert.Equal(columnLayoutEngine, proxyLayoutEngine.GetLayoutEngine<ColumnLayoutEngine>());
	}

	[Fact]
	public void ContainsEqual_Immediate()
	{
		ILayoutEngine columnLayoutEngine = new ColumnLayoutEngine();
		Assert.True(columnLayoutEngine.ContainsEqual(columnLayoutEngine));
	}

	[Fact]
	public void ContainsEqual_Fail()
	{
		Mock<ILayoutEngine> columnLayoutEngineMock = new();
		Mock<ILayoutEngine> columnLayoutEngineMock2 = new();
		Assert.False(columnLayoutEngineMock.Object.ContainsEqual(columnLayoutEngineMock2.Object));
	}

	[Fact]
	public void ContainsEqual_Proxy()
	{
		ILayoutEngine columnLayoutEngine = new ColumnLayoutEngine();
		ILayoutEngine proxyLayoutEngine = new ProxyLayoutEngine(engine => engine)(columnLayoutEngine);
		Assert.True(proxyLayoutEngine.ContainsEqual(columnLayoutEngine));
	}
}
