using UnityEngine;

public class SwitchOnBeat : MonoBehaviour
{
	[HideInInspector]
	public BeatInfo currentBeatInfo;

	private float timer;

	private float nextMeasure;

	private bool switching;

	private int target;

	public BeatInfo[] switches;

	private bool initialized;

	private void Awake()
	{
		if (!initialized)
		{
			Initialize();
		}
	}

	private void Initialize()
	{
		initialized = true;
		if (!currentBeatInfo)
		{
			currentBeatInfo = switches[target];
		}
		BeatInfo[] array = switches;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetValues();
		}
		timer = currentBeatInfo.aud.time;
	}

	private void Update()
	{
		timer = currentBeatInfo.aud.time;
		if (switching && timer >= nextMeasure)
		{
			Switch();
		}
	}

	private void Switch()
	{
		if (!initialized)
		{
			Initialize();
		}
		switching = false;
		for (int i = 0; i < switches.Length; i++)
		{
			if (i == target)
			{
				switches[i].gameObject.SetActive(value: true);
				currentBeatInfo = switches[i];
				timer = currentBeatInfo.aud.time;
			}
			else
			{
				switches[i].gameObject.SetActive(value: false);
			}
		}
	}

	public void SetTarget(int newTarget)
	{
		if (!initialized)
		{
			Initialize();
		}
		target = newTarget;
		switching = true;
		nextMeasure = 0f;
		if (currentBeatInfo.timeSignatureChanges == null || currentBeatInfo.timeSignatureChanges.Length == 0 || currentBeatInfo.timeSignatureChanges[0] == null || timer < currentBeatInfo.timeSignatureChanges[0].time)
		{
			while (nextMeasure < timer)
			{
				nextMeasure += 60f / currentBeatInfo.bpm * 4f * currentBeatInfo.timeSignature;
			}
		}
		else
		{
			for (int i = 0; i < currentBeatInfo.timeSignatureChanges.Length; i++)
			{
				if (currentBeatInfo.timeSignatureChanges[i].time > timer)
				{
					for (nextMeasure = currentBeatInfo.timeSignatureChanges[i - 1].time; nextMeasure < timer; nextMeasure += 60f / currentBeatInfo.bpm * 4f * currentBeatInfo.timeSignatureChanges[i - 1].timeSignature)
					{
					}
					break;
				}
				if (i == currentBeatInfo.timeSignatureChanges.Length - 1)
				{
					for (nextMeasure = currentBeatInfo.timeSignatureChanges[i].time; nextMeasure < timer; nextMeasure += 60f / currentBeatInfo.bpm * 4f * currentBeatInfo.timeSignatureChanges[i].timeSignature)
					{
					}
				}
			}
		}
		if (nextMeasure >= currentBeatInfo.aud.clip.length)
		{
			nextMeasure = 0f;
		}
	}
}
