using UnityEngine;
using UnityEngine.UI;

public class LevelNameFinder : MonoBehaviour
{
	public string textBeforeName;

	public bool breakLine;

	private int thisLevelNumber;

	public int otherLevelNumber;

	private Text targetText;

	public bool lookForPreviousMission;

	public bool lookForLatestMission;

	private void OnEnable()
	{
		if (targetText == null)
		{
			targetText = GetComponent<Text>();
		}
		if (lookForPreviousMission || lookForLatestMission)
		{
			bool flag = false;
			if (lookForPreviousMission)
			{
				PreviousMissionSaver instance = MonoSingleton<PreviousMissionSaver>.Instance;
				if (instance != null)
				{
					flag = true;
					otherLevelNumber = instance.previousMission;
				}
			}
			if (!flag && lookForLatestMission)
			{
				otherLevelNumber = GameProgressSaver.GetProgress(MonoSingleton<PrefsManager>.Instance.GetInt("difficulty"));
			}
		}
		if (!(targetText != null))
		{
			return;
		}
		string text = "";
		if (breakLine)
		{
			text = "\n";
		}
		if (otherLevelNumber != 0)
		{
			targetText.text = textBeforeName + text + GetMissionName.GetMission(otherLevelNumber);
			return;
		}
		if (thisLevelNumber == 0)
		{
			thisLevelNumber = (MonoSingleton<StatsManager>.Instance ? MonoSingleton<StatsManager>.Instance.levelNumber : (-1));
		}
		targetText.text = textBeforeName + text + GetMissionName.GetMission(thisLevelNumber);
	}
}
