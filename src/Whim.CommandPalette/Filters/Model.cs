namespace Whim.CommandPalette;

/// <summary>
/// A section of a string that matches a filter.
/// </summary>
/// <param name="Start"></param>
/// <param name="End"></param>
public record FilterTextMatch(int Start, int End);

/// <summary>
/// A function that takes a word and returns its matches within the word to match against.
/// </summary>
/// <param name="word"></param>
/// <param name="wordToMatchAgainst"></param>
/// <returns>
/// <see langword="null"/> if the input was invalid, otherwise an array of matches.
/// Valid inputs may return an empty array when there are no matches.
/// </returns>
public delegate FilterTextMatch[]? PaletteFilter(string word, string wordToMatchAgainst);
