using UnityEngine;

[CreateAssetMenu(fileName = "Prefab Database", menuName = "ULTRAKILL/Prefab Database")]
public class PrefabDatabase : ScriptableObject
{
	[Header("Enemies")]
	public EndlessEnemy[] meleeEnemies;

	public EndlessEnemy[] projectileEnemies;

	public EndlessEnemy[] uncommonEnemies;

	public EndlessEnemy[] specialEnemies;

	[Header("Other Prefabs")]
	public GameObject jumpPad;

	public GameObject stairs;

	public GameObject hideousMass;
}
