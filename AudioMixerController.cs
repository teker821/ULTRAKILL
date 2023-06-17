using UnityEngine;
using UnityEngine.Audio;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
[DefaultExecutionOrder(-10)]
public class AudioMixerController : MonoSingleton<AudioMixerController>
{
	public AudioMixer allSound;

	public AudioMixer goreSound;

	public AudioMixer musicSound;

	public AudioMixer doorSound;

	public AudioMixer unfreezeableSound;

	[HideInInspector]
	public float musicVolume;

	[HideInInspector]
	public float optionsMusicVolume;

	public bool forceOff;

	protected override void Awake()
	{
		optionsMusicVolume = MonoSingleton<PrefsManager>.Instance.GetFloat("musicVolume");
		musicVolume = optionsMusicVolume;
		if (!forceOff)
		{
			if (optionsMusicVolume > 0f)
			{
				musicSound.SetFloat("allVolume", Mathf.Log10(optionsMusicVolume) * 20f);
			}
			else
			{
				musicSound.SetFloat("allVolume", -80f);
			}
		}
	}

	private void Update()
	{
		if (musicVolume > optionsMusicVolume)
		{
			SetMusicVolume(optionsMusicVolume);
		}
	}

	public void SetMusicVolume(float volume)
	{
		if (!forceOff)
		{
			musicSound.SetFloat("allVolume", CalculateMusicVolume(volume));
		}
		musicVolume = volume;
	}

	public float CalculateMusicVolume(float volume)
	{
		if (volume > 0f)
		{
			return Mathf.Log10(volume) * 20f;
		}
		return -80f;
	}
}
