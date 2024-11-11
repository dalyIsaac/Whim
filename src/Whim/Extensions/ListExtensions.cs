namespace Whim;

/// <summary>
/// Extension methods for <see cref="IReadOnlyList{T}"/>.
/// </summary>
public static class ListExtensions
{
	/// <summary>
	/// Returns the index of the first element in the list that matches the predicate.
	/// </summary>
	/// <typeparam name="T">The type of elements in the list.</typeparam>
	/// <param name="list">The list to search.</param>
	/// <param name="predicate">The predicate to match.</param>
	/// <returns>The index of the first matching element, or -1 if no element matches.</returns>
	public static int FindIndex<T>(this IReadOnlyList<T> list, Func<T, bool> predicate)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (predicate(list[i]))
			{
				return i;
			}
		}
		return -1;
	}
}
