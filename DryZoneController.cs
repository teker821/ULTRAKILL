using System.Collections.Generic;
using UnityEngine;

public class DryZoneController : MonoSingleton<DryZoneController>
{
	public List<Water> waters = new List<Water>();

	public List<Collider> colliders = new List<Collider>();

	public List<int> colliderCalls = new List<int>();

	public List<DryZone> dryZones = new List<DryZone>();

	public void AddCollider(Collider other)
	{
		if (!colliders.Contains(other))
		{
			colliders.Add(other);
			colliderCalls.Add(1);
			if (waters.Count <= 0)
			{
				return;
			}
			{
				foreach (Water item in waters)
				{
					item.EnterDryZone(other);
				}
				return;
			}
		}
		colliderCalls[colliders.IndexOf(other)]++;
	}

	public void RemoveCollider(Collider other)
	{
		if (!colliders.Contains(other))
		{
			return;
		}
		int index = colliders.IndexOf(other);
		if (colliderCalls[index] > 1)
		{
			colliderCalls[index]--;
			return;
		}
		colliders.RemoveAt(index);
		colliderCalls.RemoveAt(index);
		if (waters.Count <= 0)
		{
			return;
		}
		foreach (Water item in waters)
		{
			item.ExitDryZone(other);
		}
	}
}
