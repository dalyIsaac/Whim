using System;
using System.Text.RegularExpressions;

namespace Whim.Updater;

public partial record Version
{
	public int Major { get; }
	public int Minor { get; }
	public int Patch { get; }
	public ReleaseChannel ReleaseChannel { get; }
	public string Commit { get; }

	public Version(string tagName)
	{
		Match match = ReleaseTagRegex().Match(tagName);
		if (!match.Success || match.Groups.Count != 6)
		{
			throw new ArgumentException($"The tag name '{tagName}' is not a valid release tag name.");
		}

		Major = int.Parse(match.Groups[1].Value);
		Minor = int.Parse(match.Groups[2].Value);
		Patch = int.Parse(match.Groups[3].Value);
		ReleaseChannel = match.Groups[4].Value switch
		{
			"alpha" => ReleaseChannel.Alpha,
			"beta" => ReleaseChannel.Beta,
			"stable" => ReleaseChannel.Stable,
			_ => ReleaseChannel.Stable
		};
		Commit = match.Groups[5].Value;
	}

	[GeneratedRegex(@"^v(\d+).(\d+).(\d+)-(alpha|beta|stable)\+([a-z0-9]{8})$")]
	private static partial Regex ReleaseTagRegex();

	/// <summary>
	/// Returns true if this is a newer release than the <paramref name="other"/> release.
	/// </summary>
	/// <param name="other"></param>
	/// <returns></returns>
	public bool IsNewerVersion(Version other)
	{
		if (Major > other.Major)
		{
			return true;
		}
		else if (Major < other.Major)
		{
			return false;
		}

		if (Minor > other.Minor)
		{
			return true;
		}
		else if (Minor < other.Minor)
		{
			return false;
		}

		if (Patch > other.Patch)
		{
			return true;
		}
		else if (Patch < other.Patch)
		{
			return false;
		}

		// The releases are the same version, so compare the channels
		return ReleaseChannel > other.ReleaseChannel;
	}

	/// <inheritdoc/>
	public override string ToString() => $"{Major}.{Minor}.{Patch}-{ReleaseChannel}+{Commit}";
};
