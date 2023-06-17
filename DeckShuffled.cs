using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

internal class DeckShuffled<T> : IEnumerable<T>, IEnumerable
{
	private List<T> current;

	public DeckShuffled(IEnumerable<T> target)
	{
		current = Randomize(target).ToList();
	}

	public void Reshuffle()
	{
		if (current.Count > 1)
		{
			IEnumerable<T> source = current.Take(Mathf.FloorToInt(current.Count / 2));
			IEnumerable<T> source2 = current.Skip(Mathf.FloorToInt(current.Count / 2));
			current = Randomize(source).Concat(Randomize(source2)).ToList();
		}
	}

	private static IEnumerable<T> Randomize(IEnumerable<T> source)
	{
		T[] arr = source.ToArray();
		for (int i = arr.Length - 1; i > 0; i--)
		{
			int swapIndex = Random.Range(0, i + 1);
			yield return arr[swapIndex];
			arr[swapIndex] = arr[i];
		}
		yield return arr[0];
	}

	public IEnumerator<T> GetEnumerator()
	{
		return current.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return current.GetEnumerator();
	}
}
