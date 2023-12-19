using Xunit;

namespace Whim.Updater.Tests;

public class VersionTests
{
	[Theory]
	[InlineData("v0.1.263-alpha+bc5c56c4", 0, 1, 263, ReleaseChannel.Alpha, "bc5c56c4")]
	[InlineData("v10.10.10-beta+bc5c56c4", 10, 10, 10, ReleaseChannel.Beta, "bc5c56c4")]
	[InlineData("v1.0.0-stable+bc5c56c4", 1, 0, 0, ReleaseChannel.Stable, "bc5c56c4")]
	public void Parse(string tagName, int major, int minor, int patch, ReleaseChannel releaseChannel, string commit)
	{
		// Given
		Version? version = Version.Parse(tagName);

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
	public void Parse_Invalid(string tagName)
	{
		// Given
		Version? version = Version.Parse(tagName);

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
		Version version = Version.Parse(tagName)!;
		Version otherVersion = Version.Parse(otherTagName)!;

		// Given
		bool isNewer = version.IsNewerVersion(otherVersion);

		// Then
		Assert.Equal(expected, isNewer);
	}
}
