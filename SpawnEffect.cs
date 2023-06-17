using UnityEngine;

public class SpawnEffect : MonoBehaviour
{
	private AudioSource aud;

	private Transform bubble;

	private Light light;

	public float pitch;

	private bool simple;

	private void Start()
	{
		if (MonoSingleton<PrefsManager>.Instance.GetBoolLocal("simpleSpawns"))
		{
			simple = true;
		}
		aud = GetComponent<AudioSource>();
		aud.pitch = Random.Range(pitch - 0.1f, pitch + 0.1f);
		aud.Play();
		bubble = base.transform.GetChild(0);
		if (!simple)
		{
			light = GetComponentInChildren<Light>();
			light.enabled = true;
			GetComponent<ParticleSystem>().Play();
		}
	}

	private void Update()
	{
		if (bubble.localScale.x > 0f)
		{
			bubble.localScale -= Vector3.one * 2f * Time.deltaTime;
		}
		else
		{
			bubble.localScale = Vector3.zero;
		}
		if (!simple && light != null && light.range > 0f)
		{
			light.range -= Time.deltaTime * 50f;
		}
	}
}
