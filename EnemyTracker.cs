using System.Collections.Generic;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.None)]
public class EnemyTracker : MonoSingleton<EnemyTracker>
{
	public List<EnemyIdentifier> enemies = new List<EnemyIdentifier>();

	public List<int> enemyRanks = new List<int>();

	private void Update()
	{
		if (!Debug.isDebugBuild || !Input.GetKeyDown(KeyCode.F9))
		{
			return;
		}
		foreach (EnemyIdentifier currentEnemy in GetCurrentEnemies())
		{
			currentEnemy.gameObject.SetActive(value: false);
			currentEnemy.gameObject.SetActive(value: true);
		}
	}

	public List<EnemyIdentifier> GetCurrentEnemies()
	{
		List<EnemyIdentifier> list = new List<EnemyIdentifier>();
		if (enemies != null && enemies.Count > 0)
		{
			for (int num = enemies.Count - 1; num >= 0; num--)
			{
				if (enemies[num].dead || enemies[num] == null || enemies[num].gameObject == null)
				{
					enemies.RemoveAt(num);
					enemyRanks.RemoveAt(num);
				}
				else if (enemies[num].gameObject.activeInHierarchy)
				{
					list.Add(enemies[num]);
				}
			}
		}
		return list;
	}

	public List<EnemyIdentifier> GetEnemiesOfType(EnemyType type)
	{
		List<EnemyIdentifier> currentEnemies = GetCurrentEnemies();
		if (currentEnemies.Count > 0)
		{
			for (int num = currentEnemies.Count - 1; num >= 0; num--)
			{
				if (currentEnemies[num].enemyType != type)
				{
					currentEnemies.RemoveAt(num);
				}
			}
		}
		return currentEnemies;
	}

	public void AddEnemy(EnemyIdentifier eid)
	{
		if (!enemies.Contains(eid))
		{
			enemies.Add(eid);
			enemyRanks.Add(GetEnemyRank(eid));
		}
	}

	public int GetEnemyRank(EnemyIdentifier eid)
	{
		return eid.enemyType switch
		{
			EnemyType.Cerberus => 3, 
			EnemyType.Drone => 1, 
			EnemyType.HideousMass => 6, 
			EnemyType.Ferryman => 5, 
			EnemyType.Filth => 0, 
			EnemyType.Gabriel => 6, 
			EnemyType.Stray => 0, 
			EnemyType.Schism => 1, 
			EnemyType.Sisyphus => 6, 
			EnemyType.Soldier => 1, 
			EnemyType.MaliciousFace => 3, 
			EnemyType.Mindflayer => 5, 
			EnemyType.Minos => 6, 
			EnemyType.Stalker => 4, 
			EnemyType.Streetcleaner => 2, 
			EnemyType.Swordsmachine => 3, 
			EnemyType.Turret => 3, 
			EnemyType.V2 => 6, 
			EnemyType.Virtue => 3, 
			EnemyType.Wicked => 6, 
			_ => -1, 
		};
	}
}
