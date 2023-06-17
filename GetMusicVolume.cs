using System.Collections.Generic;
using UnityEngine;

public class GetMusicVolume : MonoBehaviour
{
	private MusicManager mman;

	private AudioSource[] auds;

	private List<float> origVol = new List<float>();

	private void Start()
	{
		mman = MonoSingleton<MusicManager>.Instance;
		auds = GetComponentsInChildren<AudioSource>();
		AudioSource[] array = auds;
		foreach (AudioSource audioSource in array)
		{
			origVol.Add(audioSource.volume);
		}
		for (int j = 0; j < auds.Length; j++)
		{
			auds[j].volume = origVol[j] * MonoSingleton<AudioMixerController>.Instance.musicVolume;
		}
	}

	private void Update()
	{
		for (int i = 0; i < auds.Length; i++)
		{
			auds[i].volume = origVol[i] * MonoSingleton<AudioMixerController>.Instance.musicVolume;
		}
	}
}
