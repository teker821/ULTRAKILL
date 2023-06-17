using UnityEngine;

public class Flicker : MonoBehaviour
{
	private Light light;

	public float delay;

	private AudioSource aud;

	private float intensity;

	private float range;

	public bool onlyOnce;

	public bool quickFlicker;

	public float rangeRandomizer;

	public float intensityRandomizer;

	public float timeRandomizer;

	public bool stopAudio;

	public bool forceOnAfterDisable;

	public GameObject[] flickerDisableObjects;

	private void Start()
	{
		light = GetComponent<Light>();
		aud = GetComponent<AudioSource>();
		intensity = light.intensity;
		range = light.range;
		light.intensity = 0f;
		light.range = 0f;
		GameObject[] array = flickerDisableObjects;
		foreach (GameObject gameObject in array)
		{
			if (gameObject.activeSelf)
			{
				gameObject.SetActive(value: false);
			}
			else
			{
				gameObject.SetActive(value: true);
			}
		}
	}

	private void OnDisable()
	{
		CancelInvoke();
		if (forceOnAfterDisable)
		{
			On();
		}
	}

	private void OnEnable()
	{
		if (timeRandomizer != 0f)
		{
			Invoke("Flickering", delay + Random.Range(0f - timeRandomizer, timeRandomizer));
		}
		else
		{
			Invoke("Flickering", delay);
		}
	}

	private void Flickering()
	{
		if (light.intensity == 0f)
		{
			light.intensity = intensity + Random.Range(0f - intensityRandomizer, intensityRandomizer);
			light.range = range + Random.Range(0f - rangeRandomizer, rangeRandomizer);
			if (aud != null && base.gameObject.activeInHierarchy)
			{
				aud.Play();
			}
			if (quickFlicker)
			{
				Invoke("Off", 0.1f);
			}
		}
		else
		{
			light.intensity = 0f;
			if (aud != null && stopAudio && base.gameObject.activeInHierarchy)
			{
				aud.Stop();
			}
		}
		if (!onlyOnce)
		{
			if (timeRandomizer != 0f)
			{
				Invoke("Flickering", delay + Random.Range(0f - timeRandomizer, timeRandomizer));
			}
			else
			{
				Invoke("Flickering", delay);
			}
		}
		GameObject[] array = flickerDisableObjects;
		foreach (GameObject gameObject in array)
		{
			if (gameObject.activeSelf)
			{
				gameObject.SetActive(value: false);
			}
			else
			{
				gameObject.SetActive(value: true);
			}
		}
	}

	private void On()
	{
		light.intensity = intensity;
		light.range = range;
		if (aud != null && base.gameObject.activeInHierarchy)
		{
			aud.Play();
		}
		GameObject[] array = flickerDisableObjects;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: true);
		}
	}

	private void Off()
	{
		light.intensity = 0f;
		if (aud != null && stopAudio && base.gameObject.activeInHierarchy)
		{
			aud.Stop();
		}
		GameObject[] array = flickerDisableObjects;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: false);
		}
	}
}
