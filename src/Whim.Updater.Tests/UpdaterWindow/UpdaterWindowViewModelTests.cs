using AutoFixture;
using NSubstitute;
using Octokit;
using Whim.TestUtils;
using Xunit;

namespace Whim.Updater.Tests;

public class UpdaterWindowViewModelCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		Release release243 = Data.CreateRelease243();
		Version version243 = Version.Parse(release243.TagName)!;
		ReleaseInfo releaseInfo243 = new(release243, version243);

		Release release242 = Data.CreateRelease242();
		Version version242 = Version.Parse(release242.TagName)!;
		ReleaseInfo releaseInfo242 = new(release242, version242);

		fixture.Inject(new List<ReleaseInfo> { releaseInfo243, releaseInfo242 });
	}
}

public class UpdaterWindowViewModelTests
{
	private const string ExpectedReleaseNotes =
		"\r\n<!DOCTYPE html>\r\n<html>\r\n\t<head>\r\n\t\t<title>Whim Updater</title>\r\n\t\t<meta charset=\"utf-8\" />\r\n\t\t<link name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />\r\n\t</head>\r\n\r\n\t<style>\r\n\t\thtml {\r\n\t\t\tfont-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;\r\n\t\t}\r\n\r\n\t\t:root {\r\n\t\t\t--border-color: rgb(216. 222, 228);\r\n\t\t\tfont-size: 14px;\r\n\t\t}\r\n\r\n\t\ta {\r\n\t\t\tcolor: rgb(9, 105, 218);\r\n\t\t}\r\n\r\n\t\t@media (prefers-color-scheme: dark) {\r\n\t\t\thtml {\r\n\t\t\t\tbackground: black;\r\n\t\t\t\tcolor: white;\r\n\t\t\t}\r\n\r\n\t\t\t:root {\r\n\t\t\t\t--border-color: rgb(33, 38, 45);\r\n\t\t\t}\r\n\r\n\t\t\ta {\r\n\t\t\t\tcolor: rgb(47, 129, 247);\r\n\t\t\t}\r\n\t\t}\r\n\r\n\t\th1, h2, h3 {\r\n\t\t\tmargin: 0;\r\n\t\t\tmargin-top: .2em;\r\n\t\t\tpadding-bottom: .3em;\r\n\t\t\tborder-bottom: 1px solid var(--border-color);\r\n\t\t}\r\n\r\n\t\th1 {\r\n\t\t\tmargin-top: 1em;\r\n\t\t}\r\n\t</style>\r\n\r\n\t<body>\r\n\t\t<h1>v0.1.242-alpha+823a398d</h1><!-- Release notes generated using configuration in .github/release.yml at main -->\n<h2>What's Changed</h2>\n<h3>Core</h3>\n<ul>\n<li>Add <code>IWorkspaceManager.MoveWindowToAdjacentWorkspace</code> by <a href=\"https://github.com/urob\"/>@urob</a> in <a href=\"https://github.com/dalyIsaac/Whim/pull/719\">https://github.com/dalyIsaac/Whim/pull/719</a></li>\n</ul>\n<p><strong>Full Changelog</strong>: <a href=\"https://github.com/dalyIsaac/Whim/compare/v0.1.270-alpha+dfd0e637...v0.1.271-alpha+5ef9529c\">https://github.com/dalyIsaac/Whim/compare/v0.1.270-alpha+dfd0e637...v0.1.271-alpha+5ef9529c</a></p>\n<h1>v0.1.243-alpha+2ae89a20</h1><!-- Release notes generated using configuration in .github/release.yml at main -->\n<h2>What's Changed</h2>\n<h3>Other Changes 🖋</h3>\n<ul>\n<li>Automated app-rules upgrades by <a href=\"https://github.com/dalyIsaac\"/>@dalyIsaac</a> in <a href=\"https://github.com/dalyIsaac/Whim/pull/722\">https://github.com/dalyIsaac/Whim/pull/722</a></li>\n</ul>\n<p><strong>Full Changelog</strong>: <a href=\"https://github.com/dalyIsaac/Whim/compare/v0.1.269-alpha+150a91e8...v0.1.270-alpha+dfd0e637\">https://github.com/dalyIsaac/Whim/compare/v0.1.269-alpha+150a91e8...v0.1.270-alpha+dfd0e637</a></p>\n\r\n\t</body>\r\n</html>\r\n";

