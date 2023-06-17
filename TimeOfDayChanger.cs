using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimeOfDayChanger : MonoBehaviour
{
	private bool allOff;

	private bool allDone = true;

	public bool oneTime;

	private bool activated;

	public Light[] oldLights;

	public Light[] newLights;

	private List<float> orgOldIntensities = new List<float>();

	private List<float> origIntensities = new List<float>();

	public Material oldWalls;

	public Material oldSky;

	public Material newWalls;

	public Material newSky;

	public bool toBattleMusic;

	public bool toBossMusic;

	public bool musicWaitsUntilChange;

	public bool revertValuesOnFinish;

	public Material newSkybox;

	private Color skyboxColor;

	private Material oldSkyboxTemp;

	private Material newSkyboxTemp;

	public SpriteRenderer sunSprite;

	public Color sunSpriteColor;

	[Header("Fog")]
	public Color fogColor;

	public bool overrideFogSettings;

	public float fogStart = 450f;

	public float fogEnd = 600f;

	[Header("Lighting")]
	public Color ambientLightingColor;

	[Header("Events")]
	public UnityEvent onMaterialChange;

	private Color originalFogColor;

	private float originalFogStart;

	private float originalFogEnd;

	private Color originalSkyboxTint;

	private Color originalAmbientColor;

	private float transitionState;

	private static readonly int Tint = Shader.PropertyToID("_Tint");

	private void OnEnable()
	{
		if (oneTime && activated)
		{
			return;
		}
		activated = true;
		if (newLights.Length != 0)
		{
			for (int i = 0; i < newLights.Length; i++)
			{
				if (!newLights[i])
				{
					origIntensities.Add(0f);
					continue;
				}
				origIntensities.Add(newLights[i].intensity);
				newLights[i].intensity = 0f;
				newLights[i].enabled = true;
			}
		}
		if (oldLights.Length != 0)
		{
			Light[] array = oldLights;
			foreach (Light light in array)
			{
				if (!light)
				{
					orgOldIntensities.Add(0f);
				}
				else
				{
					orgOldIntensities.Add(light.intensity);
				}
			}
		}
		if ((bool)newSkybox)
		{
			oldSkyboxTemp = new Material(RenderSettings.skybox);
			RenderSettings.skybox = oldSkyboxTemp;
		}
		originalFogColor = RenderSettings.fogColor;
		originalFogStart = RenderSettings.fogStartDistance;
		originalFogEnd = RenderSettings.fogEndDistance;
		originalAmbientColor = RenderSettings.ambientLight;
		if ((bool)RenderSettings.skybox && RenderSettings.skybox.HasProperty(Tint))
		{
			originalSkyboxTint = RenderSettings.skybox.GetColor("_Tint");
		}
		transitionState = 0f;
		allDone = false;
		allOff = false;
		if (!musicWaitsUntilChange)
		{
			if (toBattleMusic)
			{
				MonoSingleton<MusicManager>.Instance.ArenaMusicStart();
			}
			else if (toBossMusic)
			{
				MonoSingleton<MusicManager>.Instance.PlayBossMusic();
			}
		}
	}

	private void OnDisable()
	{
	}

	private void ChangeMaterials()
	{
		MeshRenderer[] array = Object.FindObjectsOfType<MeshRenderer>();
		foreach (MeshRenderer meshRenderer in array)
		{
			if (meshRenderer.sharedMaterial == oldWalls)
			{
				meshRenderer.material = newWalls;
			}
			else if (meshRenderer.sharedMaterial == oldSky)
			{
				meshRenderer.material = newSky;
			}
		}
		if (musicWaitsUntilChange)
		{
			if (toBattleMusic)
			{
				MonoSingleton<MusicManager>.Instance.ArenaMusicStart();
			}
			else if (toBossMusic)
			{
				MonoSingleton<MusicManager>.Instance.PlayBossMusic();
			}
		}
		if ((bool)newSkybox)
		{
			newSkyboxTemp = new Material(newSkybox);
			RenderSettings.skybox = newSkyboxTemp;
			if (RenderSettings.skybox.HasProperty(Tint))
			{
				skyboxColor = RenderSettings.skybox.GetColor("_Tint");
				RenderSettings.skybox.SetColor("_Tint", Color.black);
			}
		}
		if (revertValuesOnFinish)
		{
			for (int j = 0; j < oldLights.Length; j++)
			{
				if ((bool)oldLights[j])
				{
					oldLights[j].transform.parent.gameObject.SetActive(value: false);
					oldLights[j].intensity = orgOldIntensities[j];
				}
			}
		}
		onMaterialChange?.Invoke();
	}

	private void Update()
	{
		if (allDone)
		{
			return;
		}
		transitionState += Time.deltaTime;
		RenderSettings.fogColor = Color.Lerp(originalFogColor, fogColor, transitionState / 2f);
		if (overrideFogSettings)
		{
			RenderSettings.fogStartDistance = Mathf.Lerp(originalFogStart, fogStart, transitionState / 2f);
			RenderSettings.fogEndDistance = Mathf.Lerp(originalFogEnd, fogEnd, transitionState / 2f);
		}
		RenderSettings.ambientLight = Color.Lerp(originalAmbientColor, ambientLightingColor, transitionState / 2f);
		if ((bool)sunSprite)
		{
			sunSprite.color = Color.Lerp(sunSprite.color, sunSpriteColor, transitionState / 2f);
		}
		if (!allOff)
		{
			bool flag = true;
			for (int i = 0; i < oldLights.Length; i++)
			{
				if (!oldLights[i])
				{
					continue;
				}
				Light light = oldLights[i];
				if (light.intensity != 0f)
				{
					light.intensity = Mathf.MoveTowards(light.intensity, 0f, Time.deltaTime * orgOldIntensities[i]);
					if (light.intensity != 0f)
					{
						flag = false;
					}
				}
			}
			if ((bool)newSkybox && RenderSettings.skybox.HasProperty(Tint))
			{
				RenderSettings.skybox.SetColor(Tint, Color.Lerp(originalSkyboxTint, Color.black, transitionState));
			}
			if (flag)
			{
				allOff = true;
				ChangeMaterials();
			}
		}
		else if (newLights.Length != 0)
		{
			bool flag2 = true;
			for (int j = 0; j < newLights.Length; j++)
			{
				if ((bool)newLights[j] && newLights[j].intensity != origIntensities[j])
				{
					newLights[j].intensity = Mathf.MoveTowards(newLights[j].intensity, origIntensities[j], Time.deltaTime * origIntensities[j]);
					if (newLights[j].intensity != origIntensities[j])
					{
						flag2 = false;
					}
				}
			}
			if ((bool)newSkybox && RenderSettings.skybox.HasProperty(Tint))
			{
				RenderSettings.skybox.SetColor(Tint, Color.Lerp(Color.black, skyboxColor, transitionState - 1f));
			}
			if (flag2)
			{
				if ((bool)newSkybox && RenderSettings.skybox.HasProperty(Tint))
				{
					RenderSettings.skybox.SetColor(Tint, skyboxColor);
				}
				RenderSettings.fogColor = fogColor;
				if (overrideFogSettings)
				{
					RenderSettings.fogStartDistance = fogStart;
					RenderSettings.fogEndDistance = fogEnd;
				}
				RenderSettings.ambientLight = ambientLightingColor;
				allDone = true;
			}
			else
			{
				RenderSettings.fogColor = Color.Lerp(originalFogColor, fogColor, transitionState / 2f);
				if (overrideFogSettings)
				{
					RenderSettings.fogStartDistance = Mathf.Lerp(originalFogStart, fogStart, transitionState / 2f);
					RenderSettings.fogEndDistance = Mathf.Lerp(originalFogEnd, fogEnd, transitionState / 2f);
				}
				RenderSettings.ambientLight = Color.Lerp(originalAmbientColor, ambientLightingColor, transitionState / 2f);
			}
		}
		else
		{
			if ((bool)newSkybox && (bool)RenderSettings.skybox && RenderSettings.skybox.HasProperty(Tint))
			{
				RenderSettings.skybox.SetColor(Tint, skyboxColor);
			}
			RenderSettings.fogColor = fogColor;
			if (overrideFogSettings)
			{
				RenderSettings.fogStartDistance = fogStart;
				RenderSettings.fogEndDistance = fogEnd;
			}
			RenderSettings.ambientLight = ambientLightingColor;
			allDone = true;
		}
	}
}
