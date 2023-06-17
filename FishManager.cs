using System;
using System.Collections.Generic;
using System.Linq;
using Logic;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class FishManager : MonoSingleton<FishManager>
{
	[SerializeField]
	private FishDB[] fishDbs;

	public Dictionary<FishObject, bool> recognizedFishes = new Dictionary<FishObject, bool>();

	public Action<FishObject> onFishUnlocked;

	public int RemainingFishes => recognizedFishes.Count((KeyValuePair<FishObject, bool> f) => !f.Value);

	protected override void Awake()
	{
		FishDB[] array = fishDbs;
		for (int i = 0; i < array.Length; i++)
		{
			FishDescriptor[] foundFishes = array[i].foundFishes;
			for (int j = 0; j < foundFishes.Length; j++)
			{
				FishObject fish = foundFishes[j].fish;
				if (!recognizedFishes.ContainsKey(fish))
				{
					recognizedFishes.Add(fish, value: false);
				}
			}
		}
	}

	public bool UnlockFish(FishObject fish)
	{
		if (!recognizedFishes.ContainsKey(fish))
		{
			return false;
		}
		if (recognizedFishes[fish])
		{
			return false;
		}
		recognizedFishes[fish] = true;
		MonoSingleton<MapVarManager>.Instance.AddInt("UniqueFishCaught", 1);
		onFishUnlocked?.Invoke(fish);
		return true;
	}
}
