using UnityEngine;

public class DoorlessChecker : MonoBehaviour
{
	private Door dr;

	private bool failed;

	private void Start()
	{
		MonoSingleton<ChallengeDoneByDefault>.Instance.Prepare();
	}

	private void Update()
	{
		if (!failed)
		{
			if (dr == null)
			{
				dr = GetComponent<Door>();
			}
			if (dr != null && dr.open)
			{
				MonoSingleton<ChallengeManager>.Instance.challengeFailed = true;
				failed = true;
			}
		}
	}
}
