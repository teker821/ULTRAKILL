using System.Collections.Generic;
using UnityEngine;

public class DelayedActivationManager : MonoSingleton<DelayedActivationManager>
{
	private List<GameObject> toActivate = new List<GameObject>();

	private List<float> activateCountdowns = new List<float>();

	private void Update()
	{
		if (activateCountdowns.Count == 0)
		{
			return;
		}
		for (int num = activateCountdowns.Count - 1; num >= 0; num--)
		{
			if (toActivate[num] == null)
			{
				toActivate.RemoveAt(num);
				activateCountdowns.RemoveAt(num);
			}
			else
			{
				activateCountdowns[num] = Mathf.MoveTowards(activateCountdowns[num], 0f, Time.deltaTime);
				if (activateCountdowns[num] == 0f)
				{
					toActivate[num].SetActive(value: true);
					toActivate.RemoveAt(num);
					activateCountdowns.RemoveAt(num);
				}
			}
		}
	}

	public void Add(GameObject target, float time)
	{
		toActivate.Add(target);
		activateCountdowns.Add(time);
	}

	public void Remove(GameObject target)
	{
		if (toActivate.Count == 0)
		{
			return;
		}
		for (int i = 0; i < toActivate.Count; i++)
		{
			if (toActivate[i] == target)
			{
				toActivate.RemoveAt(i);
				activateCountdowns.RemoveAt(i);
				break;
			}
		}
	}
}
