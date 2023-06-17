using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Fish Database", menuName = "ULTRAKILL/FishDB")]
public class FishDB : ScriptableObject
{
	public string fullName;

	public Color symbolColor = Color.white;

	public GameObject fishGhostPrefab;

	public FishDescriptor[] foundFishes;

	public void SetupWater(Water water)
	{
		if ((bool)fishGhostPrefab)
		{
			Bounds bounds = water.GetComponent<Collider>().bounds;
			int num = (int)(bounds.size.x * bounds.size.y / 100f);
			for (int i = 0; i < num; i++)
			{
				GameObject gameObject = Object.Instantiate(fishGhostPrefab, water.transform, worldPositionStays: true);
				gameObject.transform.position = new Vector3(Random.Range((0f - bounds.size.x) / 4f, bounds.size.x / 4f) + bounds.center.x, 0f, Random.Range((0f - bounds.size.z) / 4f, bounds.size.z / 4f) + bounds.center.z);
				gameObject.transform.position = new Vector3(gameObject.transform.position.x, bounds.center.y + Random.Range(-1f, 1f) * (bounds.size.y / 2f - 0.2f), gameObject.transform.position.z);
				gameObject.transform.localRotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
			}
		}
	}

	public FishDescriptor GetRandomFish(FishObject[] attractFish)
	{
		if (attractFish != null && attractFish.Length != 0)
		{
			FishDescriptor fishDescriptor = foundFishes.FirstOrDefault((FishDescriptor f) => attractFish.Any((FishObject a) => a == f.fish));
			if (fishDescriptor != null)
			{
				return fishDescriptor;
			}
		}
		int num = 0;
		FishDescriptor[] array = foundFishes;
		for (int i = 0; i < array.Length; i++)
		{
			int chance = array[i].chance;
			num += chance;
		}
		if (num == 0)
		{
			return null;
		}
		int num2 = Random.Range(0, num);
		int num3 = 0;
		array = foundFishes;
		foreach (FishDescriptor fishDescriptor2 in array)
		{
			int chance2 = fishDescriptor2.chance;
			num3 += chance2;
			if (num2 < num3)
			{
				return fishDescriptor2;
			}
		}
		return foundFishes.Last();
	}
}
