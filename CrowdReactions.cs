using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class CrowdReactions : MonoSingleton<CrowdReactions>
{
	private AudioSource aud;

	public AudioClip cheer;

	public AudioClip cheerLong;

	public AudioClip aww;

	private void Start()
	{
		aud = GetComponent<AudioSource>();
	}

	public void React(AudioClip clip)
	{
		if (aud.clip != cheerLong || !aud.isPlaying)
		{
			aud.pitch = Random.Range(0.9f, 1.1f);
			aud.clip = clip;
			aud.Play();
		}
	}
}
