using UnityEngine;

public class ZombieIgnorizer : MonoBehaviour
{
	public EnemyIdentifier[] eids;

	private void Start()
	{
		EnemyIdentifier[] array = eids;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ignoredByEnemies = true;
		}
	}
}
