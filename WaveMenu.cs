using System.Collections.Generic;
using UnityEngine;

public class WaveMenu : MonoBehaviour
{
	[HideInInspector]
	public List<WaveSetter> setters = new List<WaveSetter>();

	private int highestWave = -1;

	private int currentWave;

	private void Start()
	{
		if (highestWave < 0)
		{
			GetHighestWave();
		}
	}

	public ButtonState CheckWaveAvailability(WaveSetter ws)
	{
		if (highestWave < 0)
		{
			GetHighestWave();
		}
		if (!setters.Contains(ws))
		{
			setters.Add(ws);
		}
		if (ws.wave == currentWave)
		{
			return ButtonState.Selected;
		}
		if (highestWave >= ws.wave * 2)
		{
			return ButtonState.Unselected;
		}
		return ButtonState.Locked;
	}

	private void GetHighestWave()
	{
		int @int = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		CyberRankData bestCyber = GameProgressSaver.GetBestCyber();
		if (bestCyber != null && bestCyber.preciseWavesByDifficulty.Length > @int)
		{
			highestWave = Mathf.FloorToInt(bestCyber.preciseWavesByDifficulty[@int]);
			int int2 = MonoSingleton<PrefsManager>.Instance.GetInt("cyberGrind.startingWave");
			if (highestWave >= int2 * 2)
			{
				currentWave = int2;
			}
			else
			{
				currentWave = 0;
			}
			MonoSingleton<EndlessGrid>.Instance.startWave = currentWave;
		}
	}

	public void SetCurrentWave(int wave)
	{
		if (wave * 2 > highestWave)
		{
			return;
		}
		currentWave = wave;
		MonoSingleton<EndlessGrid>.Instance.startWave = currentWave;
		MonoSingleton<PrefsManager>.Instance.SetInt("cyberGrind.startingWave", wave);
		foreach (WaveSetter setter in setters)
		{
			if (setter.wave != currentWave)
			{
				setter.Unselected();
			}
		}
	}
}
