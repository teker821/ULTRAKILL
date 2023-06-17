using UnityEngine;

public class RandomSoundPlayer : MonoBehaviour
{
	public AudioClip[] sounds;

	private AudioSource aud;

	private float volume;

	public bool playing;

	private float fade;

	private float targetPitch;

	private void Awake()
	{
		aud = GetComponent<AudioSource>();
	}

	private void Update()
	{
		if (playing && fade < volume - 0.1f)
		{
			fade += Time.deltaTime / 10f;
		}
		else if (playing && fade > volume + 0.1f)
		{
			fade -= Time.deltaTime / 10f;
		}
		else if (!playing && fade > 0f)
		{
			fade -= Time.deltaTime / 10f;
		}
		else if (!playing && fade < 0f)
		{
			fade = 0f;
		}
		aud.volume = fade;
		if (playing && aud.pitch > targetPitch + 0.1f)
		{
			aud.pitch -= Time.deltaTime / 10f;
		}
		else if (playing && aud.pitch < targetPitch - 0.1f)
		{
			aud.pitch += Time.deltaTime / 10f;
		}
	}

	public void RollForPlay()
	{
		if (Random.Range(0f, 1f) >= 0.5f && !playing)
		{
			PlayRandomSound();
		}
		else if (playing)
		{
			if (Random.Range(0f, 1f) >= 0.7f)
			{
				PlayRandomSound();
			}
			if (Random.Range(0f, 1f) >= 0.7f)
			{
				targetPitch = Random.Range(0.2f, 3f);
			}
			if (Random.Range(0f, 1f) >= 0.7f)
			{
				volume = Random.Range(0.1f, 0.7f);
			}
		}
	}

	private void PlayRandomSound()
	{
		if (!playing)
		{
			playing = true;
			aud.clip = sounds[Random.Range(0, sounds.Length)];
			volume = Random.Range(0.1f, 0.5f);
			aud.pitch = Random.Range(0.2f, 3f);
			targetPitch = aud.pitch;
			aud.Play();
		}
		else
		{
			playing = false;
		}
	}
}
