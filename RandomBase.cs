using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomBase<T> : MonoBehaviour where T : RandomEntry, new()
{
	public bool randomizeOnEnable = true;

	public int toBeEnabledCount = 1;

	public T[] entries;

	private bool firstDeserialization = true;

	private int arrayLength;

	private void OnEnable()
	{
		if (randomizeOnEnable)
		{
			Randomize();
		}
	}

	public virtual void Randomize()
	{
		RandomizeWithCount(toBeEnabledCount);
	}

	private List<SimulatedRandomEntry> RebuildVirtualPool(T[] pool)
	{
		int num = 0;
		List<SimulatedRandomEntry> list = new List<SimulatedRandomEntry>();
		foreach (T val in pool)
		{
			SimulatedRandomEntry simulatedRandomEntry = new SimulatedRandomEntry
			{
				firstIndex = num,
				target = val
			};
			num = (simulatedRandomEntry.lastIndex = num + val.weight);
			list.Add(simulatedRandomEntry);
		}
		return list;
	}

	public virtual void RandomizeWithCount(int count)
	{
		List<T> list = new List<T>();
		list.AddRange(entries);
		List<SimulatedRandomEntry> list2 = RebuildVirtualPool(list.ToArray());
		for (int i = 0; i < count; i++)
		{
			if (list2.Count <= 0)
			{
				break;
			}
			int num = Random.Range(0, list2.Last().lastIndex);
			RandomEntry entry = null;
			int index = -1;
			for (int j = 0; j < list2.Count; j++)
			{
				SimulatedRandomEntry simulatedRandomEntry = list2[j];
				if (simulatedRandomEntry.firstIndex <= num && simulatedRandomEntry.lastIndex > num)
				{
					entry = simulatedRandomEntry.target;
					index = j;
					break;
				}
			}
			PerformTheAction(entry);
			list2.RemoveAt(index);
			list.RemoveAt(index);
			list2 = RebuildVirtualPool(list.ToArray());
		}
	}

	public virtual void PerformTheAction(RandomEntry entry)
	{
	}

	private void OnValidate()
	{
		if (firstDeserialization)
		{
			arrayLength = entries.Length;
			firstDeserialization = false;
		}
		else
		{
			if (entries.Length == arrayLength)
			{
				return;
			}
			if (entries.Length > arrayLength)
			{
				for (int i = arrayLength; i < entries.Length; i++)
				{
					entries[i] = new T();
				}
			}
			arrayLength = entries.Length;
		}
	}
}
