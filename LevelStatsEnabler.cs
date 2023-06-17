using UnityEngine;

public class LevelStatsEnabler : MonoBehaviour
{
	private GameObject levelStats;

	private bool keepOpen;

	private float doubleTap;

	public int secretLevel = -1;

	private void Start()
	{
		if (secretLevel < 0)
		{
			StatsManager instance = MonoSingleton<StatsManager>.Instance;
			RankData rankData = null;
			if (instance.levelNumber != 0 && !Debug.isDebugBuild)
			{
				rankData = GameProgressSaver.GetRank(returnNull: true);
			}
			if ((instance.levelNumber == 0 || ((rankData == null || rankData.levelNumber != instance.levelNumber) && !Debug.isDebugBuild)) && !SceneHelper.IsPlayingCustom)
			{
				PlayerPrefs.SetInt("LevStaOpe", 0);
				base.gameObject.SetActive(value: false);
			}
			else if (PlayerPrefs.GetInt("LevStaTut", 0) == 0)
			{
				Invoke("LevelStatsTutorial", 1.5f);
			}
		}
		else if (GameProgressSaver.GetSecretMission(secretLevel) < 2)
		{
			PlayerPrefs.SetInt("LevStaOpe", 0);
			base.gameObject.SetActive(value: false);
		}
		levelStats = base.transform.GetChild(0).gameObject;
		if (PlayerPrefs.GetInt("LevStaOpe", 0) == 0)
		{
			levelStats.SetActive(value: false);
		}
		else
		{
			keepOpen = true;
		}
	}

	private void Update()
	{
		if (!keepOpen)
		{
			if (MonoSingleton<InputManager>.Instance.InputSource.Stats.WasPerformedThisFrame)
			{
				if (!keepOpen)
				{
					if (doubleTap > 0f)
					{
						PlayerPrefs.SetInt("LevStaOpe", 1);
						keepOpen = true;
					}
					else
					{
						doubleTap = 0.5f;
					}
				}
				levelStats.SetActive(value: true);
			}
			else if (MonoSingleton<InputManager>.Instance.InputSource.Stats.WasCanceledThisFrame)
			{
				levelStats.SetActive(value: false);
			}
		}
		else if (MonoSingleton<InputManager>.Instance.InputSource.Stats.WasPerformedThisFrame)
		{
			keepOpen = false;
			PlayerPrefs.SetInt("LevStaOpe", 0);
			levelStats.SetActive(value: false);
		}
		if (doubleTap > 0f)
		{
			doubleTap = Mathf.MoveTowards(doubleTap, 0f, Time.deltaTime);
		}
	}

	private void LevelStatsTutorial()
	{
		PlayerPrefs.SetInt("LevStaTut", 1);
		MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Hold <color=orange>TAB</color> to see current stats when <color=orange>REPLAYING</color> a level.\n<color=orange>DOUBLE TAP</color> to keep open.");
	}
}
