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

		IReadOnlyList<ReleaseInfo> releases = new List<ReleaseInfo> { releaseInfo243, releaseInfo242 };
		fixture.Inject(releases);
	}
}

public class UpdaterWindowViewModelTests
{
	private const string ExpectedReleaseNotes =
		"<h1>v0.1.243-alpha+2ae89a20</h1><!-- Release notes generated using configuration in .github/release.yml at main -->\n<h2>What's Changed</h2>\n<h3>Other Changes 🖋</h3>\n<ul>\n<li>Automated app-rules upgrades by <a href=\"https://github.com/dalyIsaac\">@dalyIsaac</a> in <a href=\"https://github.com/dalyIsaac/Whim/pull/722\">https://github.com/dalyIsaac/Whim/pull/722</a></li>\n</ul>\n<p><strong>Full Changelog</strong>: <a href=\"https://github.com/dalyIsaac/Whim/compare/v0.1.269-alpha+150a91e8...v0.1.270-alpha+dfd0e637\">https://github.com/dalyIsaac/Whim/compare/v0.1.269-alpha+150a91e8...v0.1.270-alpha+dfd0e637</a></p>\n<h1>v0.1.242-alpha+823a398d</h1><!-- Release notes generated using configuration in .github/release.yml at main -->\n<h2>What's Changed</h2>\n<h3>Core</h3>\n<ul>\n<li>Add <code>IWorkspaceManager.MoveWindowToAdjacentWorkspace</code> by <a href=\"https://github.com/urob\">@urob</a> in <a href=\"https://github.com/dalyIsaac/Whim/pull/719\">https://github.com/dalyIsaac/Whim/pull/719</a></li>\n</ul>\n<p><strong>Full Changelog</strong>: <a href=\"https://github.com/dalyIsaac/Whim/compare/v0.1.270-alpha+dfd0e637...v0.1.271-alpha+5ef9529c\">https://github.com/dalyIsaac/Whim/compare/v0.1.270-alpha+dfd0e637...v0.1.271-alpha+5ef9529c</a></p>";

	[Theory, AutoSubstituteData<UpdaterWindowViewModelCustomization>]
	public void ReleaseNotes(IUpdaterPlugin updaterPlugin, IReadOnlyList<ReleaseInfo> releases)
	{
		// Given
		UpdaterWindowViewModel viewModel = new(updaterPlugin);

		// When
		viewModel.Update(releases);

		// Then
		Assert.Contains(ExpectedReleaseNotes, viewModel.ReleaseNotes);
	}

	[Theory, AutoSubstituteData<UpdaterWindowViewModelCustomization>]
	public void LastCheckedForUpdates_Never(IUpdaterPlugin updaterPlugin, IReadOnlyList<ReleaseInfo> releases)
	{
		// Given
		UpdaterWindowViewModel viewModel = new(updaterPlugin);

		// When
		viewModel.Update(releases);

		// Then
		Assert.Equal("Never", viewModel.LastCheckedForUpdates);
	}

	[Theory, AutoSubstituteData<UpdaterWindowViewModelCustomization>]
	public void LastCheckedForUpdates_DateTime(IUpdaterPlugin updaterPlugin, IReadOnlyList<ReleaseInfo> releases)
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
	public void LastRelease_Defined(IUpdaterPlugin updaterPlugin, IReadOnlyList<ReleaseInfo> releases)
	{
		// Given
		UpdaterWindowViewModel viewModel = new(updaterPlugin);

		// When
		viewModel.Update(releases);

		// Then
		Assert.Equal(releases[0], viewModel.LastRelease);
	}

	[Theory, AutoSubstituteData<UpdaterWindowViewModelCustomization>]
	public void SkippedReleases(IUpdaterPlugin updaterPlugin, IReadOnlyList<ReleaseInfo> releases)
	{
		// Given
		UpdaterWindowViewModel viewModel = new(updaterPlugin);

		// When
		viewModel.Update(releases);

		// Then
		Assert.Equal(2, viewModel.SkippedReleases);
	}

	[Theory, AutoSubstituteData<UpdaterWindowViewModelCustomization>]
	public void SkipReleaseCommand(IUpdaterPlugin updaterPlugin, IReadOnlyList<ReleaseInfo> releases)
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
	public void SkipReleaseCommand_CanExecute(IUpdaterPlugin updaterPlugin, IReadOnlyList<ReleaseInfo> releases)
	{
		// Given
		UpdaterWindowViewModel viewModel = new(updaterPlugin);
		viewModel.Update(releases);

		// When
		bool canExecute = viewModel.SkipReleaseCommand.CanExecute(null);

		// Then
		Assert.True(canExecute);
	}

	[Theory, AutoSubstituteData<UpdaterWindowViewModelCustomization>]
	public void InstallReleaseCommand(IUpdaterPlugin updaterPlugin, IReadOnlyList<ReleaseInfo> releases)
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
	public void InstallReleaseCommand_CanExecute(IUpdaterPlugin updaterPlugin, IReadOnlyList<ReleaseInfo> releases)
	{
		// Given
		UpdaterWindowViewModel viewModel = new(updaterPlugin);
		viewModel.Update(releases);

		// When
		bool canExecute = viewModel.InstallReleaseCommand.CanExecute(null);

		// Then
		Assert.True(canExecute);
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
	public void CloseUpdaterWindowCommand_CanExecute(IUpdaterPlugin updaterPlugin)
	{
		// Given
		UpdaterWindowViewModel viewModel = new(updaterPlugin);

		// When
		bool canExecute = viewModel.CloseUpdaterWindowCommand.CanExecute(null);

		// Then
		Assert.True(canExecute);
	}

	[Theory, AutoSubstituteData<UpdaterWindowViewModelCustomization>]
	public void InstallButtonText(IUpdaterPlugin updaterPlugin, IReadOnlyList<ReleaseInfo> releases)
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
