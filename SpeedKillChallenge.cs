using UnityEngine;

public class SpeedKillChallenge : MonoBehaviour
{
	public float timeLeft;

	private EnemyIdentifier eid;

	private void Start()
	{
		eid = GetComponent<EnemyIdentifier>();
	}

	private void Update()
	{
		if (timeLeft > 0f)
		{
			if (eid.dead)
			{
				MonoSingleton<ChallengeManager>.Instance.challengeDone = true;
			}
			else
			{
				timeLeft = Mathf.MoveTowards(timeLeft, 0f, Time.deltaTime);
			}
		}
		else
		{
			Object.Destroy(this);
		}
	}
}
