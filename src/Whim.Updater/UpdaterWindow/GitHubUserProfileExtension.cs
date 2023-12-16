// Based on https://github.com/xoofx/markdig/issues/748

using System.Text.RegularExpressions;
using Markdig;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace Whim.Updater;

internal class GitHubUserProfileExtension : IMarkdownExtension
{
	public void Setup(MarkdownPipelineBuilder pipeline)
	{
		if (!pipeline.InlineParsers.Contains<GitHubUserProfileParser>())
		{
			pipeline.InlineParsers.Insert(0, new GitHubUserProfileParser());
		}
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer) { }
}

internal partial class GitHubUserProfileParser : InlineParser
{
	public GitHubUserProfileParser()
	{
		OpeningCharacters = new[] { 'b' };
	}

	public override bool Match(InlineProcessor processor, ref StringSlice slice)
	{
		char precedingCharacter = slice.PeekCharExtra(-1);
		if (!precedingCharacter.IsWhiteSpaceOrZero())
		{
			return false;
		}

		while (!slice.Match("by @"))
		{
			// keep skipping
			slice.NextChar();
		}

		Match match = GitHubUserRegex().Match(slice.ToString());

		if (!match.Success)
		{
			return false;
		}

		string username = match.Groups["username"].Value;
		string literal = $"by <a href=\"https://github.com/{username}\"/>{username}</a>";

		processor.Inline = new HtmlInline(literal)
		{
			Span = { Start = processor.GetSourcePosition(slice.Start, out int line, out int column) },
			Line = line,
			Column = column,
			IsClosed = true
		};
		processor.Inline.Span.End = processor.Inline.Span.Start + match.Length - 1;
		slice.Start += match.Length;
		return true;
	}

	[GeneratedRegex("by @(?<username>[a-zA-Z0-9-]+)")]
	private static partial Regex GitHubUserRegex();
}
