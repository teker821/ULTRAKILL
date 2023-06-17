using System.Collections.Generic;
using UnityEngine;

public class LightningStormMetalChecker : MonoBehaviour
{
	public GameObject lightningBolt;

	public GameObject boltWarning;

	private List<GameObject> boltWarnings = new List<GameObject>();

	public float frequencyMinimum = 10f;

	public float frequencyMaximum = 30f;

	public float damageMultiplier = 1f;

	public float enemyDamageMultiplier = 1f;

	private void Start()
	{
		Invoke("Check", Random.Range(frequencyMinimum, frequencyMaximum));
	}

	private void Check()
	{
		Invoke("Check", Random.Range(frequencyMinimum, frequencyMaximum));
		List<EnemyIdentifier> currentEnemies = MonoSingleton<EnemyTracker>.Instance.GetCurrentEnemies();
		if (currentEnemies.Count > 0)
		{
			for (int i = 0; i < currentEnemies.Count; i++)
			{
				if ((currentEnemies[i].nailsAmount > 0 || currentEnemies[i].stuckMagnets.Count > 0) && OutdoorsChecker.CheckIfPositionOutdoors(currentEnemies[i].transform.position + Vector3.up * 0.25f))
				{
					Transform positionFromEnemy = GetPositionFromEnemy(currentEnemies[i]);
					GameObject gameObject = Object.Instantiate(boltWarning, positionFromEnemy.position, Quaternion.identity);
					Follow follow = gameObject.AddComponent<Follow>();
					follow.target = positionFromEnemy;
					follow.destroyIfNoTarget = true;
					boltWarnings.Add(gameObject);
				}
			}
		}
		Harpoon[] array = Object.FindObjectsOfType<Harpoon>();
		if (array != null && array.Length != 0)
		{
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j].drill)
				{
					GameObject gameObject2 = Object.Instantiate(boltWarning, array[j].transform.position, Quaternion.identity);
					Follow follow2 = gameObject2.AddComponent<Follow>();
					follow2.target = array[j].transform;
					follow2.destroyIfNoTarget = true;
					boltWarnings.Add(gameObject2);
				}
			}
		}
		if (boltWarnings.Count > 0)
		{
			Invoke("SummonLightning", 3f);
		}
	}

	private void SummonLightning()
	{
		if (boltWarnings != null && boltWarnings.Count > 0)
		{
			for (int num = boltWarnings.Count - 1; num >= 0; num--)
			{
				if (boltWarnings[num] != null)
				{
					if (OutdoorsChecker.CheckIfPositionOutdoors(boltWarnings[num].transform.position))
					{
						GameObject gameObject = Object.Instantiate(lightningBolt, boltWarnings[num].transform.position, Quaternion.identity);
						if ((damageMultiplier != 1f || enemyDamageMultiplier != 1f) && gameObject.TryGetComponent<LightningStrikeExplosive>(out var component))
						{
							component.damageMultiplier = damageMultiplier;
							component.enemyDamageMultiplier = enemyDamageMultiplier;
						}
					}
					Object.Destroy(boltWarnings[num]);
				}
			}
		}
		boltWarnings.Clear();
	}

	private Transform GetPositionFromEnemy(EnemyIdentifier eid)
	{
		if (eid.stuckMagnets.Count > 0)
		{
			for (int i = 0; i < eid.stuckMagnets.Count; i++)
			{
				if (eid.stuckMagnets[i] != null)
				{
					return eid.stuckMagnets[i].transform;
				}
			}
		}
		if (eid.nails.Count > 0)
		{
			for (int j = 0; j < eid.nails.Count; j++)
			{
				if (eid.nails[j] != null)
				{
					return eid.nails[j].transform;
				}
			}
		}
		return eid.transform;
	}
}
