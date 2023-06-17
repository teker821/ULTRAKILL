using UnityEngine;

public class SlideLengthChallenge : MonoBehaviour
{
	public float slideLength;

	private void Update()
	{
		if (MonoSingleton<NewMovement>.Instance.longestSlide >= slideLength)
		{
			MonoSingleton<ChallengeManager>.Instance.challengeDone = true;
		}
	}
}
