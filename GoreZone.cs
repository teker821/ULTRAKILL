using System.Collections.Generic;
using UnityEngine;

public class GoreZone : MonoBehaviour
{
	[Header("Optional")]
	public Transform goreZone;

	public Transform gibZone;

	[HideInInspector]
	public CheckPoint checkpoint;

	[HideInInspector]
	public float maxGore;

	[HideInInspector]
	public List<GameObject> outsideGore = new List<GameObject>();

	private bool endlessMode;

	private int maxGibs;

	public float goreRenderDistance;

	private bool goreUnrendered;

	private static GoreZone _globalRootAutomaticGz;

	public static GoreZone ResolveGoreZone(Transform transform)
	{
		if (!transform.parent)
		{
			if ((bool)_globalRootAutomaticGz)
			{
				transform.SetParent(_globalRootAutomaticGz.transform);
				return _globalRootAutomaticGz;
			}
			GoreZone goreZone = new GameObject("Automated Gore Zone").AddComponent<GoreZone>();
			transform.SetParent(goreZone.transform);
			_globalRootAutomaticGz = goreZone;
			return goreZone;
		}
		GoreZone componentInParent = transform.GetComponentInParent<GoreZone>();
		if ((bool)componentInParent)
		{
			return componentInParent;
		}
		GoreZone componentInChildren = transform.parent.GetComponentInChildren<GoreZone>();
		if ((bool)componentInChildren)
		{
			transform.SetParent(componentInChildren.transform);
			return componentInChildren;
		}
		GoreZone obj = new GameObject("Automated Gore Zone").AddComponent<GoreZone>();
		Transform transform2 = obj.transform;
		transform2.SetParent(transform.parent);
		transform.SetParent(transform2);
		return obj;
	}

	private void Awake()
	{
		if (goreZone == null)
		{
			GameObject gameObject = new GameObject("Gore Zone");
			goreZone = gameObject.transform;
			goreZone.SetParent(base.transform, worldPositionStays: true);
		}
		if (gibZone == null)
		{
			GameObject gameObject2 = new GameObject("Gib Zone");
			gibZone = gameObject2.transform;
			gibZone.SetParent(base.transform, worldPositionStays: true);
		}
	}

	private void Start()
	{
		maxGore = MonoSingleton<OptionsManager>.Instance.maxGore;
		endlessMode = MonoSingleton<EndlessGrid>.Instance != null;
		if (endlessMode)
		{
			maxGibs = Mathf.RoundToInt(maxGore / 40f);
		}
		else
		{
			maxGibs = Mathf.RoundToInt(maxGore / 20f);
		}
		SlowUpdate();
	}

	private void SlowUpdate()
	{
		Invoke("SlowUpdate", 1f);
		if ((float)goreZone.childCount > maxGore)
		{
			int num = Mathf.RoundToInt((float)goreZone.childCount - maxGore);
			for (int i = 0; i < num && goreZone.childCount > i; i++)
			{
				Object.Destroy(goreZone.GetChild(i).gameObject);
			}
		}
		if (gibZone.childCount > maxGibs)
		{
			int num2 = Mathf.RoundToInt(gibZone.childCount - maxGibs);
			for (int j = 0; j < num2 && gibZone.childCount > j; j++)
			{
				Object.Destroy(gibZone.GetChild(j).gameObject);
			}
		}
		if (!((float)outsideGore.Count > maxGore / 5f))
		{
			return;
		}
		int num3 = Mathf.RoundToInt((float)outsideGore.Count - maxGore / 5f);
		for (int k = 0; k < num3 && outsideGore.Count > k; k++)
		{
			if (outsideGore[k] != null)
			{
				Object.Destroy(outsideGore[k].gameObject);
			}
			outsideGore.RemoveAt(k);
		}
	}

	private void Update()
	{
		if (goreRenderDistance != 0f)
		{
			CheckRenderDistance();
		}
	}

	private void CheckRenderDistance()
	{
		if (Vector3.Distance(MonoSingleton<CameraController>.Instance.transform.position, base.transform.position) > goreRenderDistance)
		{
			if (!goreUnrendered)
			{
				goreUnrendered = true;
				goreZone.gameObject.SetActive(value: false);
				gibZone.gameObject.SetActive(value: false);
			}
		}
		else if (goreUnrendered)
		{
			goreUnrendered = false;
			goreZone.gameObject.SetActive(value: true);
			gibZone.gameObject.SetActive(value: true);
		}
	}

	public void Combine()
	{
		StaticBatchingUtility.Combine(goreZone.gameObject);
	}

	public void AddDeath()
	{
		checkpoint.restartKills++;
	}

	public void AddKillHitterTarget(int id)
	{
		if ((bool)checkpoint && !checkpoint.succesfulHitters.Contains(id))
		{
			checkpoint.succesfulHitters.Add(id);
		}
	}

	public void ResetBlood()
	{
		foreach (Transform item in goreZone)
		{
			Object.Destroy(item.gameObject);
		}
	}

	public void ResetGibs()
	{
		foreach (Transform item in gibZone)
		{
			Object.Destroy(item.gameObject);
		}
	}
}
