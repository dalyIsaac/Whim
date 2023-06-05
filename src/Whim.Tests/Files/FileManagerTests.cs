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
		IFileManager fileManager = new FileManager();

		// When
		string whimDir = fileManager.WhimDir;

		// Then
		Assert.Equal(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".whim"), whimDir);
	}

	[Fact]
	public void SavedStateDir()
	{
		// Given
		IFileManager fileManager = new FileManager();

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
		IFileManager fileManager = new FileManager();

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
		IFileManager fileManager = new FileManager();

		// When
		string whimFileDir = fileManager.GetWhimFileDir(Path.Combine("test", "subdir"));

		// Then
		Assert.Equal(
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".whim", "test", "subdir"),
			whimFileDir
		);
	}
}
