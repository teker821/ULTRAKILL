using UnityEngine;

public class StyleComboRank : MonoBehaviour
{
	public int rankToReach;

	private void Update()
	{
		if (MonoSingleton<StyleHUD>.Instance.maxReachedRank >= rankToReach)
		{
			MonoSingleton<ChallengeManager>.Instance.challengeDone = true;
		}
	}
}
