using ULTRAKILL.Cheats;
using UnityEngine;

public class ChallengeTrigger : MonoBehaviour
{
	public ChallengeType type;

	public bool checkForNoEnemies;

	public bool evenIfPlayerDead;

	private void Start()
	{
		if (type == ChallengeType.Fail)
		{
			MonoSingleton<ChallengeDoneByDefault>.Instance.Prepare();
		}
		if (GetComponent<Collider>() == null && GetComponent<Rigidbody>() == null && (evenIfPlayerDead || !MonoSingleton<NewMovement>.Instance.dead))
		{
			Entered();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Player" && (!checkForNoEnemies || !DisableEnemySpawns.DisableArenaTriggers) && (evenIfPlayerDead || !MonoSingleton<NewMovement>.Instance.dead))
		{
			Entered();
		}
	}

	public void Entered()
	{
		if (type == ChallengeType.Fail)
		{
			MonoSingleton<ChallengeManager>.Instance.ChallengeFailed();
		}
		else if (type == ChallengeType.Succeed)
		{
			MonoSingleton<ChallengeManager>.Instance.ChallengeDone();
		}
		Debug.Log("Player has entered " + base.gameObject.name);
	}
}
