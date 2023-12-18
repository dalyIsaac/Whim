using Markdig;
using Xunit;

namespace Whim.Updater.Tests;

public class GitHubUserProfileExtensionTests
{
	[Theory]
	[InlineData("PR by @dalyIsaac", "<p>PR by <a href=\"https://github.com/dalyIsaac\">@dalyIsaac</a></p>\n")]
	[InlineData("PRby @dalyIsaac", "<p>PRby @dalyIsaac</p>\n")]
	[InlineData("PR by @*dalyIsaac*", "<p>PR by @<em>dalyIsaac</em></p>\n")]
	public void Match(string markdown, string expected)
	{
		// Arrange
		MarkdownPipeline pipeline = new MarkdownPipelineBuilder().Use<GitHubUserProfileExtension>().Build();

		// Act
		string html = Markdown.ToHtml(markdown, pipeline);

		// Assert
		Assert.Equal(expected, html);
	}
}
