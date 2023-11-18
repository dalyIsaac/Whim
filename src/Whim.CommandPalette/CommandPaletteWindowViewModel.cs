using System;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Windows.System;

namespace Whim.CommandPalette;

internal class CommandPaletteWindowViewModel : ICommandPaletteWindowViewModel
{
	private readonly IContext _context;

	private readonly IVariantControl _menuVariant;
	private readonly IVariantControl _freeTextVariant;
	private readonly IVariantControl _selectVariant;

	public BaseVariantConfig ActivationConfig { get; private set; }

	public IVariantControl? ActiveVariant { get; private set; }

	public ICommandPalettePlugin Plugin { get; }

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

	private string _confirmButtonText = "Confirm";
	public string ConfirmButtonText
	{
		get => _confirmButtonText;
		set
		{
			if (ConfirmButtonText != value)
			{
				_confirmButtonText = value;
				OnPropertyChanged(nameof(ConfirmButtonText));
			}
		}
	}

	public System.Windows.Input.ICommand ConfirmCommand { get; private set; }

	public bool IsVisible => Monitor != null;

	public event PropertyChangedEventHandler? PropertyChanged;

	public event EventHandler<EventArgs>? HideRequested;

	public event EventHandler<EventArgs>? FocusTextBoxRequested;

	public event EventHandler<EventArgs>? SetWindowPosRequested;

	public CommandPaletteWindowViewModel(
		IContext context,
		CommandPalettePlugin plugin,
		IVariantControl? menuVariant = null,
		IVariantControl? freeTextVariant = null,
		IVariantControl? selectVariant = null
	)
	{
		_context = context;
		Plugin = plugin;
		ActivationConfig =
			Plugin.Config.ActivationConfig ?? new MenuVariantConfig() { Commands = context.CommandManager };

		_menuVariant = menuVariant ?? new MenuVariantControl(context, this);
		_freeTextVariant = freeTextVariant ?? new FreeTextVariantControl(this);
		_selectVariant = selectVariant ?? new SelectVariantControl(this);

		ConfirmCommand = new ConfirmCommand(this);
	}

	/// <summary>
	/// Activate the command palette.
	/// </summary>
	/// <param name="config">The configuration for activation.</param>
	/// <param name="monitor">The monitor to display the command palette on.</param>
	public UIElement? Activate(BaseVariantConfig config, IMonitor? monitor)
	{
		ActiveVariant = config switch
		{
			MenuVariantConfig => _menuVariant,
			FreeTextVariantConfig => _freeTextVariant,
			SelectVariantConfig => _selectVariant,
			_ => null
		};

		if (ActiveVariant == null)
		{
			Logger.Error($"Unknown variant type: {config.GetType().Name}");
			return null;
		}

		ActivationConfig = config;
		Monitor = monitor ?? _context.MonitorManager.ActiveMonitor;

		ConfirmButtonText = ActivationConfig.ConfirmButtonText ?? "Confirm";
		Text = ActivationConfig.InitialText ?? "";
		PlaceholderText = ActivationConfig.Hint ?? "Start typing...";
		MaxHeight = (int)(Monitor.WorkingArea.Height * Plugin.Config.MaxHeightPercent / 100.0);

		ActiveVariant.ViewModel.Activate(ActivationConfig);
		SetWindowPosRequested?.Invoke(this, EventArgs.Empty);

		return ActiveVariant.Control;
	}

	public void RequestHide()
	{
		Logger.Debug("Request to hide the command palette window");
		HideRequested?.Invoke(this, EventArgs.Empty);

		_monitor = null;
		_text = "";
	}

	public void RequestFocusTextBox()
	{
		Logger.Debug("Request to focus the command palette text box");
		FocusTextBoxRequested?.Invoke(this, EventArgs.Empty);
	}

	public void OnKeyDown(VirtualKey key)
	{
		if (key == VirtualKey.Escape)
		{
			RequestHide();
		}
		else
		{
			ActiveVariant?.ViewModel.OnKeyDown(key);
		}
	}

	public void Update()
	{
		ActiveVariant?.ViewModel.Update();
		SetWindowPosRequested?.Invoke(this, EventArgs.Empty);
	}

	public double GetViewMaxHeight() => ActiveVariant?.GetViewMaxHeight() ?? 0;

	public bool IsConfigActive(BaseVariantConfig config) => ActivationConfig == config;

	protected virtual void OnPropertyChanged(string? propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
