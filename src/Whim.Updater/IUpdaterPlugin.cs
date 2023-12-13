using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;

namespace Whim.Updater;

internal record SavedUpdaterPluginState(string? SkippedReleaseTagName, DateTime? LastCheckedForUpdates);

public interface IUpdaterPlugin : IPlugin, IDisposable
{
	DateTime? LastCheckedForUpdates { get; }

	Task<List<ReleaseInfo>> GetNotInstalledReleases();

	Task InstallRelease(Release release);

	void SkipRelease(Release release);
}
