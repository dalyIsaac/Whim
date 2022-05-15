using System;
using System.Collections.Generic;
using System.Text;

namespace Whim.CommandPalette;

/// <summary>
/// MRUMatcher will return matches in the order of the most recently used.
/// </summary>
public class MRUMatcher : ICommandPaletteMatcher
{
	public IEnumerable<CommandPaletteMatch> Match(
		string query,
		IEnumerable<(ICommand, IKeybind?)> items,
		IConfigContext configContext,
		CommandPalettePlugin plugin
	)
	{
		// Filter out the items which cannot be executed, or are not a match.
		List<(int, ICommand, IKeybind?)> filteredItems = new();
		foreach ((ICommand command, IKeybind? keybind) in items)
		{
			if (!command.CanExecute())
			{
				continue;
			}

			int startIdx = command.Title.IndexOf(query, StringComparison.OrdinalIgnoreCase);
			if (startIdx == -1)
			{
				continue;
			}

			filteredItems.Add((startIdx, command, keybind));
		}

		// Sort the filtered items by the start index of the match.
		filteredItems.Sort((a, b) => a.Item1.CompareTo(b.Item1));

		// Return the filtered items.
		foreach ((int startIdx, ICommand command, IKeybind? keybind) in filteredItems)
		{
			yield return new CommandPaletteMatch(
				CreateMatchText(startIdx, query.Length, command),
				command,
				keybind
			);
		}
	}

	/// <summary>
	/// CreateMatchText will create a markdown-formatted string representing the
	/// match, where the matched portion is bold.
	/// </summary>
	private static string CreateMatchText(int startIdx, int matchLength, ICommand command)
	{
		StringBuilder builder = new(command.Title.Length + 4);
		builder.Append(command.Title, 0, startIdx);
		builder.Append("**");
		builder.Append(command.Title, startIdx, matchLength);
		builder.Append("**");
		builder.Append(command.Title, startIdx + matchLength, command.Title.Length - startIdx - matchLength);
		return builder.ToString();
	}
}
