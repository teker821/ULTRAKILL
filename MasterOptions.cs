using UnityEngine;

public class MasterOptions : MonoBehaviour
{
	public float mouseSensitivity;

	public bool enemySimplifier;

	public float enemySimplifierDistance;

	public float maxGore;

	public float bloodstainChance;

	public float musicVolume;

	public float masterVolume;

	private void Start()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}
}
