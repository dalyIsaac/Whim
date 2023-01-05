using Microsoft.UI.Xaml;
using System;
using System.ComponentModel;
using Windows.System;

namespace Whim.CommandPalette;

internal class CommandPaletteWindowViewModel : ICommandPaletteWindowViewModel
{
	private readonly IConfigContext _configContext;
	private BaseVariantConfig _activationConfig;

	private readonly IVariantControl<MenuVariantConfig> _menuVariant;

	private readonly IVariantControl<FreeTextVariantConfig> _freeTextVariant;

	private IVariantControl<BaseVariantConfig>? _activeVariant;

	public CommandPalettePlugin Plugin { get; }

	public int MaxHeight { get; set; }

	private IMonitor? _monitor;
	public IMonitor? Monitor
	{
		get => _monitor;
		private set
		{
			if (Monitor != value)
			{
				_monitor = value;
				OnPropertyChanged(nameof(IsVisible));
			}
		}
	}

	private string _text = "";
	public string Text
	{
		get => _text;
		set
		{
			if (Text != value)
			{
				_text = value;
				OnPropertyChanged(nameof(Text));
			}
		}
	}

	private string _placeholderText = "";
	public string PlaceholderText
	{
		get => _placeholderText;
		set
		{
			if (PlaceholderText != value)
			{
				_placeholderText = value;
				OnPropertyChanged(nameof(PlaceholderText));
			}
		}
	}

	public bool IsVisible => Monitor != null;

	public event PropertyChangedEventHandler? PropertyChanged;

	public event EventHandler<EventArgs>? HideRequested;

	public event EventHandler<EventArgs>? SetWindowPosRequested;

	public CommandPaletteWindowViewModel(
		IConfigContext configContext,
		CommandPalettePlugin plugin,
		IVariantControl<MenuVariantConfig>? menuVariant = null,
		IVariantControl<FreeTextVariantConfig>? freeTextVariant = null
	)
	{
		_configContext = configContext;
		Plugin = plugin;
		_activationConfig =
			Plugin.Config.ActivationConfig ?? new MenuVariantConfig() { Commands = configContext.CommandManager };

		_menuVariant = menuVariant ?? new MenuVariantControl(configContext, this);
		_freeTextVariant = freeTextVariant ?? new FreeTextVariantControl(this);
	}

	/// <summary>
	/// Activate the command palette.
	/// </summary>
	/// <param name="config">The configuration for activation.</param>
	/// <param name="monitor">The monitor to display the command palette on.</param>
	public UIElement? Activate(BaseVariantConfig config, IMonitor? monitor)
	{
		_activationConfig = config;
		Monitor = monitor ?? _configContext.MonitorManager.FocusedMonitor;

		Text = _activationConfig.InitialText ?? "";
		PlaceholderText = _activationConfig.Hint ?? "Start typing...";
		MaxHeight = (int)(Monitor.WorkingArea.Height * Plugin.Config.MaxHeightPercent / 100.0);

		_activeVariant = _activationConfig switch
		{
			MenuVariantConfig => (IVariantControl<BaseVariantConfig>)_menuVariant,
			FreeTextVariantConfig => (IVariantControl<BaseVariantConfig>)_freeTextVariant,
			_ => null
		};

		if (_activeVariant == null)
		{
			Logger.Error($"Unknown variant type: {config.GetType().Name}");
			return null;
		}

		_activeVariant.ViewModel.Activate(_activationConfig);
		SetWindowPosRequested?.Invoke(this, EventArgs.Empty);

		return _activeVariant.Control;
	}

	public void RequestHide()
	{
		Logger.Debug("Request to hide the command palette window");
		HideRequested?.Invoke(this, EventArgs.Empty);

		_monitor = null;
		_text = "";
	}

	public void OnKeyDown(VirtualKey key)
	{
		if (key == VirtualKey.Escape)
		{
			RequestHide();
		}
		else
		{
			_activeVariant?.ViewModel.OnKeyDown(key);
		}
	}

	public void Update()
	{
		_activeVariant?.ViewModel.Update();
		SetWindowPosRequested?.Invoke(this, EventArgs.Empty);
	}

	public double GetViewHeight() => _activeVariant?.GetViewHeight() ?? 0;

	public bool IsVariantActive(BaseVariantConfig config) => _activationConfig == config;

	protected virtual void OnPropertyChanged(string? propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
