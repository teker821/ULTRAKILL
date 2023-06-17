using UnityEngine;
using UnityEngine.UI;

public class LevelStats : MonoBehaviour
{
	public bool secretLevel;

	public Text levelName;

	private StatsManager sman;

	private bool ready;

	public Text time;

	public Text timeRank;

	private float seconds;

	private float minutes;

	public Text kills;

	public Text killsRank;

	public Text style;

	public Text styleRank;

	public Text secrets;

	private int currentSecrets;

	public Text challenge;

	public Text majorAssists;

	private void Start()
	{
		sman = MonoSingleton<StatsManager>.Instance;
		if (secretLevel)
		{
			levelName.text = "SECRET MISSION";
			ready = true;
			CheckStats();
			return;
		}
		if (SceneHelper.IsPlayingCustom)
		{
			MapInfo instance = MapInfo.Instance;
			levelName.text = ((instance != null) ? instance.levelName : "???");
			ready = true;
			CheckStats();
		}
		RankData rankData = null;
		if (sman.levelNumber != 0 && !Debug.isDebugBuild)
		{
			rankData = GameProgressSaver.GetRank(returnNull: true);
		}
		if (sman.levelNumber != 0 && (Debug.isDebugBuild || (rankData != null && rankData.levelNumber == sman.levelNumber)))
		{
			StockMapInfo instance2 = StockMapInfo.Instance;
			if (instance2 != null)
			{
				levelName.text = instance2.assets.LargeText;
			}
			else
			{
				levelName.text = "???";
			}
			ready = true;
			CheckStats();
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void Update()
	{
		if (ready)
		{
			CheckStats();
		}
	}

	private void CheckStats()
	{
		if ((bool)time)
		{
			seconds = sman.seconds;
			minutes = 0f;
			while (seconds >= 60f)
			{
				seconds -= 60f;
				minutes += 1f;
			}
			time.text = minutes + ":" + seconds.ToString("00.000");
		}
		if ((bool)timeRank)
		{
			timeRank.text = sman.GetRanks(sman.timeRanks, sman.seconds, reverse: true);
		}
		if ((bool)kills)
		{
			kills.text = sman.kills.ToString();
		}
		if ((bool)killsRank)
		{
			killsRank.text = sman.GetRanks(sman.killRanks, sman.kills, reverse: false);
		}
		if ((bool)style)
		{
			style.text = sman.stylePoints.ToString();
		}
		if ((bool)styleRank)
		{
			styleRank.text = sman.GetRanks(sman.styleRanks, sman.stylePoints, reverse: false);
		}
		if ((bool)secrets)
		{
			int num = sman.secrets + sman.prevSecrets.Count;
			secrets.text = num + "/" + sman.secretObjects.Length;
		}
		if ((bool)challenge)
		{
			if (MonoSingleton<ChallengeManager>.Instance.challengeDone && !MonoSingleton<ChallengeManager>.Instance.challengeFailed)
			{
				challenge.text = "<color=#FFAF00>YES</color>";
			}
			else
			{
				challenge.text = "NO";
			}
		}
		if ((bool)majorAssists)
		{
			if (sman.majorUsed)
			{
				majorAssists.text = "<color=#4C99E6>YES</color>";
			}
			else
			{
				majorAssists.text = "NO";
			}
		}
	}
}
