using System;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class PhysicsSounds : MonoSingleton<PhysicsSounds>
{
	[Serializable]
	public struct PhysSounds
	{
		public AudioClip plastic;

		public AudioClip wood;

		public AudioClip stone;

		public AudioClip metal;

		public AudioClip fleshy;

		public AudioClip glass;

		public AudioClip grass;
	}

	public enum PhysMaterial
	{
		Plastic,
		Wood,
		Stone,
		Metal,
		Fleshy,
		Glass,
		Grass
	}

	[SerializeField]
	private PhysSounds sounds;

	[SerializeField]
	private AudioSource template;

	public AudioClip ResolveSound(PhysMaterial material)
	{
		return material switch
		{
			PhysMaterial.Plastic => sounds.plastic, 
			PhysMaterial.Wood => sounds.wood, 
			PhysMaterial.Stone => sounds.stone, 
			PhysMaterial.Metal => sounds.metal, 
			PhysMaterial.Fleshy => sounds.fleshy, 
			PhysMaterial.Grass => sounds.grass, 
			PhysMaterial.Glass => sounds.glass, 
			_ => sounds.plastic, 
		};
	}

	public void ImpactAt(Vector3 point, float magnitude, PhysMaterial material)
	{
		if (!(magnitude < 3.5f))
		{
			AudioSource audioSource = UnityEngine.Object.Instantiate(template);
			audioSource.transform.position = point;
			audioSource.clip = ResolveSound(material);
			audioSource.volume = Mathf.Lerp(0.2f, 1f, magnitude / 60f);
			audioSource.pitch = Mathf.Lerp(0.65f, 2.2f, (60f - magnitude) / 60f);
			audioSource.gameObject.SetActive(value: true);
			audioSource.Play();
		}
	}
}
