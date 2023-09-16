using Microsoft.UI.Xaml.Media.Imaging;
using Moq;
using System.IO;
using System.Text;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Xunit;

namespace Whim.Tests;

public class IconHelperTests
{
	private class Wrapper
	{
		public const string PackageXml = """
			<?xml version="1.0" encoding="utf-8"?>
			<Package xmlns="http://schemas.microsoft.com/appx/2010/manifest">
				<Identity Name="Contoso.MyApp" Publisher="CN=01234567-89ab-cdef-0123-456789abcdef" Version="1.0.0.0" />
				<Properties>
					<DisplayName>MyApp</DisplayName>
					<PublisherDisplayName>Contoso</PublisherDisplayName>
					<Logo>assets\StoreLogo.png</Logo>
				</Properties>
				<Resources>
					<Resource Language="en-us" />
				</Resources>
				<Applications>
					<Application Id="App" Executable="$targetnametoken$.exe" EntryPoint="MyApp.App">
						<VisualElements DisplayName="MyApp" Description="MyApp description" BackgroundColor="#FFFFFF" Square150x150Logo="assets\Square150x150Logo.png" Square44x44Logo="assets\Square44x44Logo.png">
							<DefaultTile Wide310x150Logo="assets\Wide310x150Logo.png" Square71x71Logo="assets\Square71x71Logo.png">
								<ShowNameOnTiles>
									<ShowOn Tile="square150x150Logo"/>
									<ShowOn Tile="wide310x150Logo"/>
								</ShowNameOnTiles>
							</DefaultTile>
							<SplashScreen Image="assets\SplashScreen.png" />
						</VisualElements>
					</Application>
				</Applications>
				<Dependencies>
					<TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.10240.0" MaxVersionTested="10.0.10586.0" />
				</Dependencies>
			</Package>
			""";

		public Mock<IInternalContext> InternalContext { get; } = new();
		public Mock<INativeManager> NativeManager { get; } = new();
		public Mock<IFileManager> FileManager { get; } = new();
		public Mock<IContext> Context { get; } = new();
		public Mock<IWindow> Window { get; } = new();

		public Wrapper()
		{
			Window.SetupGet(w => w.Handle).Returns((HWND)123);
			Context.SetupGet(c => c.NativeManager).Returns(NativeManager.Object);
			Context.SetupGet(c => c.FileManager).Returns(FileManager.Object);
		}

		public Wrapper WithIsUwp()
		{
			Window.SetupGet(w => w.IsUwp).Returns(true);
			return this;
		}

		public Wrapper WithUwpAppProcessPath(string? path)
		{
			NativeManager.Setup(n => n.GetUwpAppProcessPath(Window.Object)).Returns(path);
			return this;
		}

		public Wrapper FileExists(string path, bool exists)
		{
			FileManager.Setup(n => n.FileExists(path)).Returns(exists);
			return this;
		}

		public Wrapper OpenRead(string path, string content)
		{
			MemoryStream stream = new();
			stream.Write(Encoding.ASCII.GetBytes(content));
			stream.Position = 0;

			FileManager.Setup(n => n.OpenRead(path)).Returns(stream);
			return this;
		}

		public Wrapper SendMessage(int icon)
		{
			InternalContext
				.Setup(
					n => n.CoreNativeManager.SendMessage(Window.Object.Handle, PInvoke.WM_GETICON, PInvoke.ICON_BIG, 0)
				)
				.Returns((LRESULT)icon);
			return this;
		}

		public Wrapper GetClassLongPtr(nuint icon)
		{
			InternalContext
				.Setup(n => n.CoreNativeManager.GetClassLongPtr(Window.Object.Handle, GET_CLASS_LONG_INDEX.GCL_HICON))
				.Returns(icon);
			return this;
		}
	}

	[Fact]
	public void GetUwpAppIcon_NoExePath()
	{
		// Given
		Wrapper wrapper = new Wrapper().WithIsUwp().WithUwpAppProcessPath(null);

		// When
		BitmapImage? result = IconHelper.GetIcon(
			wrapper.Window.Object,
			wrapper.Context.Object,
			wrapper.InternalContext.Object
		);

		// Then
		Assert.Null(result);
	}

	[Fact]
	public void GetUwpAppIcon_NoExeDir()
	{
		// Given
		Wrapper wrapper = new Wrapper().WithIsUwp().WithUwpAppProcessPath("C:\\foo.exe");

		// When
		BitmapImage? result = IconHelper.GetIcon(
			wrapper.Window.Object,
			wrapper.Context.Object,
			wrapper.InternalContext.Object
		);

		// Then
		Assert.Null(result);
	}

	[Fact]
	public void GetUwpAppIcon_NoManifest()
	{
		// Given
		Wrapper wrapper = new Wrapper()
			.WithIsUwp()
			.WithUwpAppProcessPath("C:\\foo.exe")
			.FileExists("C:\\foo.exe", true)
			.FileExists("C:\\AppxManifest.xml", false);

		// When
		BitmapImage? result = IconHelper.GetIcon(
			wrapper.Window.Object,
			wrapper.Context.Object,
			wrapper.InternalContext.Object
		);

		// Then
		Assert.Null(result);
	}

	[Fact]
	public void GetUwpAppIcon_NoLogo()
	{
		// Given
		Wrapper wrapper = new Wrapper()
			.WithIsUwp()
			.WithUwpAppProcessPath("C:\\foo.exe")
			.FileExists("C:\\foo.exe", true)
			.FileExists("C:\\AppxManifest.xml", true)
			.OpenRead(
				"C:\\AppxManifest.xml",
				"<foo><Properties><SomethingElse>bar.png</SomethingElse></Properties></foo>"
			);

		// When
		BitmapImage? result = IconHelper.GetIcon(
			wrapper.Window.Object,
			wrapper.Context.Object,
			wrapper.InternalContext.Object
		);

		// Then
		Assert.Null(result);
	}

	[Fact]
	public void GetWindowIcon_NoIcon()
	{
		// Given
		Wrapper wrapper = new Wrapper().SendMessage(0).GetClassLongPtr(0);

		// When
		BitmapImage? result = IconHelper.GetIcon(
			wrapper.Window.Object,
			wrapper.Context.Object,
			wrapper.InternalContext.Object
		);

		// Then
		Assert.Null(result);
	}
}
