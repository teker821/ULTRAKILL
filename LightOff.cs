using UnityEngine;

public class LightOff : MonoBehaviour
{
	private Light light;

	private AudioSource[] aud;

	public GameObject otherLamp;

	private Light otherLight;

	public float oLIntensity;

	private void Awake()
	{
		aud = GetComponentsInChildren<AudioSource>();
		light = GetComponentInChildren<Light>();
		otherLight = otherLamp.GetComponent<Light>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Player")
		{
			AudioSource[] array = aud;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Play();
			}
			if (light != null)
			{
				light.enabled = false;
			}
			if (otherLight != null)
			{
				otherLight.intensity = oLIntensity;
			}
		}
	}
}
