using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class UltrakillEvent
{
	public GameObject[] toActivateObjects;

	public GameObject[] toDisActivateObjects;

	public UnityEvent onActivate;

	public UnityEvent onDisActivate;

	public void Invoke()
	{
		if (toDisActivateObjects != null)
		{
			GameObject[] array = toDisActivateObjects;
			foreach (GameObject gameObject in array)
			{
				if ((bool)gameObject)
				{
					gameObject.SetActive(value: false);
				}
			}
		}
		if (toActivateObjects != null)
		{
			GameObject[] array = toActivateObjects;
			foreach (GameObject gameObject2 in array)
			{
				if ((bool)gameObject2)
				{
					gameObject2.SetActive(value: true);
				}
			}
		}
		onActivate?.Invoke();
	}

	public void Revert()
	{
		if (toDisActivateObjects != null)
		{
			GameObject[] array = toDisActivateObjects;
			foreach (GameObject gameObject in array)
			{
				if ((bool)gameObject)
				{
					gameObject.SetActive(value: true);
				}
			}
		}
		if (toActivateObjects != null)
		{
			GameObject[] array = toActivateObjects;
			foreach (GameObject gameObject2 in array)
			{
				if ((bool)gameObject2)
				{
					gameObject2.SetActive(value: false);
				}
			}
		}
		onDisActivate?.Invoke();
	}
}
