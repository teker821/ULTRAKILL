using UnityEngine;

public class MixerVolumeDip : MonoBehaviour
{
	private bool dipped;

	public float targetAmount = -5f;

	private void OnEnable()
	{
		if (!dipped)
		{
			dipped = true;
			MonoSingleton<AudioMixerController>.Instance.allSound.SetFloat("allVolume", targetAmount);
			MonoSingleton<AudioMixerController>.Instance.goreSound.SetFloat("allVolume", targetAmount);
			MonoSingleton<AudioMixerController>.Instance.unfreezeableSound.SetFloat("allVolume", targetAmount);
		}
	}

	private void OnDisable()
	{
		if (dipped)
		{
			dipped = false;
			if ((bool)MonoSingleton<AudioMixerController>.Instance)
			{
				MonoSingleton<AudioMixerController>.Instance.allSound.SetFloat("allVolume", 0f);
				MonoSingleton<AudioMixerController>.Instance.goreSound.SetFloat("allVolume", 0f);
				MonoSingleton<AudioMixerController>.Instance.unfreezeableSound.SetFloat("allVolume", 0f);
			}
		}
	}
}
