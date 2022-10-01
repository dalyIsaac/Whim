namespace Whim.CommandPalette;

/// <summary>
/// A section of a string that matches a filter.
/// </summary>
/// <param name="Start"></param>
/// <param name="End"></param>
public record PaletteFilterMatch(int Start, int End);

/// <summary>
/// A function that takes a word and returns its matches within the word to match against.
/// </summary>
/// <param name="word"></param>
/// <param name="wordToMatchAgainst"></param>
/// <returns></returns>
public delegate PaletteFilterMatch[]? PaletteFilter(string word, string wordToMatchAgainst);
