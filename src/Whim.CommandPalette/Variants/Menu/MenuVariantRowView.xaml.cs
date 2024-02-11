using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Whim.CommandPalette;

/// <summary>
/// A palette row is a single command title, and an optional associated keybind.
/// </summary>
internal sealed partial class MenuVariantRowView
	: UserControl,
		IVariantRowView<MenuVariantRowModelData, MenuVariantRowViewModel>
{
	public static double MenuRowHeight => 36;

	private readonly IContext _context;

	public MenuVariantRowViewModel ViewModel { get; }

	public MenuVariantRowView(IContext context, MatcherResult<MenuVariantRowModelData> matcherResult)
	{
		_context = context;
		ViewModel = new MenuVariantRowViewModel(matcherResult);
		UIElementExtensions.InitializeComponent(this, "Whim.CommandPalette", "Variants/Menu/MenuVariantRowView");
	}

	public void Initialize()
	{
		this.SetTitle(CommandTitle.Inlines);
		SetKeybinds();
	}

	public void Update(MatcherResult<MenuVariantRowModelData> matcherResult)
	{
		Logger.Verbose("Updating with a new item");
		ViewModel.Update(matcherResult);
		this.SetTitle(CommandTitle.Inlines);
		SetKeybinds();
	}

	private void SetKeybinds()
	{
		Logger.Verbose("Setting keybinds");

		if (ViewModel.Model.Data.Keybind is not null)
		{
			CommandKeybind.Text = ViewModel.Model.Data.Keybind.ToString(_context.KeybindManager.UnifyKeyModifiers);
			CommandKeybindBorder.Visibility = Visibility.Visible;
		}
		else
		{
			CommandKeybindBorder.Visibility = Visibility.Collapsed;
		}
	}
}
