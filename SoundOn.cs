using UnityEngine;

public class SoundOn : MonoBehaviour
{
	private AudioSource aud;

	public float volume;

	private void Awake()
	{
		aud = GetComponentInChildren<AudioSource>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Player")
		{
			aud.volume = volume;
		}
	}
}
