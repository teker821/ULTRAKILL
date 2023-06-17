using System.Collections.Generic;
using UnityEngine;

public class GrenadeList : MonoSingleton<GrenadeList>
{
	public List<Grenade> grenadeList = new List<Grenade>();

	public List<Cannonball> cannonballList = new List<Cannonball>();

	public void AddGrenade(Grenade gren)
	{
		if (!grenadeList.Contains(gren))
		{
			grenadeList.Add(gren);
		}
	}

	public void AddCannonball(Cannonball cb)
	{
		if (!cannonballList.Contains(cb))
		{
			cannonballList.Add(cb);
		}
	}

	public void RemoveGrenade(Grenade gren)
	{
		if (grenadeList.Contains(gren))
		{
			grenadeList.Remove(gren);
		}
	}

	public void RemoveCannonball(Cannonball cb)
	{
		if (cannonballList.Contains(cb))
		{
			cannonballList.Remove(cb);
		}
	}

	public Grenade GetGrenade(Transform tf)
	{
		for (int num = grenadeList.Count - 1; num >= 0; num--)
		{
			if (grenadeList[num] != null && grenadeList[num].transform == tf)
			{
				return grenadeList[num];
			}
		}
		return null;
	}

	public Cannonball GetCannonball(Transform tf)
	{
		for (int num = cannonballList.Count - 1; num >= 0; num--)
		{
			if (cannonballList[num] != null && cannonballList[num].transform == tf)
			{
				return cannonballList[num];
			}
		}
		return null;
	}

	public bool HasTransform(Transform tf)
	{
		for (int num = grenadeList.Count - 1; num >= 0; num--)
		{
			if (grenadeList[num] != null && grenadeList[num].transform == tf)
			{
				return true;
			}
		}
		for (int num2 = cannonballList.Count - 1; num2 >= 0; num2--)
		{
			if (cannonballList[num2] != null && cannonballList[num2].transform == tf)
			{
				return true;
			}
		}
		return false;
	}

	private void Start()
	{
		Invoke("SlowUpdate", 30f);
	}

	private void SlowUpdate()
	{
		Invoke("SlowUpdate", 30f);
		for (int num = grenadeList.Count - 1; num >= 0; num--)
		{
			if (grenadeList[num] == null)
			{
				grenadeList.RemoveAt(num);
			}
		}
		for (int num2 = cannonballList.Count - 1; num2 >= 0; num2--)
		{
			if (cannonballList[num2] == null)
			{
				cannonballList.RemoveAt(num2);
			}
		}
	}
}
