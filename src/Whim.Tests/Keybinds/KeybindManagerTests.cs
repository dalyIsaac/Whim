using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Xunit;

namespace Whim.Tests;

public class KeybindManagerTests
{
	[Theory, AutoSubstituteData]
	public void Add_DoesNotContainKeybind(IContext context, ICommand command)
	{
		// Given
		IKeybindManager keybindManager = new KeybindManager(context);
		IKeybind keybind = new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_A);

		context.CommandManager.TryGetCommand("command").Returns(command);

		// When
		keybindManager.Add("command", keybind);

		// Then
		ICommand[] allCommands = keybindManager.GetCommands(keybind);
		Assert.Single(allCommands);
		Assert.Equal(command, allCommands[0]);
	}

	[Theory, AutoSubstituteData]
	public void Add_ContainsKeybind(IContext context, ICommand command, ICommand command2)
	{
		// Given
		IKeybindManager keybindManager = new KeybindManager(context);
		IKeybind keybind = new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_A);

		context.CommandManager.TryGetCommand("command").Returns(command);
		context.CommandManager.TryGetCommand("command2").Returns(command2);

		// When
		keybindManager.Add("command", keybind);
		keybindManager.Add("command2", keybind);

		// Then
		ICommand[] allCommands = keybindManager.GetCommands(keybind);
		Assert.Equal(2, allCommands.Length);
		Assert.Equal(command, allCommands[0]);
		Assert.Equal(command2, allCommands[1]);
	}

	[Theory, AutoSubstituteData]
	public void Add_UnifyKeyModifiers(IContext context, ICommand command)
	{
		// Given
		IKeybindManager keybindManager = new KeybindManager(context);
		IKeybind keybind = new Keybind(KeyModifiers.RWin | KeyModifiers.RControl, VIRTUAL_KEY.VK_A);

		context.CommandManager.TryGetCommand("command").Returns(command);

		// When
		keybindManager.UnifyKeyModifiers = true;
		keybindManager.Add("command", keybind);

		// Then
		IKeybind? result = keybindManager.TryGetKeybind("command");
		Assert.NotNull(result);
		Assert.Equal(new Keybind(KeyModifiers.LWin | KeyModifiers.LControl, VIRTUAL_KEY.VK_A), result);
	}

	[Theory, AutoSubstituteData]
	public void GetCommands_DoesNotContainKeybind(IContext context)
	{
		// Given
		IKeybindManager keybindManager = new KeybindManager(context);
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
		IKeybindManager keybindManager = new KeybindManager(context);
		IKeybind keybind = new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_A);

		context.CommandManager.TryGetCommand("command2").Returns((ICommand?)null);

		// When
		keybindManager.Add("command2", keybind);
		ICommand[] allCommands = keybindManager.GetCommands(keybind);

		// Then
		Assert.Empty(allCommands);
	}

	[Theory, AutoSubstituteData]
	public void GetCommands_Success(IContext context, ICommand command, ICommand command2)
	{
		// Given
		IKeybindManager keybindManager = new KeybindManager(context);
		IKeybind keybind = new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_A);

		context.CommandManager.TryGetCommand("command").Returns(command);
		context.CommandManager.TryGetCommand("command2").Returns(command2);

		// When
		keybindManager.Add("command", keybind);
		keybindManager.Add("command2", keybind);

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
		IKeybindManager keybindManager = new KeybindManager(context);
		IKeybind leftKeybind = new Keybind(KeyModifiers.LWin | KeyModifiers.LControl, VIRTUAL_KEY.VK_A);
		IKeybind rightKeybind = new Keybind(KeyModifiers.RWin | KeyModifiers.RControl, VIRTUAL_KEY.VK_A);

		context.CommandManager.TryGetCommand("command").Returns(command);

		// When
		keybindManager.UnifyKeyModifiers = true;
		keybindManager.Add("command", rightKeybind);

		// Then
		ICommand[] allCommands = keybindManager.GetCommands(leftKeybind);
		Assert.Single(allCommands);
		Assert.Equal(command, allCommands[0]);
	}

	[Theory, AutoSubstituteData]
	public void TryGet_DoesNotContainCommand(IContext context)
	{
		// Given
		IKeybindManager keybindManager = new KeybindManager(context);

		// When
		IKeybind? keybind = keybindManager.TryGetKeybind("command");

		// Then
		Assert.Null(keybind);
	}

	[Theory, AutoSubstituteData]
	public void TryGet_ContainsCommand(IContext context)
	{
		// Given
		IKeybindManager keybindManager = new KeybindManager(context);
		IKeybind keybind = new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_A);

		// When
		keybindManager.Add("command", keybind);
		IKeybind? result = keybindManager.TryGetKeybind("command");

		// Then
		Assert.Equal(keybind, result);
	}

	[Theory, AutoSubstituteData]
	public void Remove_DoesNotContainCommand(IContext context)
	{
		// Given
		IKeybindManager keybindManager = new KeybindManager(context);

		// When
		bool result = keybindManager.Remove("command");

		// Then
		Assert.False(result);
	}

	[Theory, AutoSubstituteData]
	public void Remove_ContainsCommand(IContext context)
	{
		// Given
		IKeybindManager keybindManager = new KeybindManager(context);
		IKeybind keybind = new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_A);

		// When
		keybindManager.Add("command", keybind);
		bool result = keybindManager.Remove("command");

		// Then
		Assert.True(result);
		Assert.Empty(keybindManager.GetCommands(keybind));
	}

	[Theory, AutoSubstituteData]
	public void UnifyKeyModifiers_SetToTrue(IContext context)
	{
		// Given
		IKeybindManager keybindManager = new KeybindManager(context) { UnifyKeyModifiers = false };
		IKeybind keybind = new Keybind(KeyModifiers.RWin | KeyModifiers.RControl, VIRTUAL_KEY.VK_A);

		// When
		keybindManager.Add("command", keybind);
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
		IKeybindManager keybindManager = new KeybindManager(context);
		IKeybind keybind = new Keybind(KeyModifiers.RWin | KeyModifiers.RControl, VIRTUAL_KEY.VK_A);

		context.CommandManager.TryGetCommand("command").Returns(command);

		// When
		keybindManager.Add("command", keybind);
		ICommand[] allCommands = keybindManager.GetCommands(new Keybind(modifiers, key));

		// Then
		Assert.Single(allCommands);
		Assert.Same(command, allCommands[0]);
	}
}
