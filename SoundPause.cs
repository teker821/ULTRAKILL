using UnityEngine;

public class SoundPause : MonoBehaviour
{
	private AudioSource aud;

	private bool wasPlaying;

	private void Start()
	{
		aud = GetComponent<AudioSource>();
	}

	private void Update()
	{
		if ((bool)aud)
		{
			if (aud.isPlaying && Time.timeScale == 0f)
			{
				wasPlaying = true;
				aud.Pause();
			}
			else if (Time.timeScale != 0f && wasPlaying)
			{
				wasPlaying = false;
				aud.UnPause();
			}
		}
	}
}
