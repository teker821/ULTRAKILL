using System.Collections.Generic;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class KillHitterCache : MonoSingleton<KillHitterCache>
{
	public int neededScore;

	public int currentScore;

	private List<int> eids = new List<int>();

	public bool ignoreRestarts;

	public void OneDone(int enemyId)
	{
		if (eids.Count == 0 || !eids.Contains(enemyId))
		{
			currentScore++;
			eids.Add(enemyId);
			if (currentScore >= neededScore)
			{
				MonoSingleton<ChallengeManager>.Instance.challengeDone = true;
			}
		}
	}

	public void RemoveId(int enemyId)
	{
		if (eids.Contains(enemyId))
		{
			currentScore--;
			eids.Remove(enemyId);
			if (currentScore < neededScore)
			{
				MonoSingleton<ChallengeManager>.Instance.challengeDone = false;
			}
		}
	}
}
