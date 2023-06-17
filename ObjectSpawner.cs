using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
	public GameObject[] spawnables;

	public void SpawnObject(int objectNumber)
	{
		if (spawnables != null && spawnables.Length > objectNumber && spawnables[objectNumber] != null)
		{
			Object.Instantiate(spawnables[objectNumber], spawnables[objectNumber].transform.position, spawnables[objectNumber].transform.rotation).SetActive(value: true);
		}
	}
}
