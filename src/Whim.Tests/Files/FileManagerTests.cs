using System;
using System.IO;
using Xunit;

namespace Whim.Tests;

public class FileManagerTests
{
	private static readonly string ExpectedWhimDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".whim");
	private static readonly string ExpectedAltWhimDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "test");

	[Fact]
	public void WhimDir()
	{
		// Given
		IFileManager fileManager = new FileManager(Array.Empty<string>());

		// When
		string whimDir = fileManager.WhimDir;

		// Then
		Assert.Equal(ExpectedWhimDir, whimDir);
	}

	[Fact]
	public void WhimDir_WithDirArg()
	{
		// Given
		string dirArg = "--dir";
		string[] args = { dirArg, ExpectedAltWhimDir };
		IFileManager fileManager = new FileManager(args);

		// When
		string whimDirFromArgs = fileManager.WhimDir;

		// Then
		Assert.Equal(ExpectedAltWhimDir, whimDirFromArgs);
	}

	[Fact]
	public void WhimDir_WithDirArg_WithExtraArg()
	{
		// Given
		string dirArg = "--dir";
		string[] args = { dirArg, ExpectedAltWhimDir, "--extra" };
		IFileManager fileManager = new FileManager(args);

		// When
		string whimDirFromArgs = fileManager.WhimDir;

		// Then
		Assert.Equal(ExpectedAltWhimDir, whimDirFromArgs);
	}

	[Fact]
	public void WhimDir_WithDirArg_Equals()
	{
		// Given
		string dirArg = "--dir";
		string[] args = { dirArg + "=" + ExpectedAltWhimDir };
		IFileManager fileManager = new FileManager(args);

		// When
		string whimDirFromArgs = fileManager.WhimDir;

		// Then
		Assert.Equal(ExpectedAltWhimDir, whimDirFromArgs);
	}

	[Fact]
	public void SavedStateDir()
	{
		// Given
		IFileManager fileManager = new FileManager(Array.Empty<string>());

		// When
		string savedStateDir = fileManager.SavedStateDir;

		// Then
		Assert.Equal(Path.Combine(ExpectedWhimDir, "state"), savedStateDir);
	}

	[Fact]
	public void GetWhimFileDir()
	{
		// Given
		IFileManager fileManager = new FileManager(Array.Empty<string>());

		// When
		string whimFileDir = fileManager.GetWhimFileDir("test");

		// Then
		Assert.Equal(Path.Combine(ExpectedWhimDir, "test"), whimFileDir);
	}

	[Fact]
	public void GetWhimFileDir_WithSubDir()
	{
		// Given
		IFileManager fileManager = new FileManager(Array.Empty<string>());

		// When
		string whimFileDir = fileManager.GetWhimFileDir(Path.Combine("test", "subdir"));

		// Then
		Assert.Equal(Path.Combine(ExpectedWhimDir, "test", "subdir"), whimFileDir);
	}
}
