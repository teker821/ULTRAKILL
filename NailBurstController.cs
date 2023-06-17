using System.Collections.Generic;
using UnityEngine;

public class NailBurstController : MonoBehaviour
{
	public List<EnemyIdentifier> damagedEnemies = new List<EnemyIdentifier>();

	public List<Nail> nails;

	private void Update()
	{
		for (int num = nails.Count - 1; num >= 0; num--)
		{
			if (nails[num] == null || nails[num].hit)
			{
				nails.RemoveAt(num);
			}
		}
		if (nails.Count == 0)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
