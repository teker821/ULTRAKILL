using UnityEngine;

public class KillAmountChallenge : MonoBehaviour
{
	public int kills;

	private void Update()
	{
		if (MonoSingleton<StatsManager>.Instance.kills == kills)
		{
			MonoSingleton<ChallengeManager>.Instance.challengeDone = true;
		}
		else
		{
			MonoSingleton<ChallengeManager>.Instance.challengeDone = false;
		}
	}
}
