using UnityEngine;

public class FinalDoor : MonoBehaviour
{
	public Door[] doors;

	public GameObject doorLight;

	public bool startOpen;

	public Material[] offMaterials;

	public Material[] onMaterials;

	public bool levelNameOnOpen;

	private MeshRenderer[] allRenderers;

	private bool opened;

	[HideInInspector]
	public bool aboutToOpen;

	private AudioSource aud;

	public GameObject closingBlocker;

	private void Start()
	{
		if (doorLight == null)
		{
			doorLight = GetComponentInChildren<Light>().gameObject;
		}
		if (!aboutToOpen)
		{
			doorLight.SetActive(value: false);
		}
		if (startOpen || (aboutToOpen && !opened))
		{
			Open();
		}
		allRenderers = GetComponentsInChildren<MeshRenderer>();
	}

	public void Open()
	{
		aboutToOpen = true;
		MonoSingleton<MusicManager>.Instance.ArenaMusicEnd();
		Invoke("OpenDoors", 1f);
		if (!aud)
		{
			aud = GetComponent<AudioSource>();
		}
		aud?.Play();
		doorLight?.SetActive(value: true);
		if (onMaterials.Length == 0)
		{
			return;
		}
		if (allRenderers == null || allRenderers.Length == 0)
		{
			allRenderers = GetComponentsInChildren<MeshRenderer>();
		}
		MeshRenderer[] array = allRenderers;
		foreach (MeshRenderer meshRenderer in array)
		{
			int onMaterial = GetOnMaterial(meshRenderer);
			if (onMaterial >= 0)
			{
				meshRenderer.sharedMaterial = onMaterials[onMaterial];
			}
		}
	}

	public void Close()
	{
		if (!opened && !aboutToOpen)
		{
			return;
		}
		CancelInvoke("OpenDoors");
		aud?.Stop();
		doorLight?.SetActive(value: false);
		if (offMaterials.Length != 0)
		{
			if (allRenderers == null || allRenderers.Length == 0)
			{
				allRenderers = GetComponentsInChildren<MeshRenderer>();
			}
			MeshRenderer[] array = allRenderers;
			foreach (MeshRenderer meshRenderer in array)
			{
				int offMaterial = GetOffMaterial(meshRenderer);
				if (offMaterial >= 0)
				{
					meshRenderer.sharedMaterial = offMaterials[offMaterial];
				}
			}
		}
		if (opened)
		{
			Door[] array2 = doors;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].Close(force: true);
			}
		}
		if ((bool)closingBlocker)
		{
			closingBlocker.SetActive(value: true);
		}
		opened = false;
		aboutToOpen = false;
	}

	private int GetOnMaterial(MeshRenderer mr)
	{
		for (int i = 0; i < offMaterials.Length; i++)
		{
			if (mr.sharedMaterial == offMaterials[i])
			{
				return i;
			}
		}
		return -1;
	}

	private int GetOffMaterial(MeshRenderer mr)
	{
		for (int i = 0; i < onMaterials.Length; i++)
		{
			if (mr.sharedMaterial == onMaterials[i])
			{
				return i;
			}
		}
		return -1;
	}

	public void OpenDoors()
	{
		if (!opened)
		{
			opened = true;
			aboutToOpen = false;
			Door[] array = doors;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Open(enemy: false, skull: true);
			}
			if (levelNameOnOpen)
			{
				Invoke("LevelNameGo", 1f);
			}
			if ((bool)closingBlocker)
			{
				closingBlocker.SetActive(value: false);
			}
			MonoSingleton<PlayerTracker>.Instance.CheckPlayerType();
		}
	}

	private void LevelNameGo()
	{
		MonoSingleton<LevelNamePopup>.Instance.NameAppear();
	}
}
