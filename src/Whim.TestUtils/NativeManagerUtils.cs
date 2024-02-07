using Microsoft.UI.Dispatching;
using NSubstitute;

namespace Whim.TestUtils;

public static class NativeManagerUtils
{
	public static void SetupTryEnqueue(IContext ctx)
	{
		ctx.NativeManager.When(cnm => cnm.TryEnqueue(Arg.Any<DispatcherQueueHandler>()))
			.Do(callInfo =>
			{
				DispatcherQueueHandler handler = callInfo.ArgAt<DispatcherQueueHandler>(0);
				handler.Invoke();
			});
	}
}
