using System;
using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;

namespace Whim;

internal class ResourceManager : IResourceManager
{
	public string DefaultDictPath { get; set; } = "Resources/Defaults.xaml";

	public void Initialize()
	{
		// Set default dictionary
		AddDictionary(DefaultDictPath);
	}

	public void AddDictionary(string filePath)
	{
		Logger.Debug($"Adding ResourceDictionary from {filePath}");

		Uri uri = new("ms-appx:///" + filePath);
		ResourceDictionary dict = new() { Source = uri };

		AddMergedDictionary(dict);
	}

	public void AddUserDictionary(string filePath)
	{
		if (!File.Exists(filePath))
		{
			Logger.Error($"Not a valid file path: {filePath}");
			return;
		}

		Logger.Debug($"Adding User ResourceDictionary from {filePath}");

		// We are reading user dicts as raw strings as `ResourceDictionary.Source(uri)
		// does not seem to work when `uri` uses the `file:///` scheme
		string raw = File.ReadAllText(filePath);
		ResourceDictionary dict = (ResourceDictionary)XamlReader.Load(raw);

		AddMergedDictionary(dict);
	}

	/// <summary>
	/// Merge ResourceDictionary with previously added dictionaries. If there is a key
	/// conflict, dictionaries that are merged later take precedence.
	/// </summary>
	/// <param name="dict"></param>
	private static void AddMergedDictionary(ResourceDictionary dict)
	{
		Application.Current.Resources.MergedDictionaries.Add(dict);
	}
}
