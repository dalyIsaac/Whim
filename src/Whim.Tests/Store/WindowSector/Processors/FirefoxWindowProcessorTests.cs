using Windows.Win32;

namespace Whim.Tests;

public class FirefoxWindowProcessorTests
{
	[Theory]
	[AutoSubstituteData]
	public void Create_Failure(IContext ctx, IWindow window)
	{
		// Given a window which is not a Firefox window
		window.ProcessFileName.Returns("chrome.exe");

		// When `Create` is called
		IWindowProcessor? processor = FirefoxWindowProcessor.Create(ctx, window);

		// Then the processor should be null
		Assert.Null(processor);
	}

	[Theory]
	[AutoSubstituteData]
	public void Create_Success(IContext ctx, IWindow window)
	{
		// Given a window which is a Firefox window
		window.ProcessFileName.Returns("firefox.exe");

		// When `Create` is called
		IWindowProcessor? processor = FirefoxWindowProcessor.Create(ctx, window);

		// Then the processor should not be null
		Assert.NotNull(processor);
	}

	[Theory]
	[InlineAutoSubstituteData(PInvoke.EVENT_OBJECT_SHOW)]
	[InlineAutoSubstituteData(PInvoke.EVENT_SYSTEM_FOREGROUND)]
	[InlineAutoSubstituteData(PInvoke.EVENT_OBJECT_UNCLOAKED)]
	[InlineAutoSubstituteData(PInvoke.EVENT_OBJECT_HIDE)]
	[InlineAutoSubstituteData(PInvoke.EVENT_SYSTEM_MOVESIZESTART)]
	[InlineAutoSubstituteData(PInvoke.EVENT_SYSTEM_MOVESIZEEND)]
	[InlineAutoSubstituteData(PInvoke.EVENT_OBJECT_LOCATIONCHANGE)]
	[InlineAutoSubstituteData(PInvoke.EVENT_SYSTEM_MINIMIZESTART)]
	[InlineAutoSubstituteData(PInvoke.EVENT_SYSTEM_MINIMIZEEND)]
	public void ShouldBeIgnored_UntilCloaked(uint eventType, IContext ctx, IWindow window)
	{
		// Given an event which isn't `EVENT_OBJECT_CLOAKED` or `EVENT_OBJECT_DESTROY`
		window.ProcessFileName.Returns("firefox.exe");
		IWindowProcessor processor = FirefoxWindowProcessor.Create(ctx, window)!;

		// When the event is passed to `ShouldBeIgnored`
		WindowProcessorResult result = processor.ProcessEvent(eventType, 0, 0, 0, 0);

		// Then the event should be ignored
		Assert.Equal(WindowProcessorResult.Ignore, result);
	}

	[Theory]
	[AutoSubstituteData]
	public void ShouldBeIgnored_FirstCloaked(IContext ctx, IWindow window)
	{
		// Given the first `EVENT_OBJECT_CLOAKED` event
		window.ProcessFileName.Returns("firefox.exe");
		IWindowProcessor processor = FirefoxWindowProcessor.Create(ctx, window)!;

		// When the event is passed to `ShouldBeIgnored`
		WindowProcessorResult result = processor.ProcessEvent(PInvoke.EVENT_OBJECT_CLOAKED, 0, 0, 0, 0);

		// Then the event should be ignored
		Assert.Equal(WindowProcessorResult.Ignore, result);
	}

	[Theory]
	[AutoSubstituteData]
	public void ShouldNotBeIgnored_SecondCloaked(IContext ctx, IWindow window)
	{
		// Given the second `EVENT_OBJECT_CLOAKED` event
		window.ProcessFileName.Returns("firefox.exe");
		IWindowProcessor processor = FirefoxWindowProcessor.Create(ctx, window)!;
		processor.ProcessEvent(PInvoke.EVENT_OBJECT_CLOAKED, 0, 0, 0, 0);

		// When the event is passed to `ShouldBeIgnored`
		WindowProcessorResult result = processor.ProcessEvent(PInvoke.EVENT_OBJECT_CLOAKED, 0, 0, 0, 0);

		// Then the event should not be ignored
		Assert.Equal(WindowProcessorResult.Process, result);
	}

	[Theory]
	[AutoSubstituteData]
	public void ShouldBeRemoved(IContext ctx, IWindow window)
	{
		// Given an `EVENT_OBJECT_DESTROY` event
		window.ProcessFileName.Returns("firefox.exe");
		IWindowProcessor processor = FirefoxWindowProcessor.Create(ctx, window)!;

		// When the event is passed to `ShouldBeIgnored`
		WindowProcessorResult result = processor.ProcessEvent(PInvoke.EVENT_OBJECT_DESTROY, 0, 0, 0, 0);

		// Then the processor should be removed
		Assert.Equal(WindowProcessorResult.RemoveProcessor, result);
	}
}
