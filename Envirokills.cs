using UnityEngine;

public class Envirokills : MonoBehaviour
{
	public enviroKillType ekt;

	public int killAmount;

	private void Update()
	{
		if (ekt == enviroKillType.Glass && killAmount <= MonoSingleton<StatsManager>.Instance.maxGlassKills)
		{
			MonoSingleton<ChallengeManager>.Instance.challengeDone = true;
		}
	}
}
