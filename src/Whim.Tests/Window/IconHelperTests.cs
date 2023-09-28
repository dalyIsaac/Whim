using AutoFixture;
using Microsoft.UI.Xaml.Media.Imaging;
using NSubstitute;
using System.IO;
using System.Text;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim.Tests;

internal class IconHelperCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		IWindow window = fixture.Freeze<IWindow>();
		window.Handle.Returns((HWND)123);
	}
}

public class IconHelperTests
{
	[Theory, AutoSubstituteData<IconHelperCustomization>]
	internal void GetUwpAppIcon_NoExePath(IContext ctx, IInternalContext internalCtx, IWindow window)
	{
		// Given
		window.IsUwp.Returns(true);
		ctx.NativeManager.GetUwpAppProcessPath(window).Returns((string?)null);

		// When
		BitmapImage? result = IconHelper.GetIcon(window, ctx, internalCtx);

		// Then
		Assert.Null(result);
	}

	[Theory, AutoSubstituteData<IconHelperCustomization>]
	internal void GetUwpAppIcon_NoExeDir(IContext ctx, IInternalContext internalCtx, IWindow window)
	{
		// Given
		window.IsUwp.Returns(true);
		ctx.NativeManager.GetUwpAppProcessPath(window).Returns("C:\\foo.exe");

		// When
		BitmapImage? result = IconHelper.GetIcon(window, ctx, internalCtx);

		// Then
		Assert.Null(result);
	}

	[Theory, AutoSubstituteData<IconHelperCustomization>]
	internal void GetUwpAppIcon_NoManifest(IContext ctx, IInternalContext internalCtx, IWindow window)
	{
		// Given
		window.IsUwp.Returns(true);

		ctx.NativeManager.GetUwpAppProcessPath(window).Returns("C:\\foo.exe");
		ctx.FileManager.FileExists("C:\\foo.exe").Returns(true);
		ctx.FileManager.FileExists("C:\\AppxManifest.xml").Returns(false);

		// When
		BitmapImage? result = IconHelper.GetIcon(window, ctx, internalCtx);

		// Then
		Assert.Null(result);
	}

	[Theory, AutoSubstituteData<IconHelperCustomization>]
	internal void GetUwpAppIcon_NoLogo(IContext ctx, IInternalContext internalCtx, IWindow window)
	{
		// Given
		window.IsUwp.Returns(true);

		ctx.NativeManager.GetUwpAppProcessPath(window).Returns("C:\\foo.exe");
		ctx.FileManager.FileExists("C:\\foo.exe").Returns(true);
		ctx.FileManager.FileExists("C:\\AppxManifest.xml").Returns(true);

		MemoryStream stream = new();
		stream.Write(
			Encoding.ASCII.GetBytes("<foo><Properties><SomethingElse>bar.png</SomethingElse></Properties></foo>")
		);
		stream.Position = 0;
		ctx.FileManager.OpenRead("C:\\AppxManifest.xml").Returns(stream);

		// When
		BitmapImage? result = IconHelper.GetIcon(window, ctx, internalCtx);

		// Then
		Assert.Null(result);
	}

	[Theory, AutoSubstituteData<IconHelperCustomization>]
	internal void GetWindowIcon_NoIcon(IContext ctx, IInternalContext internalCtx, IWindow window)
	{
		// When
		BitmapImage? result = IconHelper.GetIcon(window, ctx, internalCtx);

		// Then
		Assert.Null(result);
	}
}
