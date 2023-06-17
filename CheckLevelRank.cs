using UnityEngine;

public class CheckLevelRank : MonoBehaviour
{
	public UltrakillEvent onSuccess;

	public UltrakillEvent onFail;

	private void Start()
	{
		if (CheckLevelStatus())
		{
			onSuccess?.Invoke();
		}
		else
		{
			onFail?.Invoke();
		}
	}

	public static bool CheckLevelStatus()
	{
		if (SceneHelper.IsPlayingCustom)
		{
			return true;
		}
		if (MonoSingleton<StatsManager>.Instance.levelNumber == 0)
		{
			return true;
		}
		RankData rank = GameProgressSaver.GetRank(returnNull: true);
		if (rank != null && rank.levelNumber == MonoSingleton<StatsManager>.Instance.levelNumber && rank.ranks != null && rank.ranks.Length != 0)
		{
			int[] ranks = rank.ranks;
			for (int i = 0; i < ranks.Length; i++)
			{
				if (ranks[i] != -1)
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}
}
