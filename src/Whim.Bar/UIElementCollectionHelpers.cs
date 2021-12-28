using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Whim.Bar;

public static class UIElementCollectionHelpers
{
	public static void AddRange(this UIElementCollection collection, IEnumerable<UIElement> elements)
	{
		foreach (var element in elements)
		{
			collection.Add(element);
		}
	}
}
