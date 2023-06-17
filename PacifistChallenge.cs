using UnityEngine;

public class PacifistChallenge : MonoBehaviour
{
	private void Update()
	{
		if ((bool)MonoSingleton<StyleCalculator>.Instance)
		{
			if (!MonoSingleton<StyleCalculator>.Instance.enemiesShot)
			{
				MonoSingleton<ChallengeManager>.Instance.challengeDone = true;
			}
			else
			{
				MonoSingleton<ChallengeManager>.Instance.challengeDone = false;
			}
		}
	}
}
