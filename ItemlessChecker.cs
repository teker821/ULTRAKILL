using UnityEngine;

public class ItemlessChecker : MonoBehaviour
{
	private ItemIdentifier itid;

	private bool failed;

	private void Start()
	{
		MonoSingleton<ChallengeDoneByDefault>.Instance.Prepare();
	}

	private void Update()
	{
		if (!failed)
		{
			if (itid == null)
			{
				itid = GetComponent<ItemIdentifier>();
			}
			if (itid != null && itid.pickedUp)
			{
				MonoSingleton<ChallengeManager>.Instance.challengeFailed = true;
				failed = true;
			}
		}
	}
}
