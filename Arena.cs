using System.Collections.Generic;
using UnityEngine;

public class Arena : MonoBehaviour
{
	public Door[] doors;

	private void Awake()
	{
		ActivateNextWave[] waves = GetWaves();
		AutoSetupWaves(waves);
		ActivateNextWave[] array = waves;
		for (int i = 0; i < array.Length; i++)
		{
			GameObject[] enemies = GetEnemies(array[i].transform);
			for (int j = 0; j < enemies.Length; j++)
			{
				enemies[j].SetActive(value: false);
			}
		}
	}

	public ActivateNextWave[] GetWaves()
	{
		List<ActivateNextWave> list = new List<ActivateNextWave>();
		foreach (Transform item in base.transform)
		{
			if ((bool)item.GetComponent<ActivateNextWave>())
			{
				list.Add(item.GetComponent<ActivateNextWave>());
			}
		}
		return list.ToArray();
	}

	public static GameObject[] GetEnemies(Transform target)
	{
		List<GameObject> list = new List<GameObject>();
		foreach (Transform item in target.transform)
		{
			list.Add(item.gameObject);
		}
		return list.ToArray();
	}

	public ActivateArena GetActivateArena()
	{
		return GetComponentInChildren<ActivateArena>();
	}

	private void ConfigureActivateArena(ActivateArena aa)
	{
		aa.doors = doors;
		ActivateNextWave[] waves = GetWaves();
		if (waves.Length != 0)
		{
			aa.enemies = GetEnemies(waves[0].transform);
		}
	}

	private void ConfigureWaves(ActivateNextWave[] waves)
	{
		if (waves == null || waves.Length == 0)
		{
			return;
		}
		for (int i = 0; i < waves.Length; i++)
		{
			waves[i].CountEnemies();
			if (i < waves.Length - 1)
			{
				waves[i].nextEnemies = GetEnemies(waves[i + 1].transform);
			}
			waves[i].doors = new Door[0];
			waves[i].lastWave = false;
		}
		waves[^1].doors = doors;
		waves[^1].lastWave = true;
	}

	public void AutoSetupWaves(ActivateNextWave[] waves)
	{
		ConfigureWaves(waves);
		ConfigureActivateArena(GetActivateArena());
	}

	private void OnDrawGizmosSelected()
	{
		Door[] array = doors;
		foreach (Door door in array)
		{
			Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
			Vector3? vector = null;
			Vector3? vector2 = null;
			Vector3 vector3 = Vector3.zero;
			Vector3 vector4 = Vector3.zero;
			GameObject gameObject = ((door.doorType != 0) ? door.gameObject : door.transform.parent.gameObject);
			Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				Bounds bounds = componentsInChildren[j].bounds;
				float num = bounds.center.x - bounds.size.x;
				float num2 = bounds.center.x + bounds.size.x;
				float num3 = bounds.center.y - bounds.size.y;
				float num4 = bounds.center.y + bounds.size.y;
				float num5 = bounds.center.z - bounds.size.z;
				float num6 = bounds.center.z + bounds.size.z;
				if (!vector.HasValue)
				{
					vector4 = new Vector3(num, num3, num5);
					vector3 = new Vector3(num2, num4, num6);
					vector2 = new Vector3(vector3.x - vector4.x, vector3.y - vector4.y, vector3.z - vector4.z);
					vector = new Vector3(num + vector2.Value.x / 2f, num3 + vector2.Value.y / 2f, num5 + vector2.Value.z / 2f);
					continue;
				}
				if (num < vector4.x)
				{
					vector4.x = num;
				}
				if (vector3.x < num2)
				{
					vector3.x = num2;
				}
				if (num3 < vector4.y)
				{
					vector4.y = num3;
				}
				if (vector3.y < num4)
				{
					vector3.y = num4;
				}
				if (num5 < vector4.z)
				{
					vector4.z = num5;
				}
				if (vector3.z < num6)
				{
					vector3.z = num6;
				}
			}
			vector2 = new Vector3(vector3.x - vector4.x, vector3.y - vector4.y, vector3.z - vector4.z);
			vector = new Vector3(vector4.x + vector2.Value.x / 2f, vector4.y + vector2.Value.y / 2f, vector4.z + vector2.Value.z / 2f);
			Gizmos.color = new Color(1f, 1f, 0f, 1f);
			Gizmos.DrawWireCube(vector.Value, vector2.Value * 0.75f);
			Gizmos.color = new Color(1f, 1f, 0f, 0.15f);
			Gizmos.DrawCube(vector.Value, vector2.Value * 0.75f);
		}
	}
}
