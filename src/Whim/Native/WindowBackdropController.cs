using System;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using WinRT;

namespace Whim;

/// <summary>
/// The available backdrop types - see <see href="https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/materials"/>..
/// </summary>
public enum BackdropType
{
	/// <summary>
	/// No backdrop.
	/// </summary>
	None,

	/// <summary>
	/// Acrylic backdrop - see <see href="https://learn.microsoft.com/en-us/windows/apps/design/style/acrylic"/>.
	/// </summary>
	Acrylic,

	/// <summary>
	/// A more transparent Acrylic backdrop - based on the <see cref="Acrylic"/> backdrop.
	/// </summary>
	AcrylicThin,

	/// <summary>
	/// Mica backdrop - see <see href="https://learn.microsoft.com/en-us/windows/apps/design/style/mica"/>.
	/// Unsupported on Windows 10 - Whim will fall back to <see cref="Acrylic"/>.
	/// </summary>
	Mica,

	/// <summary>
	/// A Mica variant which looks different to <see cref="Mica"/> - see <see href="https://learn.microsoft.com/en-us/windows/apps/design/style/mica"/>
	/// Unsupported on Windows 10 - Whim will fall back to <see cref="Acrylic"/>.
	/// </summary>
	MicaAlt
}

/// <summary>
/// WinUI supports different backdrops/materials, depending on the OS version. To see the different
/// materials available, see
/// <see href="https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/materials"/>.
/// The different materials support differing levels and style of transparency.
///
/// This config lets you specify which backdrop to use for a given Whim window.
/// </summary>
/// <param name="Backdrop">
/// The backdrop type to use.
/// </param>
/// <param name="AlwaysShowBackdrop">
/// By default, WinUI will disable the backdrop when the window loses focus. Whim overrides this setting.
/// Set this to <see langword="false"/> to disable the backdrop when the window loses focus.
/// </param>
public record WindowBackdropConfig(BackdropType Backdrop, bool AlwaysShowBackdrop = true);

/// <summary>
/// Manages the system backdrop for a <see cref="Microsoft.UI.Xaml.Window"/>.
/// </summary>
public class WindowBackdropController : IDisposable
{
	private readonly Microsoft.UI.Xaml.Window _window;
	private readonly WindowBackdropConfig _config;
	private bool _disposedValue;
	private SystemBackdropConfiguration? _backdropConfiguration;
	private DesktopAcrylicController? _acrylicController;
	private MicaController? _micaController;

	/// <summary>
	/// Creates a new <see cref="WindowBackdropController"/>.
	/// </summary>
	/// <param name="window">
	/// The <see cref="Microsoft.UI.Xaml.Window"/> to manage the backdrop for.
	/// </param>
	/// <param name="config">
	/// The configuration for the backdrop.
	/// </param>
	public WindowBackdropController(Microsoft.UI.Xaml.Window window, WindowBackdropConfig config)
	{
		_window = window;
		_config = config;

		// Default to Mica for now.
		TrySetBackdrop();
	}

	private void TrySetBackdrop()
	{
		if (_disposedValue)
		{
			Logger.Error("Cannot set backdrop on disposed controller");
			return;
		}

		if (_config.Backdrop == BackdropType.None)
		{
			Logger.Debug("No backdrop set");
			return;
		}

		BackdropType backdrop = _config.Backdrop;

		bool isAcrylic = backdrop == BackdropType.Acrylic || backdrop == BackdropType.AcrylicThin;
		bool isMica = backdrop == BackdropType.Mica || backdrop == BackdropType.MicaAlt;

		if (isMica && !MicaController.IsSupported())
		{
			Logger.Error("Mica is not supported on this system, falling back to Acrylic");
			backdrop = BackdropType.Acrylic;
		}

		if (isAcrylic && !DesktopAcrylicController.IsSupported())
		{
			Logger.Error("Acrylic is not supported on this system");
			return;
		}

		_backdropConfiguration ??= new();

		// Set up events, if they haven't been set up yet.
		_window.Activated += Window_Activated;
		_window.Closed += Window_Closed;
		((FrameworkElement)_window.Content).ActualThemeChanged += Window_ThemeChanged;

		// Initial configuration state.
		_backdropConfiguration.IsInputActive = true;
		SetConfigurationSourceTheme();

		// Set up the backdrop.
		if (isAcrylic)
		{
			SetupAcrylicBackdrop(backdrop);
		}
		else if (isMica)
		{
			SetupMicaBackdrop(backdrop);
		}
	}

	private void Window_Activated(object sender, WindowActivatedEventArgs e)
	{
		if (!_config.AlwaysShowBackdrop && _backdropConfiguration != null)
		{
			_backdropConfiguration.IsInputActive = e.WindowActivationState != WindowActivationState.Deactivated;
		}
	}

	private void Window_Closed(object sender, Microsoft.UI.Xaml.WindowEventArgs e) => DisposeWindowItems();

	private void Window_ThemeChanged(FrameworkElement sender, object args)
	{
		if (_backdropConfiguration != null)
		{
			SetConfigurationSourceTheme();
		}
	}

	private void SetConfigurationSourceTheme()
	{
		if (_backdropConfiguration == null)
		{
			return;
		}

		switch (((FrameworkElement)_window.Content).ActualTheme)
		{
			case ElementTheme.Dark:
				_backdropConfiguration.Theme = SystemBackdropTheme.Dark;
				break;
			case ElementTheme.Light:
				_backdropConfiguration.Theme = SystemBackdropTheme.Light;
				break;
			case ElementTheme.Default:
				_backdropConfiguration.Theme = SystemBackdropTheme.Default;
				break;
		}
	}

	private void SetupAcrylicBackdrop(BackdropType backdrop)
	{
		_acrylicController ??= new DesktopAcrylicController
		{
			Kind = backdrop == BackdropType.AcrylicThin ? DesktopAcrylicKind.Thin : DesktopAcrylicKind.Base
		};

		// Enable the system backdrop.
		_acrylicController.AddSystemBackdropTarget(
			_window.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>()
		);
		_acrylicController.SetSystemBackdropConfiguration(_backdropConfiguration);
	}

	private void SetupMicaBackdrop(BackdropType backdrop)
	{
		_micaController ??= new MicaController
		{
			Kind = backdrop == BackdropType.MicaAlt ? MicaKind.BaseAlt : MicaKind.Base
		};

		_micaController.AddSystemBackdropTarget(
			_window.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>()
		);
		_micaController.SetSystemBackdropConfiguration(_backdropConfiguration);
	}

	/// <summary>
	/// Make sure any Mica/Acrylic controller is disposed so it doesn't try to
	/// use this closed window.
	/// </summary>
	private void DisposeWindowItems()
	{
		if (_micaController != null)
		{
			_micaController.Dispose();
			_micaController = null;
		}

		if (_acrylicController != null)
		{
			_acrylicController.Dispose();
			_acrylicController = null;
		}

		_window.Activated -= Window_Activated;
		_window.Closed -= Window_Closed;
	}

	/// <inheritdoc/>
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				DisposeWindowItems();
			}

			// free unmanaged resources (unmanaged objects) and override finalizer
			// set large fields to null
			_disposedValue = true;
		}
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
