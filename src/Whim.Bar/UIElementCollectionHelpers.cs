using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;

namespace Whim.Bar;

/// <summary>
/// Extension methods for <see cref="UIElementCollection"/>.
/// </summary>
public static class UIElementCollectionHelpers
{
	/// <summary>
	/// Adds a range of elements to the collection.
	/// </summary>
	public static void AddRange(this UIElementCollection collection, IEnumerable<UIElement> elements)
	{
		foreach (UIElement element in elements)
		{
			collection.Add(element);
		}
	}
}
