using System.Collections.Generic;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class OutdoorLightMaster : MonoSingleton<OutdoorLightMaster>
{
	public bool inverse;

	private List<Light> outdoorLights = new List<Light>();

	public Light[] extraLights;

	public GameObject[] activateWhenOutside;

	[HideInInspector]
	public LayerMask normalMask;

	[HideInInspector]
	public LayerMask playerMask;

	private int requests;

	public bool dontRotateSkybox;

	private float skyboxRotation;

	private bool firstDoorOpened;

	public bool waitForFirstDoorOpen;

	private Material skyboxMaterial;

	private Material tempSkybox;

	public List<AudioLowPassFilter> muffleWhenIndoors = new List<AudioLowPassFilter>();

	private List<float> muffleGoals = new List<float>();

	private bool muffleSounds;

	private float currentMuffle;

	[HideInInspector]
	public List<Collider> outdoorsZones = new List<Collider>();

	private void Start()
	{
		Light[] componentsInChildren = GetComponentsInChildren<Light>(includeInactive: true);
		outdoorLights.AddRange(componentsInChildren);
		if (extraLights != null)
		{
			outdoorLights.AddRange(extraLights);
		}
		if (outdoorLights.Count != 0)
		{
			normalMask = 16777216;
			normalMask = (int)normalMask | 0x2000000;
			LayerMask layerMask = 8192;
			playerMask = (int)normalMask | (int)layerMask;
		}
		foreach (Light outdoorLight in outdoorLights)
		{
			if (inverse && (!waitForFirstDoorOpen || firstDoorOpened))
			{
				outdoorLight.cullingMask = playerMask;
			}
			else
			{
				outdoorLight.cullingMask = normalMask;
			}
		}
		if (activateWhenOutside != null)
		{
			GameObject[] array = activateWhenOutside;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(inverse && (!waitForFirstDoorOpen || firstDoorOpened));
			}
		}
		for (int j = 0; j < muffleWhenIndoors.Count; j++)
		{
			muffleGoals.Add(muffleWhenIndoors[j].cutoffFrequency);
		}
		muffleSounds = inverse && waitForFirstDoorOpen && !firstDoorOpened;
		currentMuffle = (muffleSounds ? 1 : 0);
		UpdateMuffle();
	}

	private void Update()
	{
		if (!dontRotateSkybox && (bool)RenderSettings.skybox)
		{
			if (!tempSkybox)
			{
				UpdateSkyboxMaterial();
			}
			else
			{
				skyboxRotation += Time.deltaTime;
				if (skyboxRotation >= 360f)
				{
					skyboxRotation -= 360f;
				}
				RenderSettings.skybox.SetFloat("_Rotation", skyboxRotation);
			}
		}
		if ((muffleSounds && currentMuffle != 1f) || (!muffleSounds && currentMuffle != 0f))
		{
			currentMuffle = Mathf.MoveTowards(currentMuffle, muffleSounds ? 1 : 0, Time.deltaTime * 3f);
			UpdateMuffle();
		}
	}

	public void AddRequest()
	{
		requests++;
		if (requests != 1)
		{
			return;
		}
		foreach (Light outdoorLight in outdoorLights)
		{
			outdoorLight.cullingMask = (inverse ? normalMask : playerMask);
		}
		GameObject[] array = activateWhenOutside;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(!inverse);
		}
		muffleSounds = inverse;
	}

	public void RemoveRequest()
	{
		requests--;
		if (requests != 0)
		{
			return;
		}
		foreach (Light outdoorLight in outdoorLights)
		{
			outdoorLight.cullingMask = (inverse ? playerMask : normalMask);
		}
		GameObject[] array = activateWhenOutside;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(inverse);
		}
		muffleSounds = !inverse;
	}

	public void FirstDoorOpen()
	{
		if (firstDoorOpened)
		{
			return;
		}
		firstDoorOpened = true;
		if (!inverse || !waitForFirstDoorOpen || requests > 0)
		{
			return;
		}
		foreach (Light outdoorLight in outdoorLights)
		{
			outdoorLight.cullingMask = playerMask;
		}
		if (activateWhenOutside != null)
		{
			GameObject[] array = activateWhenOutside;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: true);
			}
		}
		muffleSounds = false;
	}

	public void UpdateSkyboxMaterial()
	{
		if (!skyboxMaterial)
		{
			skyboxMaterial = RenderSettings.skybox;
		}
		tempSkybox = new Material(skyboxMaterial);
		RenderSettings.skybox = tempSkybox;
	}

	public void ForceMuffle(float target)
	{
		currentMuffle = Mathf.Clamp(target, 0f, 1f);
		UpdateMuffle();
	}

	private void UpdateMuffle()
	{
		for (int i = 0; i < muffleWhenIndoors.Count; i++)
		{
			if (!(muffleWhenIndoors[i] == null))
			{
				muffleWhenIndoors[i].enabled = currentMuffle != 0f;
				muffleWhenIndoors[i].cutoffFrequency = Mathf.Lerp(5000f, muffleGoals[i], currentMuffle);
			}
		}
	}
}
