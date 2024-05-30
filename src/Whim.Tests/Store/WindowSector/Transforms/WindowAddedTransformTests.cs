using System.ComponentModel;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim.Tests;

public class WindowAddedTransformTests
{
	private static void Setup(
		IContext ctx,
		IInternalContext internalCtx,
		HWND hwnd,
		bool isSplashScreen = false,
		bool isCloakedWindow = false,
		bool isNotStandardWindow = false,
		bool hasVisibleWindow = false,
		bool cannotCreateWindow = false,
		bool shouldBeIgnored = false
	)
	{
		internalCtx.CoreNativeManager.IsSplashScreen(hwnd).Returns(isSplashScreen);
		internalCtx.CoreNativeManager.IsCloakedWindow(hwnd).Returns(isCloakedWindow);
		internalCtx.CoreNativeManager.IsStandardWindow(hwnd).Returns(!isNotStandardWindow);
		internalCtx.CoreNativeManager.HasNoVisibleOwner(hwnd).Returns(!hasVisibleWindow);

		if (cannotCreateWindow)
		{
			internalCtx.CoreNativeManager.GetProcessNameAndPath(Arg.Any<int>()).Throws(new Win32Exception());
		}
		else
		{
			internalCtx
				.CoreNativeManager.GetProcessNameAndPath(Arg.Any<int>())
				.Returns(("processName", "processFileName"));
		}

		ctx.FilterManager.ShouldBeIgnored(Arg.Any<IWindow>()).Returns(shouldBeIgnored);
	}

	[InlineAutoSubstituteData<StoreCustomization>("IsSplashScreen", true, false, false, false, false, false)]
	[InlineAutoSubstituteData<StoreCustomization>("IsCloakedWindow", false, true, false, false, false, false)]
	[InlineAutoSubstituteData<StoreCustomization>("IsStandardWindow", false, false, true, false, false, false)]
	[InlineAutoSubstituteData<StoreCustomization>("HasNoVisibleWindow", false, false, false, true, false, false)]
	[InlineAutoSubstituteData<StoreCustomization>("CannotCreateWindow", false, false, false, false, true, false)]
	[InlineAutoSubstituteData<StoreCustomization>("ShouldBeIgnored", false, false, false, false, false, true)]
	[Theory]
	internal void Failure(
		string _,
		bool isSplashScreen,
		bool isCloakedWindow,
		bool isNotStandardWindow,
		bool hasNoVisibleWindow,
		bool cannotCreateWindow,
		bool shouldBeIgnored,
		IContext ctx,
		IInternalContext internalCtx
	)
	{
		// Given the handle fails
		HWND hwnd = (HWND)1;
		Setup(
			ctx,
			internalCtx,
			hwnd,
			isSplashScreen,
			isCloakedWindow,
			isNotStandardWindow,
			hasNoVisibleWindow,
			cannotCreateWindow,
			shouldBeIgnored
		);
		WindowAddedTransform sut = new(hwnd);

		// When we dispatch the transform
		var result = ctx.Store.Dispatch(sut);

		// Then we received an error
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success(IContext ctx, IInternalContext internalCtx)
	{
		// Given the handle succeeds
		HWND hwnd = (HWND)1;
		Setup(ctx, internalCtx, hwnd);
		WindowAddedTransform sut = new(hwnd);

		// When we dispatch the transform
		var result = ctx.Store.Dispatch(sut);

		// Then we receive the window
		Assert.True(result.IsSuccessful);
		Assert.IsAssignableFrom<IWindow>(result.Value);
	}
}
