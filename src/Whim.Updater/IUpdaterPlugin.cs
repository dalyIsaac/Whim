using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;

namespace Whim.Updater;

public interface IUpdaterPlugin : IPlugin, IDisposable
{
	public Task<List<ReleaseInfo>> GetNotInstalledReleases();

	public Task InstallRelease(Release release);
}
