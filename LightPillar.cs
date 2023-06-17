using UnityEngine;

public class LightPillar : MonoBehaviour
{
	public bool gotValues;

	public bool activated;

	public bool completed;

	private Light[] lights;

	private AudioSource aud;

	[HideInInspector]
	public Vector3 origScale;

	[HideInInspector]
	public float lightRange;

	[HideInInspector]
	public float origPitch;

	public float speed;

	private void Start()
	{
		if (activated || completed)
		{
			return;
		}
		lights = GetComponentsInChildren<Light>();
		aud = GetComponentInChildren<AudioSource>();
		if (!gotValues)
		{
			gotValues = true;
			origScale = base.transform.localScale;
			origPitch = aud.pitch + Random.Range(-0.1f, 0.1f);
			if (lights.Length != 0)
			{
				lightRange = lights[0].range;
				Light[] array = lights;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].range = 0f;
				}
			}
		}
		aud.pitch = 0f;
		base.transform.localScale = new Vector3(0f, origScale.y, 0f);
	}

	private void Update()
	{
		if (!activated)
		{
			return;
		}
		if (base.transform.localScale != origScale)
		{
			base.transform.localScale = Vector3.MoveTowards(base.transform.localScale, origScale, speed * Time.deltaTime);
		}
		if (lights != null && lights.Length != 0 && lights[0].range != lightRange)
		{
			Light[] array = lights;
			foreach (Light obj in array)
			{
				obj.range = Mathf.MoveTowards(obj.range, lightRange, speed * 4f * Time.deltaTime);
			}
		}
		if (aud.pitch != origPitch)
		{
			aud.pitch = Mathf.MoveTowards(aud.pitch, origPitch, speed / 3f * origPitch * Time.deltaTime);
		}
		else if (base.transform.localScale == origScale && lights[0].range == lightRange)
		{
			activated = false;
			completed = true;
		}
	}

	public void ActivatePillar()
	{
		activated = true;
	}
}
