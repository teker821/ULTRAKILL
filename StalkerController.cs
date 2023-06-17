using System.Collections.Generic;
using UnityEngine;

public class StalkerController : MonoSingleton<StalkerController>
{
	public List<Transform> targets = new List<Transform>();

	public bool CheckIfTargetTaken(Transform target)
	{
		if (targets.Contains(target))
		{
			return true;
		}
		return false;
	}
}