	[Theory, AutoSubstituteData<UpdaterWindowViewModelCustomization>]
	public void ReleaseNotes(IUpdaterPlugin updaterPlugin, List<ReleaseInfo> releases)
	{
		// Given
		UpdaterWindowViewModel viewModel = new(updaterPlugin);

		// When
		viewModel.Update(releases);

		// Then
		Assert.Equal(ExpectedReleaseNotes, viewModel.ReleaseNotes);
	}

	[Theory, AutoSubstituteData<UpdaterWindowViewModelCustomization>]
	public void LastCheckedForUpdates_Never(IUpdaterPlugin updaterPlugin, List<ReleaseInfo> releases)
	{
		// Given
		UpdaterWindowViewModel viewModel = new(updaterPlugin);

		// When
		viewModel.Update(releases);

		// Then
		Assert.Equal("Never", viewModel.LastCheckedForUpdates);
	}

	[Theory, AutoSubstituteData<UpdaterWindowViewModelCustomization>]
	public void LastCheckedForUpdates_DateTime(IUpdaterPlugin updaterPlugin, List<ReleaseInfo> releases)
	{
		// Given
		DateTime lastCheckedForUpdates = DateTime.Now;
		updaterPlugin.LastCheckedForUpdates.Returns(lastCheckedForUpdates);
		UpdaterWindowViewModel viewModel = new(updaterPlugin);

		// When
		viewModel.Update(releases);

		// Then
		Assert.Equal(lastCheckedForUpdates.ToString(), viewModel.LastCheckedForUpdates);
	}

	[Theory, AutoSubstituteData<UpdaterWindowViewModelCustomization>]
	public void LastRelease_Null(IUpdaterPlugin updaterPlugin)
	{
		// Given
		UpdaterWindowViewModel viewModel = new(updaterPlugin);

		// When
		viewModel.Update(new List<ReleaseInfo>());

		// Then
		Assert.Null(viewModel.LastRelease);
	}

	[Theory, AutoSubstituteData<UpdaterWindowViewModelCustomization>]
	public void LastRelease_Defined(IUpdaterPlugin updaterPlugin, List<ReleaseInfo> releases)
	{
		// Given
		UpdaterWindowViewModel viewModel = new(updaterPlugin);

		// When
		viewModel.Update(releases);

		// Then
		Assert.Equal(releases[0], viewModel.LastRelease);
	}

	[Theory, AutoSubstituteData<UpdaterWindowViewModelCustomization>]
	public void SkippedReleases(IUpdaterPlugin updaterPlugin, List<ReleaseInfo> releases)
	{
		// Given
		UpdaterWindowViewModel viewModel = new(updaterPlugin);

		// When
		viewModel.Update(releases);

		// Then
		Assert.Equal(2, viewModel.SkippedReleases);
	}

	[Theory, AutoSubstituteData<UpdaterWindowViewModelCustomization>]
	public void SkipReleaseCommand(IUpdaterPlugin updaterPlugin, List<ReleaseInfo> releases)
	{
		// Given
		UpdaterWindowViewModel viewModel = new(updaterPlugin);
		viewModel.Update(releases);

		// When
		viewModel.SkipReleaseCommand.Execute(null);

		// Then
		updaterPlugin.Received(1).SkipRelease(releases[0].Release);
	}

	[Theory, AutoSubstituteData<UpdaterWindowViewModelCustomization>]
	public void InstallReleaseCommand(IUpdaterPlugin updaterPlugin, List<ReleaseInfo> releases)
	{
		// Given
		UpdaterWindowViewModel viewModel = new(updaterPlugin);
		viewModel.Update(releases);

		// When
		viewModel.InstallReleaseCommand.Execute(null);

		// Then
		updaterPlugin.Received(1).InstallRelease(releases[0].Release);
		updaterPlugin.Received(1).CloseUpdaterWindow();
	}

	[Theory, AutoSubstituteData<UpdaterWindowViewModelCustomization>]
	public void CloseUpdaterWindowCommand(IUpdaterPlugin updaterPlugin)
	{
		// Given
		UpdaterWindowViewModel viewModel = new(updaterPlugin);

		// When
		viewModel.CloseUpdaterWindowCommand.Execute(null);

		// Then
		updaterPlugin.Received(1).CloseUpdaterWindow();
	}

	[Theory, AutoSubstituteData<UpdaterWindowViewModelCustomization>]
	public void InstallButtonText(IUpdaterPlugin updaterPlugin, List<ReleaseInfo> releases)
	{
		// Given
		UpdaterWindowViewModel viewModel = new(updaterPlugin);
		viewModel.Update(releases);

		// When
		string installButtonText = viewModel.InstallButtonText;

		// Then
		Assert.Equal("Install v0.1.243-alpha+2ae89a20", installButtonText);
	}
}
