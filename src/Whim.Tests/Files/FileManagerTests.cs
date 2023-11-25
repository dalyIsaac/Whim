using System;
using System.IO;
using Xunit;

namespace Whim.Tests;

public class FileManagerTests
{
	[Fact]
	public void WhimDir()
	{
		// Given
		IFileManager fileManager = new FileManager(Array.Empty<string>());

		// When
		string whimDir = fileManager.WhimDir;

		// Then
		Assert.Equal(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".whim"), whimDir);
	}

	[Fact]
	public void WhimDir_WithDirArg()
	{
		// Given
		string dirArg = "--dir";
		string whimDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "test");
		string[] args = { dirArg, whimDir };
		IFileManager fileManager = new FileManager(args);

		// When
		string whimDirFromArgs = fileManager.WhimDir;

		// Then
		Assert.Equal(whimDir, whimDirFromArgs);
	}

	[Fact]
	public void WhimDir_WithDirArg_WithExtraArg()
	{
		// Given
		string dirArg = "--dir";
		string whimDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "test");
		string[] args = { dirArg, whimDir, "--extra" };
		IFileManager fileManager = new FileManager(args);

		// When
		string whimDirFromArgs = fileManager.WhimDir;

		// Then
		Assert.Equal(whimDir, whimDirFromArgs);
	}

	[Fact]
	public void WhimDir_WithDirArg_Equals()
	{
		// Given
		string dirArg = "--dir";
		string whimDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "test");
		string[] args = { dirArg + "=" + whimDir };
		IFileManager fileManager = new FileManager(args);

		// When
		string whimDirFromArgs = fileManager.WhimDir;

		// Then
		Assert.Equal(whimDir, whimDirFromArgs);
	}

	[Fact]
	public void SavedStateDir()
	{
		// Given
		IFileManager fileManager = new FileManager(Array.Empty<string>());

		// When
		string savedStateDir = fileManager.SavedStateDir;

		// Then
		Assert.Equal(
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".whim", "state"),
			savedStateDir
		);
	}

	[Fact]
	public void GetWhimFileDir()
	{
		// Given
		IFileManager fileManager = new FileManager(Array.Empty<string>());

		// When
		string whimFileDir = fileManager.GetWhimFileDir("test");

		// Then
		Assert.Equal(
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".whim", "test"),
			whimFileDir
		);
	}

	[Fact]
	public void GetWhimFileDir_WithSubDir()
	{
		// Given
		IFileManager fileManager = new FileManager(Array.Empty<string>());

		// When
		string whimFileDir = fileManager.GetWhimFileDir(Path.Combine("test", "subdir"));

		// Then
		Assert.Equal(
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".whim", "test", "subdir"),
			whimFileDir
		);
	}
}
