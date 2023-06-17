using UnityEngine;

public class EnviroParticle : MonoBehaviour
{
	private ParticleSystem part;

	private void Start()
	{
		part = GetComponent<ParticleSystem>();
		if (MonoSingleton<PrefsManager>.Instance.GetBoolLocal("disableEnvironmentParticles"))
		{
			part.Stop();
			part.Clear();
		}
	}

	private void OnEnable()
	{
		CheckEnviroParticles();
	}

	public void CheckEnviroParticles()
	{
		if (part == null)
		{
			part = GetComponent<ParticleSystem>();
		}
		if (MonoSingleton<PrefsManager>.Instance.GetBoolLocal("disableEnvironmentParticles"))
		{
			part.Stop();
			part.Clear();
		}
		else
		{
			part.Play();
		}
	}
}
