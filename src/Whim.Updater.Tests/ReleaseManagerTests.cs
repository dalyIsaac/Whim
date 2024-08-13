using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.UI.Dispatching;
using NSubstitute;
using Octokit;
using Whim.TestUtils;
using Xunit;

namespace Whim.Updater.Tests;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class ReleaseManagerTests
{
	#region CheckForUpdates
	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public async Task CheckForUpdates_WhenNoReleases(IContext ctx, IGitHubClient client)
	{
		// Given
		UpdaterPlugin plugin = new(ctx, new UpdaterConfig());
		ReleaseManager sut = new(ctx, plugin) { GitHubClient = client };

		// When
		await sut.CheckForUpdates();

		// Then
		ctx.NativeManager.DidNotReceive().TryEnqueue(Arg.Any<DispatcherQueueHandler>());
		ctx.Store.Received(1).Dispatch(Arg.Any<SaveStateTransform>());
		Assert.NotNull(plugin.LastCheckedForUpdates);
	}

	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public async Task CheckForUpdates_ReleaseIsSkipped(IContext ctx, IGitHubClient client)
	{
		// Given
		Release release = Data.CreateRelease242(tagName: "v0.1.265-alpha+bc5c56c4");
		client.Repository.Release.GetAll("dalyIsaac", "Whim", Arg.Any<ApiOptions>()).Returns([release]);

		UpdaterPlugin plugin = new(ctx, new UpdaterConfig() { ReleaseChannel = ReleaseChannel.Alpha });
		plugin.SkipRelease(release.TagName);
		ctx.Store.ClearReceivedCalls();

		ReleaseManager sut = new(ctx, plugin) { GitHubClient = client };

		// When
		await sut.CheckForUpdates();

		// Then
		ctx.NativeManager.DidNotReceive().TryEnqueue(Arg.Any<DispatcherQueueHandler>());
		ctx.Store.Received(1).Dispatch(Arg.Any<SaveStateTransform>());
		Assert.NotNull(plugin.LastCheckedForUpdates);
	}

	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public async Task CheckForUpdates_NewRelease(IContext ctx, IGitHubClient client)
	{
		// Given
		Release release = Data.CreateRelease242(tagName: "v0.1.265-alpha+bc5c56c4");
		client.Repository.Release.GetAll("dalyIsaac", "Whim", Arg.Any<ApiOptions>()).Returns([release]);

		UpdaterPlugin plugin = new(ctx, new UpdaterConfig() { ReleaseChannel = ReleaseChannel.Alpha });
		ReleaseManager sut = new(ctx, plugin) { GitHubClient = client };

		// When
		await sut.CheckForUpdates();

		// Then
		ctx.NativeManager.Received(1).TryEnqueue(Arg.Any<DispatcherQueueHandler>());
		ctx.Store.Received(1).Dispatch(Arg.Any<SaveStateTransform>());
		Assert.NotNull(plugin.LastCheckedForUpdates);
	}
	#endregion

	#region GetNotInstalledReleases
	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public async Task GetNotInstalledReleases_WhenNoReleases(IContext ctx, IGitHubClient client)
	{
		// Given
		UpdaterPlugin plugin = new(ctx, new UpdaterConfig());
		ReleaseManager sut = new(ctx, plugin) { GitHubClient = client };

		// When
		IEnumerable<ReleaseInfo> releases = await sut.GetNotInstalledReleases();

		// Then
		Assert.Empty(releases);
	}

	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public async Task GetNotInstalledReleases_InvalidVersion(IContext ctx, IGitHubClient client)
	{
		// Given
		UpdaterPlugin plugin = new(ctx, new UpdaterConfig());
		ReleaseManager sut = new(ctx, plugin) { GitHubClient = client };
		client
			.Repository.Release.GetAll("dalyIsaac", "Whim", Arg.Any<ApiOptions>())
			.Returns([Data.CreateRelease242(tagName: "welp")]);

		// When
		IEnumerable<ReleaseInfo> releases = await sut.GetNotInstalledReleases();

		// Then
		Assert.Empty(releases);
	}

	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public async Task GetNotInstalledReleases_DifferentChannel(IContext ctx, IGitHubClient client)
	{
		// Given
		UpdaterPlugin plugin = new(ctx, new UpdaterConfig());
		ReleaseManager sut = new(ctx, plugin) { GitHubClient = client };
		client
			.Repository.Release.GetAll("dalyIsaac", "Whim", Arg.Any<ApiOptions>())
			.Returns([Data.CreateRelease242(tagName: "v0.1.263-beta+bc5c56c4")]);

		// When
		IEnumerable<ReleaseInfo> releases = await sut.GetNotInstalledReleases();

		// Then
		Assert.Empty(releases);
	}

	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public async Task GetNotInstalledReleases_OlderVersion(IContext ctx, IGitHubClient client)
	{
		// Given
		UpdaterPlugin plugin = new(ctx, new UpdaterConfig());
		ReleaseManager sut = new(ctx, plugin) { GitHubClient = client };
		client
			.Repository.Release.GetAll("dalyIsaac", "Whim", Arg.Any<ApiOptions>())
			.Returns([Data.CreateRelease242(tagName: "v0.1.261-alpha+bc5c56c4")]);

		// When
		IEnumerable<ReleaseInfo> releases = await sut.GetNotInstalledReleases();

		// Then
		Assert.Empty(releases);
	}

	[Theory, AutoSubstituteData<UpdaterPluginCustomization>]
	public async Task GetNotInstalledReleases_Ordered(IContext ctx, IGitHubClient client)
	{
		// Given
		UpdaterPlugin plugin = new(ctx, new UpdaterConfig() { ReleaseChannel = ReleaseChannel.Alpha });
		ReleaseManager sut = new(ctx, plugin) { GitHubClient = client };

		string[] orderedReleases =
		[
			"v0.1.261-alpha+bc5c56c4",
			"v0.1.262-beta+bc5c56c4",
			"v0.1.263-stable+bc5c56c4",
			"v0.1.264-alpha+bc5c56c4",
			"v0.2.265-alpha+bc5c56c4",
			"v1.1.266-alpha+bc5c56c4"
		];
		string[] expectedReleases = ["v0.1.264-alpha+bc5c56c4", "v0.2.265-alpha+bc5c56c4", "v1.1.266-alpha+bc5c56c4"];

		client
			.Repository.Release.GetAll("dalyIsaac", "Whim", Arg.Any<ApiOptions>())
			.Returns(orderedReleases.Select(t => Data.CreateRelease242(tagName: t)).ToArray());

		// When
		IEnumerable<ReleaseInfo> releases = await sut.GetNotInstalledReleases();

		// Then
		releases.Select(r => r.Release.TagName).Should().BeEquivalentTo(expectedReleases);
	}
	#endregion
}
