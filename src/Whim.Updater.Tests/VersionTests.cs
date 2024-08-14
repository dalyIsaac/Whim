using Xunit;

namespace Whim.Updater.Tests;

public class VersionTests
{
	[Theory]
	[InlineData("v0.1.263-alpha+bc5c56c4", 0, 1, 263, ReleaseChannel.Alpha, "bc5c56c4")]
	[InlineData("v10.10.10-beta+bc5c56c4", 10, 10, 10, ReleaseChannel.Beta, "bc5c56c4")]
	[InlineData("v1.0.0-stable+bc5c56c4", 1, 0, 0, ReleaseChannel.Stable, "bc5c56c4")]
	public void ParseTag(string tagName, int major, int minor, int patch, ReleaseChannel releaseChannel, string commit)
	{
		// Given
		Version? version = Version.ParseTag(tagName);

		// Then
		Assert.NotNull(version);
		Assert.Equal(major, version.Major);
		Assert.Equal(minor, version.Minor);
		Assert.Equal(patch, version.Patch);
		Assert.Equal(releaseChannel, version.ReleaseChannel);
		Assert.Equal(commit, version.Commit);
		Assert.Equal(tagName, version.ToString());
	}

	[Theory]
	[InlineData("v0.1.263-alpha+bc5c56c4123123")]
	[InlineData("10.10.10-beta+bc5c56c4")]
	[InlineData("v1.0.0+stable+bc5c56c4")]
	[InlineData("v1.0.0-stable-bc5c56c4")]
	public void ParseTag_Invalid(string tagName)
	{
		// Given
		Version? version = Version.ParseTag(tagName);

		// Then
		Assert.Null(version);
	}

	[Theory]
	[InlineData("0.1.263-alpha+bc5c56c4.012371231235123621", 0, 1, 263, ReleaseChannel.Alpha, "bc5c56c4")]
	[InlineData("10.10.10-beta+bc5c56c4.012371231235123621", 10, 10, 10, ReleaseChannel.Beta, "bc5c56c4")]
	[InlineData("1.0.0-stable+bc5c56c4.012371231235123621", 1, 0, 0, ReleaseChannel.Stable, "bc5c56c4")]
	public void ParseProductVersion(
		string tagName,
		int major,
		int minor,
		int patch,
		ReleaseChannel releaseChannel,
		string commit
	)
	{
		// Given
		Version? version = Version.ParseProductVersion(tagName);

		// Then
		Assert.NotNull(version);
		Assert.Equal(major, version.Major);
		Assert.Equal(minor, version.Minor);
		Assert.Equal(patch, version.Patch);
		Assert.Equal(releaseChannel, version.ReleaseChannel);
		Assert.Equal(commit, version.Commit);
	}

	[Theory]
	[InlineData("0.1.263-alpha+bc5c56c4123123.1723123801237")]
	[InlineData(".10.10-beta+bc5c56c4.1723123801237")]
	[InlineData("1.0.0+stable+bc5c56c4.1723123801237")]
	[InlineData("1.0.0-stable-bc5c56c4.1723123801237")]
	public void ParseProductVersion_Invalid(string tagName)
	{
		// Given
		Version? version = Version.ParseProductVersion(tagName);

		// Then
		Assert.Null(version);
	}

	[Theory]
	[InlineData("v0.1.263-alpha+bc5c56c4", "v0.1.263-alpha+bc5c56c4", false)]
	[InlineData("v0.1.263-beta+bc5c56c4", "v0.1.263-alpha+bc5c56c4", true)]
	[InlineData("v0.1.263-stable+bc5c56c4", "v0.1.263-beta+bc5c56c4", true)]
	[InlineData("v0.1.264-alpha+bc5c56c4", "v0.1.263-alpha+bc5c56c4", true)]
	[InlineData("v0.2.263-alpha+bc5c56c4", "v0.1.263-alpha+bc5c56c4", true)]
	[InlineData("v1.1.263-alpha+bc5c56c4", "v0.1.263-alpha+bc5c56c4", true)]
	public void IsNewerVersion(string tagName, string otherTagName, bool expected)
	{
		// Arrange
		Version version = Version.ParseTag(tagName)!;
		Version otherVersion = Version.ParseTag(otherTagName)!;

		// Given
		bool isNewer = version.IsNewerVersion(otherVersion);

		// Then
		Assert.Equal(expected, isNewer);
	}
}
