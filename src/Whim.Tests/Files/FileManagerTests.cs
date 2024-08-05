using System.IO;

namespace Whim.Tests;

public class FileManagerTests
{
	private static readonly string ExpectedWhimDir = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
		".whim"
	);
	private static readonly string ExpectedAltWhimDir = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
		"test"
	);
	private const string DIR_ARG = "--dir";

	[Fact]
	public void WhimDir()
	{
		// Given
		FileManager fileManager = new([]);

		// When
		string whimDir = fileManager.WhimDir;

		// Then
		Assert.Equal(ExpectedWhimDir, whimDir);
	}

	[Fact]
	public void WhimDir_WithDirArg()
	{
		// Given
		string[] args = [DIR_ARG, ExpectedAltWhimDir];
		FileManager fileManager = new(args);

		// When
		string whimDirFromArgs = fileManager.WhimDir;

		// Then
		Assert.Equal(ExpectedAltWhimDir, whimDirFromArgs);
	}

	[Fact]
	public void WhimDir_WithDirArg_WithExtraArg()
	{
		// Given
		string[] args = [DIR_ARG, ExpectedAltWhimDir, "--extra"];
		FileManager fileManager = new(args);

		// When
		string whimDirFromArgs = fileManager.WhimDir;

		// Then
		Assert.Equal(ExpectedAltWhimDir, whimDirFromArgs);
	}

	[Fact]
	public void WhimDir_WithDirArg_StartsWithExtraArg()
	{
		// Given
		string[] args = ["--extra", DIR_ARG, ExpectedAltWhimDir];
		FileManager fileManager = new(args);

		// When
		string whimDirFromArgs = fileManager.WhimDir;

		// Then
		Assert.Equal(ExpectedAltWhimDir, whimDirFromArgs);
	}

	[Fact]
	public void WhimDir_WithDirArg_NextIsNotArg()
	{
		// Given
		string[] args = [DIR_ARG, "--extra", ExpectedAltWhimDir];
		FileManager fileManager = new(args);

		// When
		string whimDirFromArgs = fileManager.WhimDir;

		// Then
		Assert.Equal(ExpectedWhimDir, whimDirFromArgs);
	}

	[Fact]
	public void WhimDir_WithDirArg_EmptyString()
	{
		// Given
		string[] args = [DIR_ARG, string.Empty];
		FileManager fileManager = new(args);

		// When
		string whimDirFromArgs = fileManager.WhimDir;

		// Then
		Assert.Equal(ExpectedWhimDir, whimDirFromArgs);
	}

	[Fact]
	public void WhimDir_WithDirArg_Equals()
	{
		// Given
		string[] args = [DIR_ARG + "=" + ExpectedAltWhimDir];
		FileManager fileManager = new(args);

		// When
		string whimDirFromArgs = fileManager.WhimDir;

		// Then
		Assert.Equal(ExpectedAltWhimDir, whimDirFromArgs);
	}

	[Fact]
	public void SavedStateDir()
	{
		// Given
		FileManager fileManager = new([]);

		// When
		string savedStateDir = fileManager.SavedStateDir;

		// Then
		Assert.Equal(Path.Combine(ExpectedWhimDir, "state"), savedStateDir);
	}

	[Fact]
	public void GetWhimFileDir()
	{
		// Given
		FileManager fileManager = new([]);

		// When
		string whimFileDir = fileManager.GetWhimFileDir("test");

		// Then
		Assert.Equal(Path.Combine(ExpectedWhimDir, "test"), whimFileDir);
	}

	[Fact]
	public void GetWhimFileDir_WithSubDir()
	{
		// Given
		FileManager fileManager = new([]);

		// When
		string whimFileDir = fileManager.GetWhimFileDir(Path.Combine("test", "subdir"));

		// Then
		Assert.Equal(Path.Combine(ExpectedWhimDir, "test", "subdir"), whimFileDir);
	}
}
