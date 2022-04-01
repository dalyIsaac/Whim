using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;

namespace Whim.Bar;

public static class UIElementCollectionHelpers
{
	public static void AddRange(this UIElementCollection collection, IEnumerable<UIElement> elements)
	{
		foreach (UIElement element in elements)
		{
			collection.Add(element);
		}
	}
}
