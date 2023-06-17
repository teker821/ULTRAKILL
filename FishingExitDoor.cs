using UnityEngine;

public class FishingExitDoor : MonoBehaviour
{
	[SerializeField]
	private FishManager manager;

	[SerializeField]
	private UltrakillEvent onUnlock;

	private bool isLocked = true;

	private void Update()
	{
		if (isLocked && manager.RemainingFishes <= 0)
		{
			isLocked = false;
			onUnlock.Invoke();
		}
	}
}
