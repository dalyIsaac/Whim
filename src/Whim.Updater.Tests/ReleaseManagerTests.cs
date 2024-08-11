using System.Diagnostics.CodeAnalysis;
using Microsoft.UI.Dispatching;
using Microsoft.Windows.AppNotifications;
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
		ctx.NotificationManager.DidNotReceive().SendToastNotification(Arg.Any<AppNotification>());
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

		ReleaseManager sut = new(ctx, plugin);

		// When
		await sut.CheckForUpdates();

		// Then
		ctx.NotificationManager.DidNotReceive().SendToastNotification(Arg.Any<AppNotification>());
		ctx.NativeManager.Received(1).TryEnqueue(Arg.Any<DispatcherQueueHandler>());
	}
	#endregion
}
