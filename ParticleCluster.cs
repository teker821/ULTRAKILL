using UnityEngine;

public class ParticleCluster : MonoBehaviour
{
	[SerializeField]
	private ParticleSystem[] particles;

	private ParticleSystem.EmissionModule[] emissionModules;

	private void Awake()
	{
		emissionModules = new ParticleSystem.EmissionModule[particles.Length];
		for (int i = 0; i < particles.Length; i++)
		{
			emissionModules[i] = particles[i].emission;
		}
	}

	public void EmissionOn()
	{
		for (int i = 0; i < particles.Length; i++)
		{
			emissionModules[i].enabled = true;
		}
	}

	public void EmissionOff()
	{
		for (int i = 0; i < particles.Length; i++)
		{
			emissionModules[i].enabled = false;
		}
	}
}
