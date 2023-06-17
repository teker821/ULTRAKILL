using UnityEngine;

[CreateAssetMenu(menuName = "ULTRAKILL/Spawnable Object List")]
public class SpawnableObjectsDatabase : ScriptableObject
{
	public SpawnableObject[] enemies;

	public SpawnableObject[] objects;

	public SpawnableObject[] sandboxTools;

	public SpawnableObject[] sandboxObjects;

	public SpawnableObject[] specialSandbox;

	public SpawnableObject[] unlockables;

	public SpawnableObject[] debug;
}
