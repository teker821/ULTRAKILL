using UnityEngine;

public class LaughingSkull : MonoBehaviour
{
	private AudioSource aud;

	private void Start()
	{
		aud = GetComponent<AudioSource>();
	}

	public void PlayAudio()
	{
		aud.Play();
	}
}
