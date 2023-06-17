using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
[DefaultExecutionOrder(600)]
public class MusicManager : MonoSingleton<MusicManager>
{
	public bool off;

	public bool dontMatch;

	public AudioSource battleTheme;

	public AudioSource cleanTheme;

	public AudioSource bossTheme;

	public AudioSource targetTheme;

	private AudioSource[] allThemes;

	public float volume = 1f;

	public float requestedThemes;

	private bool arenaMode;

	private float defaultVolume;

	public float fadeSpeed;

	public bool forcedOff;

	private bool filtering;

	private new void OnEnable()
	{
		if (fadeSpeed == 0f)
		{
			fadeSpeed = 1f;
		}
		allThemes = GetComponentsInChildren<AudioSource>();
		defaultVolume = volume;
		if (!off)
		{
			AudioSource[] array = allThemes;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Play();
			}
			cleanTheme.volume = volume;
			targetTheme = cleanTheme;
		}
		else
		{
			targetTheme = GetComponent<AudioSource>();
		}
		if ((bool)MonoSingleton<AudioMixerController>.Instance.musicSound)
		{
			MonoSingleton<AudioMixerController>.Instance.musicSound.FindSnapshot("Unpaused").TransitionTo(0f);
		}
	}

	private void Update()
	{
		AudioSource[] array;
		if (!off && targetTheme.volume != volume)
		{
			array = allThemes;
			foreach (AudioSource audioSource in array)
			{
				if (audioSource == targetTheme)
				{
					if (audioSource.volume > volume)
					{
						audioSource.volume = volume;
					}
					if (Time.timeScale == 0f)
					{
						audioSource.volume = volume;
					}
					else
					{
						audioSource.volume = Mathf.MoveTowards(audioSource.volume, volume, fadeSpeed * Time.deltaTime);
					}
				}
				else if (Time.timeScale == 0f)
				{
					audioSource.volume = 0f;
				}
				else
				{
					audioSource.volume = Mathf.MoveTowards(audioSource.volume, 0f, fadeSpeed * Time.deltaTime);
				}
			}
			if (targetTheme.volume == volume)
			{
				array = allThemes;
				foreach (AudioSource audioSource2 in array)
				{
					if (audioSource2 != targetTheme)
					{
						audioSource2.volume = 0f;
					}
				}
			}
		}
		if (filtering)
		{
			MonoSingleton<AudioMixerController>.Instance.musicSound.GetFloat("highPassVolume", out var value);
			value = Mathf.MoveTowards(value, 0f, 1200f * Time.unscaledDeltaTime);
			MonoSingleton<AudioMixerController>.Instance.musicSound.SetFloat("highPassVolume", value);
			if (value == 0f)
			{
				filtering = false;
			}
		}
		if (volume != 0f && (!off || !(targetTheme.volume > 0f)))
		{
			return;
		}
		array = allThemes;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].volume -= Time.deltaTime / 5f * fadeSpeed;
		}
		if (targetTheme.volume <= 0f)
		{
			array = allThemes;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].volume = 0f;
			}
		}
	}

	public void ForceStartMusic()
	{
		forcedOff = false;
		StartMusic();
	}

	public void StartMusic()
	{
		if (forcedOff)
		{
			return;
		}
		AudioSource[] array = allThemes;
		foreach (AudioSource audioSource in array)
		{
			if (audioSource.clip != null)
			{
				audioSource.Play();
				if (off && audioSource.time != 0f)
				{
					audioSource.time = 0f;
				}
			}
		}
		off = false;
		cleanTheme.volume = volume;
		targetTheme = cleanTheme;
	}

	public void PlayBattleMusic()
	{
		if (!dontMatch && targetTheme != battleTheme)
		{
			battleTheme.time = cleanTheme.time;
		}
		if (targetTheme != bossTheme)
		{
			targetTheme = battleTheme;
		}
		requestedThemes += 1f;
	}

	public void PlayCleanMusic()
	{
		requestedThemes -= 1f;
		if (requestedThemes <= 0f && !arenaMode)
		{
			requestedThemes = 0f;
			if (!dontMatch && targetTheme != cleanTheme)
			{
				cleanTheme.time = battleTheme.time;
			}
			if (battleTheme.volume == volume)
			{
				cleanTheme.time = battleTheme.time;
			}
			targetTheme = cleanTheme;
		}
	}

	public void PlayBossMusic()
	{
		Debug.Log("PlayBossMusic");
		if (targetTheme != bossTheme)
		{
			bossTheme.time = cleanTheme.time;
		}
		targetTheme = bossTheme;
	}

	public void ArenaMusicStart()
	{
		if (forcedOff)
		{
			return;
		}
		if (off)
		{
			AudioSource[] array = allThemes;
			foreach (AudioSource audioSource in array)
			{
				if (audioSource.clip != null)
				{
					audioSource.Play();
					if (off && audioSource.time != 0f)
					{
						audioSource.time = 0f;
					}
				}
			}
			off = false;
			battleTheme.volume = volume;
			targetTheme = battleTheme;
		}
		if (!battleTheme.isPlaying)
		{
			AudioSource[] array = allThemes;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Play();
			}
			battleTheme.volume = volume;
		}
		if (targetTheme != bossTheme)
		{
			targetTheme = battleTheme;
		}
		arenaMode = true;
	}

	public void ArenaMusicEnd()
	{
		requestedThemes = 0f;
		targetTheme = cleanTheme;
		arenaMode = false;
	}

	public void ForceStopMusic()
	{
		forcedOff = true;
		StopMusic();
	}

	public void StopMusic()
	{
		off = true;
		AudioSource[] array = allThemes;
		foreach (AudioSource obj in array)
		{
			obj.volume = 0f;
			obj.Stop();
		}
	}

	public void FilterMusic()
	{
		MonoSingleton<AudioMixerController>.Instance.musicSound.SetFloat("highPassVolume", -80f);
		CancelInvoke("RemoveHighPass");
		MonoSingleton<AudioMixerController>.Instance.musicSound.FindSnapshot("Paused").TransitionTo(0f);
		filtering = true;
	}

	public void UnfilterMusic()
	{
		filtering = false;
		MonoSingleton<AudioMixerController>.Instance.musicSound.FindSnapshot("Unpaused").TransitionTo(0.5f);
		Invoke("RemoveHighPass", 0.5f);
	}

	private void RemoveHighPass()
	{
		MonoSingleton<AudioMixerController>.Instance.musicSound.SetFloat("highPassVolume", -80f);
	}
}
