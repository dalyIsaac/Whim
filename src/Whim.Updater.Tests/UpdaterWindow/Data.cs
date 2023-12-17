using Octokit;

namespace Whim.Updater.Tests;

internal static class Data
{
	private static Author CreateAuthor() =>
		new(
			login: "github-actions[bot]",
			id: 41898282,
			nodeId: "MDM6Qm90NDE4OTgyODI=",
			avatarUrl: "https://avatars.githubusercontent.com/in/15368?v=4",
			url: "https://api.github.com/users/github-actions%5Bbot%5D",
			htmlUrl: "https://github.com/apps/github-actions",
			followersUrl: "https://api.github.com/users/github-actions%5Bbot%5D/followers",
			followingUrl: "https://api.github.com/users/github-actions%5Bbot%5D/following{/other_user}",
			gistsUrl: "https://api.github.com/users/github-actions%5Bbot%5D/gists{/gist_id}",
			starredUrl: "https://api.github.com/users/github-actions%5Bbot%5D/starred{/owner}{/repo}",
			subscriptionsUrl: "https://api.github.com/users/github-actions%5Bbot%5D/subscriptions",
			organizationsUrl: "https://api.github.com/users/github-actions%5Bbot%5D/orgs",
			reposUrl: "https://api.github.com/users/github-actions%5Bbot%5D/repos",
			eventsUrl: "https://api.github.com/users/github-actions%5Bbot%5D/events{/privacy}",
			receivedEventsUrl: "https://api.github.com/users/github-actions%5Bbot%5D/received_events",
			type: "Bot",
			siteAdmin: false
		);

	private const string Release1Body =
		@"<!-- Release notes generated using configuration in .github/release.yml at main -->

## What's Changed
### Core
* Add `IWorkspaceManager.MoveWindowToAdjacentWorkspace` by @urob in https://github.com/dalyIsaac/Whim/pull/719


**Full Changelog**: https://github.com/dalyIsaac/Whim/compare/v0.1.270-alpha+dfd0e637...v0.1.271-alpha+5ef9529c
";

	public static Release CreateRelease1()
	{
		ReleaseAsset[] assets = new ReleaseAsset[2];

		assets[0] = new ReleaseAsset(
			url: "https://api.github.com/repos/dalyIsaac/Whim/releases/assets/137453491",
			id: 137453491,
			nodeId: "RA_kwDOGWZ1Ts4IMV-z",
			name: "WhimInstaller-arm64-0.1.242-alpha+823a398d.823a398db7a01e1c50dc296c9dd62782007025dd.exe",
			label: "",
			contentType: "application/x-msdownload",
			state: "uploaded",
			size: 24339890,
			downloadCount: 1,
			createdAt: DateTimeOffset.Parse("2023-11-26T02:09:15Z"),
			updatedAt: DateTimeOffset.Parse("2023-11-26T02:09:16Z"),
			browserDownloadUrl: "https://github.com/dalyIsaac/Whim/releases/download/v0.1.242-alpha%2B823a398d/WhimInstaller-arm64-0.1.242-alpha%2B823a398d.823a398db7a01e1c50dc296c9dd62782007025dd.exe",
			uploader: CreateAuthor()
		);

		assets[1] = new ReleaseAsset(
			url: "https://api.github.com/repos/dalyIsaac/Whim/releases/assets/137453051",
			id: 137453051,
			nodeId: "RA_kwDOGWZ1Ts4IMV37",
			name: "WhimInstaller-x64-0.1.242-alpha+823a398d.823a398db7a01e1c50dc296c9dd62782007025dd.exe",
			label: "",
			contentType: "application/x-msdownload",
			state: "uploaded",
			size: 25963657,
			downloadCount: 1,
			createdAt: DateTimeOffset.Parse("2023-11-26T02:04:03Z"),
			updatedAt: DateTimeOffset.Parse("2023-11-26T02:04:04Z"),
			browserDownloadUrl: "https://github.com/dalyIsaac/Whim/releases/download/v0.1.242-alpha%2B823a398d/WhimInstaller-x64-0.1.242-alpha%2B823a398d.823a398db7a01e1c50dc296c9dd62782007025dd.exe",
			uploader: CreateAuthor()
		);

		return new Release(
			url: "https://api.github.com/repos/dalyIsaac/Whim/releases/131450677",
			assetsUrl: "https://api.github.com/repos/dalyIsaac/Whim/releases/131450677/assets",
			uploadUrl: "https://uploads.github.com/repos/dalyIsaac/Whim/releases/131450677/assets{?name,label}",
			htmlUrl: "https://github.com/dalyIsaac/Whim/releases/tag/v0.1.242-alpha%2B823a398d",
			id: 131450677,
			nodeId: "RE_kwDOGWZ1Ts4H1cc1",
			tagName: "v0.1.242-alpha+823a398d",
			targetCommitish: "main",
			name: "v0.1.242-alpha+823a398d",
			draft: false,
			prerelease: true,
			createdAt: DateTimeOffset.Parse("2023-11-26T02:00:16Z"),
			publishedAt: DateTimeOffset.Parse("2023-11-26T02:00:36Z"),
			tarballUrl: "https://api.github.com/repos/dalyIsaac/Whim/tarball/v0.1.242-alpha+823a398d",
			zipballUrl: "https://api.github.com/repos/dalyIsaac/Whim/zipball/v0.1.242-alpha+823a398d",
			body: Release1Body,
			author: CreateAuthor(),
			assets: assets
		);
	}

