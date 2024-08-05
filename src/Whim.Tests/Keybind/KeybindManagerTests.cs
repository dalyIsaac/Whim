using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim.Tests;

public class KeybindManagerTests
{
	[Theory, AutoSubstituteData]
	public void SetKeybind_DoesNotContainKeybind(IContext context, ICommand command)
	{
		// Given
		KeybindManager keybindManager = new(context);
		IKeybind keybind = new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_A);

		context.CommandManager.TryGetCommand("command").Returns(command);

		// When
		keybindManager.SetKeybind("command", keybind);

		// Then
		ICommand[] allCommands = keybindManager.GetCommands(keybind);
		Assert.Single(allCommands);
		Assert.Equal(command, allCommands[0]);
	}

	[Theory, AutoSubstituteData]
	public void SetKeybind_ContainsKeybind(IContext context, ICommand command, ICommand command2)
	{
		// Given
		KeybindManager keybindManager = new(context);
		IKeybind keybind = new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_A);

		context.CommandManager.TryGetCommand("command").Returns(command);
		context.CommandManager.TryGetCommand("command2").Returns(command2);

		// When
		keybindManager.SetKeybind("command", keybind);
		keybindManager.SetKeybind("command2", keybind);

		// Then
		ICommand[] allCommands = keybindManager.GetCommands(keybind);
		Assert.Equal(2, allCommands.Length);
		Assert.Equal(command, allCommands[0]);
		Assert.Equal(command2, allCommands[1]);
	}

	[Theory, AutoSubstituteData]
	public void SetKeybind_AlreadyContainsKeybindForCommand(IContext context, ICommand command)
	{
		// Given
		KeybindManager keybindManager = new(context);
		IKeybind keybind = new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_A);
		IKeybind keybind2 = new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_B);

		context.CommandManager.TryGetCommand("command").Returns(command);

		// When
		keybindManager.SetKeybind("command", keybind);

		// Then
		keybindManager.SetKeybind("command", keybind2);
		ICommand[] allCommands = keybindManager.GetCommands(keybind2);
		Assert.Single(allCommands);
		Assert.Equal(command, allCommands[0]);
	}

	[Theory, AutoSubstituteData]
	public void SetKeybind_UnifyKeyModifiers(IContext context, ICommand command)
	{
		// Given
		KeybindManager keybindManager = new(context);
		IKeybind keybind = new Keybind(KeyModifiers.RWin | KeyModifiers.RControl, VIRTUAL_KEY.VK_A);

		context.CommandManager.TryGetCommand("command").Returns(command);

		// When
		keybindManager.UnifyKeyModifiers = true;
		keybindManager.SetKeybind("command", keybind);

		// Then
		IKeybind? result = keybindManager.TryGetKeybind("command");
		Assert.NotNull(result);
		Assert.Equal(new Keybind(KeyModifiers.LWin | KeyModifiers.LControl, VIRTUAL_KEY.VK_A), result);
	}

	[Theory, AutoSubstituteData]
	public void SetKeybind_OverriddenKeybind(IContext context, ICommand command)
	{
		// Given
		KeybindManager keybindManager = new(context);
		IKeybind keybind1 = new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_A);
		IKeybind keybind2 = new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_B);

		context.CommandManager.TryGetCommand("command").Returns(command);

		// When
		keybindManager.SetKeybind("command", keybind1);
		keybindManager.SetKeybind("command", keybind2);

		// Then
		ICommand[] allCommands = keybindManager.GetCommands(keybind1);
		Assert.Empty(allCommands);
		allCommands = keybindManager.GetCommands(keybind2);
		Assert.Single(allCommands);
		Assert.Equal(command, allCommands[0]);
	}

	[Theory, AutoSubstituteData]
	public void GetCommands_DoesNotContainKeybind(IContext context)
	{
		// Given
		KeybindManager keybindManager = new(context);
		IKeybind keybind = new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_A);

		// When
		ICommand[] allCommands = keybindManager.GetCommands(keybind);

		// Then
		Assert.Empty(allCommands);
	}

	[Theory, AutoSubstituteData]
	public void GetCommands_DoesNotContainCommand(IContext context)
	{
		// Given
		KeybindManager keybindManager = new(context);
		IKeybind keybind = new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_A);

		context.CommandManager.TryGetCommand("command2").Returns((ICommand?)null);

		// When
		keybindManager.SetKeybind("command2", keybind);
		ICommand[] allCommands = keybindManager.GetCommands(keybind);

		// Then
		Assert.Empty(allCommands);
	}

	[Theory, AutoSubstituteData]
	public void GetCommands_Success(IContext context, ICommand command, ICommand command2)
	{
		// Given
		KeybindManager keybindManager = new(context);
		IKeybind keybind = new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_A);

		context.CommandManager.TryGetCommand("command").Returns(command);
		context.CommandManager.TryGetCommand("command2").Returns(command2);

		// When
		keybindManager.SetKeybind("command", keybind);
		keybindManager.SetKeybind("command2", keybind);

		// Then
		ICommand[] allCommands = keybindManager.GetCommands(keybind);
		Assert.Equal(2, allCommands.Length);
		Assert.Equal(command, allCommands[0]);
		Assert.Equal(command2, allCommands[1]);
	}

	[Theory, AutoSubstituteData]
	public void GetCommands_UnifyKeyModifiers(IContext context, ICommand command)
	{
		// Given
		KeybindManager keybindManager = new(context);
		IKeybind leftKeybind = new Keybind(KeyModifiers.LWin | KeyModifiers.LControl, VIRTUAL_KEY.VK_A);
		IKeybind rightKeybind = new Keybind(KeyModifiers.RWin | KeyModifiers.RControl, VIRTUAL_KEY.VK_A);

		context.CommandManager.TryGetCommand("command").Returns(command);

		// When
		keybindManager.UnifyKeyModifiers = true;
		keybindManager.SetKeybind("command", rightKeybind);

		// Then
		ICommand[] allCommands = keybindManager.GetCommands(leftKeybind);
		Assert.Single(allCommands);
		Assert.Equal(command, allCommands[0]);
	}

	[Theory, AutoSubstituteData]
	public void TryGet_DoesNotContainCommand(IContext context)
	{
		// Given
		KeybindManager keybindManager = new(context);

		// When
		IKeybind? keybind = keybindManager.TryGetKeybind("command");

		// Then
		Assert.Null(keybind);
	}

	[Theory, AutoSubstituteData]
	public void TryGet_ContainsCommand(IContext context)
	{
		// Given
		KeybindManager keybindManager = new(context);
		IKeybind keybind = new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_A);

		// When
		keybindManager.SetKeybind("command", keybind);
		IKeybind? result = keybindManager.TryGetKeybind("command");

		// Then
		Assert.Equal(keybind, result);
	}

	[Theory, AutoSubstituteData]
	public void Remove_DoesNotContainCommand(IContext context)
	{
		// Given
		KeybindManager keybindManager = new(context);

		// When
		bool result = keybindManager.Remove("command");

		// Then
		Assert.False(result);
	}

	[Theory, AutoSubstituteData]
	public void Remove_ContainsCommand(IContext context)
	{
		// Given
		KeybindManager keybindManager = new(context);
		IKeybind keybind = new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_A);

		// When
		keybindManager.SetKeybind("command", keybind);
		bool result = keybindManager.Remove("command");

		// Then
		Assert.True(result);
		Assert.Empty(keybindManager.GetCommands(keybind));
	}

	[Theory, AutoSubstituteData]
	public void UnifyKeyModifiers_SetToTrue(IContext context)
	{
		// Given
		KeybindManager keybindManager = new(context) { UnifyKeyModifiers = false };
		IKeybind keybind = new Keybind(KeyModifiers.RWin | KeyModifiers.RControl, VIRTUAL_KEY.VK_A);

		// When
		keybindManager.SetKeybind("command", keybind);
		keybindManager.UnifyKeyModifiers = true;

		// Then
		IKeybind? result = keybindManager.TryGetKeybind("command");
		Assert.NotNull(result);
		Assert.Equal(new Keybind(KeyModifiers.LWin | KeyModifiers.LControl, VIRTUAL_KEY.VK_A), result);
	}

	[Theory]
	[InlineAutoSubstituteData(KeyModifiers.LWin | KeyModifiers.LControl, VIRTUAL_KEY.VK_A)]
	[InlineAutoSubstituteData(KeyModifiers.RWin | KeyModifiers.RControl, VIRTUAL_KEY.VK_A)]
	[InlineAutoSubstituteData(KeyModifiers.LWin | KeyModifiers.RControl, VIRTUAL_KEY.VK_A)]
	[InlineAutoSubstituteData(KeyModifiers.RWin | KeyModifiers.LControl, VIRTUAL_KEY.VK_A)]
	public void GetCommands_Unified(KeyModifiers modifiers, VIRTUAL_KEY key, IContext context, ICommand command)
	{
		// Given
		KeybindManager keybindManager = new(context);
		IKeybind keybind = new Keybind(KeyModifiers.RWin | KeyModifiers.RControl, VIRTUAL_KEY.VK_A);

		context.CommandManager.TryGetCommand("command").Returns(command);

		// When
		keybindManager.SetKeybind("command", keybind);
		ICommand[] allCommands = keybindManager.GetCommands(new Keybind(modifiers, key));

		// Then
		Assert.Single(allCommands);
		Assert.Same(command, allCommands[0]);
	}

	[Theory, AutoSubstituteData]
	public void GetCommands_NotUnified_FailedLookup(IContext context)
	{
		// Given
		KeybindManager keybindManager = new(context) { UnifyKeyModifiers = false };
		IKeybind keybind = new Keybind(KeyModifiers.RWin | KeyModifiers.RControl, VIRTUAL_KEY.VK_A);

		// When
		keybindManager.SetKeybind("command", keybind);
		ICommand[] allCommands = keybindManager.GetCommands(
			new Keybind(KeyModifiers.LWin | KeyModifiers.LControl, VIRTUAL_KEY.VK_A)
		);

		// Then
		Assert.Empty(allCommands);
	}

	[Theory]
	[InlineAutoSubstituteData(KeyModifiers.LWin | KeyModifiers.LControl, VIRTUAL_KEY.VK_A)]
	[InlineAutoSubstituteData(KeyModifiers.RWin | KeyModifiers.RControl, VIRTUAL_KEY.VK_A)]
	[InlineAutoSubstituteData(KeyModifiers.LWin | KeyModifiers.RControl, VIRTUAL_KEY.VK_A)]
	public void GetCommands_NotUnified_Success(
		KeyModifiers modifiers,
		VIRTUAL_KEY key,
		IContext context,
		ICommand command
	)
	{
		// Given
		KeybindManager keybindManager = new(context) { UnifyKeyModifiers = false };
		IKeybind keybind = new Keybind(modifiers, key);

		context.CommandManager.TryGetCommand("command").Returns(command);

		// When
		keybindManager.SetKeybind("command", keybind);
		ICommand[] allCommands = keybindManager.GetCommands(keybind);

		// Then
		Assert.Single(allCommands);
		Assert.Same(command, allCommands[0]);
	}

	[Theory, AutoSubstituteData]
	public void Clear_CommandsCleared(IContext context)
	{
		// Given
		KeybindManager keybindManager = new(context);
		IKeybind keybind = new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_A);

		// When
		keybindManager.SetKeybind("command", keybind);
		keybindManager.Clear();

		// Then
		Assert.Empty(keybindManager.GetCommands(keybind));
		Assert.Null(keybindManager.TryGetKeybind("command"));
	}
}
