using Windows.Win32;

namespace Whim.Tests;

public class WindowProcessorManagerTests
{
	[Theory]
	[AutoSubstituteData]
	public void ShouldBeIgnored_CreateProcessor_Failure(IContext ctx, IWindow window)
	{
		// Given a window which is not a Firefox window
		window.ProcessFileName.Returns("chrome.exe");
		WindowProcessorManager sut = new(ctx);

		// When ShouldBeIgnored is called
		bool result = sut.ShouldBeIgnored(window, default, default, default, default, default, default);

		// Then the result should be false
		Assert.False(result);
	}

	[Theory]
	[AutoSubstituteData]
	public void ShouldBeIgnored_CreateProcessor_Success(IContext ctx, IWindow window)
	{
		// Given a window which is a Firefox window
		window.ProcessFileName.Returns("firefox.exe");
		WindowProcessorManager sut = new(ctx);

		// When ShouldBeIgnored is called for the first time
		bool result = sut.ShouldBeIgnored(window, default, default, default, default, default, default);

		// Then the result should be true
		Assert.True(result);
	}

	[Theory]
	[AutoSubstituteData]
	public void ShouldBeIgnored_ProcessorExists_Process(IContext ctx, IWindow window)
	{
		// Given a window which is a Firefox window
		window.ProcessFileName.Returns("firefox.exe");
		WindowProcessorManager sut = new(ctx);

		// When ShouldBeIgnored is called for the second time
		sut.ShouldBeIgnored(window, default, PInvoke.EVENT_OBJECT_CLOAKED, 0, 0, 0, 0);
		bool result = sut.ShouldBeIgnored(window, default, 0, 0, 0, 0, 0);

		// Then the processor should have been created by the second call, and the window should not be ignored
		Assert.False(result);
	}

	[Theory]
	[AutoSubstituteData]
	public void ShouldBeIgnored_ProcessorExists_RemoveProcessor(IContext ctx, IWindow window)
	{
		// Given a window which is a Firefox window
		window.ProcessFileName.Returns("firefox.exe");
		WindowProcessorManager sut = new(ctx);

		// When ShouldBeIgnored is called for the second time
		sut.ShouldBeIgnored(window, default, PInvoke.EVENT_OBJECT_CLOAKED, 0, 0, 0, 0);
		bool firstProcessorResult = sut.ShouldBeIgnored(window, default, PInvoke.EVENT_OBJECT_DESTROY, 0, 0, 0, 0);
		bool secondProcessorResult = sut.ShouldBeIgnored(window, default, 0, 0, 0, 0, 0);

		// Then the processor should have been removed by the second call, and the window should be ignored in the next call
		Assert.False(firstProcessorResult);
		Assert.True(secondProcessorResult);
	}
}