	private const string Release2Body =
		@"<!-- Release notes generated using configuration in .github/release.yml at main -->

## What's Changed
### Other Changes 🖋
* Automated app-rules upgrades by @dalyIsaac in https://github.com/dalyIsaac/Whim/pull/722


**Full Changelog**: https://github.com/dalyIsaac/Whim/compare/v0.1.269-alpha+150a91e8...v0.1.270-alpha+dfd0e637
";

	public static Release CreateRelease2()
	{
		ReleaseAsset[] assets = new ReleaseAsset[2];

		assets[0] = new ReleaseAsset(
			url: "https://api.github.com/repos/dalyIsaac/Whim/releases/assets/137468755",
			id: 137468755,
			nodeId: "RA_kwDOGWZ1Ts4IMZtT",
			name: "WhimInstaller-arm64-0.1.243-alpha+2ae89a20.2ae89a204825f950e0be5b712b56cd5929e94adb.exe",
			label: "",
			contentType: "application/x-msdownload",
			state: "uploaded",
			size: 24362101,
			downloadCount: 1,
			createdAt: DateTimeOffset.Parse("2023-11-26T05:03:23Z"),
			updatedAt: DateTimeOffset.Parse("2023-11-26T05:03:25Z"),
			browserDownloadUrl: "https://github.com/dalyIsaac/Whim/releases/download/v0.1.243-alpha%2B2ae89a20/WhimInstaller-arm64-0.1.243-alpha%2B2ae89a20.2ae89a204825f950e0be5b712b56cd5929e94adb.exe",
			uploader: CreateAuthor()
		);

		assets[1] = new ReleaseAsset(
			url: "https://api.github.com/repos/dalyIsaac/Whim/releases/assets/137468478",
			id: 137468478,
			nodeId: "RA_kwDOGWZ1Ts4IMZo-",
			name: "WhimInstaller-x64-0.1.243-alpha+2ae89a20.2ae89a204825f950e0be5b712b56cd5929e94adb.exe",
			label: "",
			contentType: "application/x-msdownload",
			state: "uploaded",
			size: 25945113,
			downloadCount: 8,
			createdAt: DateTimeOffset.Parse("2023-11-26T05:00:29Z"),
			updatedAt: DateTimeOffset.Parse("2023-11-26T05:00:30Z"),
			browserDownloadUrl: "https://github.com/dalyIsaac/Whim/releases/download/v0.1.243-alpha%2B2ae89a20/WhimInstaller-x64-0.1.243-alpha%2B2ae89a20.2ae89a204825f950e0be5b712b56cd5929e94adb.exe",
			uploader: CreateAuthor()
		);

		return new Release(
			url: "https://api.github.com/repos/dalyIsaac/Whim/releases/131453811",
			assetsUrl: "https://api.github.com/repos/dalyIsaac/Whim/releases/131453811/assets",
			uploadUrl: "https://uploads.github.com/repos/dalyIsaac/Whim/releases/131453811/assets{?name,label}",
			htmlUrl: "https://github.com/dalyIsaac/Whim/releases/tag/v0.1.243-alpha%2B2ae89a20",
			id: 131453811,
			nodeId: "RE_kwDOGWZ1Ts4H1dNz",
			tagName: "v0.1.243-alpha+2ae89a20",
			targetCommitish: "main",
			name: "v0.1.243-alpha+2ae89a20",
			draft: false,
			prerelease: true,
			createdAt: DateTimeOffset.Parse("2023-11-26T04:57:35Z"),
			publishedAt: DateTimeOffset.Parse("2023-11-26T04:57:56Z"),
			tarballUrl: "https://api.github.com/repos/dalyIsaac/Whim/tarball/v0.1.243-alpha+2ae89a20",
			zipballUrl: "https://api.github.com/repos/dalyIsaac/Whim/zipball/v0.1.243-alpha+2ae89a20",
			body: Release2Body,
			author: CreateAuthor(),
			assets: assets
		);
	}
}
