using UnityEngine;

[CreateAssetMenu(menuName = "ULTRAKILL/Endless Enemy Data")]
public class EndlessEnemy : ScriptableObject
{
	public EnemyType enemyType;

	public GameObject prefab;

	public int spawnCost;

	public int spawnWave;

	public int costIncreasePerSpawn;
}
