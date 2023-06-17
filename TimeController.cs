using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class TimeController : MonoSingleton<TimeController>
{
	[SerializeField]
	private GameObject parryLight;

	[SerializeField]
	private GameObject parryFlash;

	private float currentStop;

	private AudioMixer[] audmix;

	[HideInInspector]
	public bool controlTimeScale = true;

	[HideInInspector]
	public bool controlPitch = true;

	public float timeScale;

	public float timeScaleModifier;

	private float slowDown = 1f;

	private void Start()
	{
		audmix = new AudioMixer[4]
		{
			MonoSingleton<AudioMixerController>.Instance.allSound,
			MonoSingleton<AudioMixerController>.Instance.goreSound,
			MonoSingleton<AudioMixerController>.Instance.musicSound,
			MonoSingleton<AudioMixerController>.Instance.doorSound
		};
		if ((bool)MonoSingleton<AssistController>.Instance && MonoSingleton<AssistController>.Instance.majorEnabled)
		{
			timeScale = MonoSingleton<AssistController>.Instance.gameSpeed;
		}
		else
		{
			timeScale = 1f;
		}
		Time.timeScale = timeScale * timeScaleModifier;
	}

	private void Update()
	{
		if (controlTimeScale)
		{
			if (MonoSingleton<AssistController>.Instance.majorEnabled && timeScale != MonoSingleton<AssistController>.Instance.gameSpeed)
			{
				timeScale = MonoSingleton<AssistController>.Instance.gameSpeed;
				Time.timeScale = timeScale * timeScaleModifier;
			}
			else if (!MonoSingleton<AssistController>.Instance.majorEnabled && timeScale != 1f)
			{
				timeScale = 1f;
				Time.timeScale = timeScale * timeScaleModifier;
			}
		}
	}

	private void FixedUpdate()
	{
		if (slowDown < timeScale * timeScaleModifier)
		{
			slowDown = Mathf.MoveTowards(slowDown, timeScale * timeScaleModifier, 0.02f);
			Time.timeScale = slowDown;
			if (controlPitch)
			{
				AudioMixer[] array = audmix;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetFloat("allPitch", slowDown / timeScale / (MonoSingleton<AssistController>.Instance.majorEnabled ? MonoSingleton<AssistController>.Instance.gameSpeed : 1f));
				}
			}
		}
		else if (controlPitch)
		{
			AudioMixer[] array = audmix;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetFloat("allPitch", timeScale / (MonoSingleton<AssistController>.Instance.majorEnabled ? MonoSingleton<AssistController>.Instance.gameSpeed : 1f));
			}
		}
	}

	public void ParryFlash()
	{
		Object.Instantiate(parryLight, MonoSingleton<PlayerTracker>.Instance.GetTarget().position, Quaternion.identity, MonoSingleton<PlayerTracker>.Instance.GetTarget());
		parryFlash?.SetActive(value: true);
		Invoke("HideFlash", 0.1f);
		TrueStop(0.25f);
		MonoSingleton<CameraController>.Instance.CameraShake(0.5f);
		MonoSingleton<RumbleManager>.Instance.SetVibration("rumble.parry_flash");
	}

	private void HideFlash()
	{
		parryFlash?.SetActive(value: false);
		if ((bool)MonoSingleton<CrowdReactions>.Instance && MonoSingleton<CrowdReactions>.Instance.enabled)
		{
			MonoSingleton<CrowdReactions>.Instance.React(MonoSingleton<CrowdReactions>.Instance.cheer);
		}
	}

	public void SlowDown(float amount)
	{
		if (amount <= 0f)
		{
			amount = 0.01f;
		}
		slowDown = amount;
	}

	public void HitStop(float length)
	{
		if (length > currentStop)
		{
			currentStop = length;
			Time.timeScale = 0f;
			StartCoroutine(TimeIsStopped(length, trueStop: false));
		}
	}

	public void TrueStop(float length)
	{
		if (!(length > currentStop))
		{
			return;
		}
		currentStop = length;
		if (controlPitch)
		{
			AudioMixer[] array = audmix;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetFloat("allPitch", 0f);
			}
		}
		Time.timeScale = 0f;
		StartCoroutine(TimeIsStopped(length, trueStop: true));
	}

	private IEnumerator TimeIsStopped(float length, bool trueStop)
	{
		yield return new WaitForSecondsRealtime(length);
		ContinueTime(length, trueStop);
	}

	private void ContinueTime(float length, bool trueStop)
	{
		if (!(length >= currentStop))
		{
			return;
		}
		Time.timeScale = timeScale * timeScaleModifier;
		if (trueStop && controlPitch)
		{
			AudioMixer[] array = audmix;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetFloat("allPitch", 1f);
			}
		}
		currentStop = 0f;
	}
}
