using UnityEngine;

public class TimeChallenge : MonoBehaviour
{
	public float time;

	public bool reachedGoal;

	private void Update()
	{
		if (MonoSingleton<StatsManager>.Instance.seconds >= time && !reachedGoal)
		{
			MonoSingleton<ChallengeManager>.Instance.challengeFailed = true;
			base.enabled = false;
		}
		else
		{
			MonoSingleton<ChallengeManager>.Instance.challengeDone = true;
		}
	}

	public void ReachedGoal()
	{
		reachedGoal = true;
	}
}
