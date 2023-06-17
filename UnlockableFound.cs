using UnityEngine;

public class UnlockableFound : MonoBehaviour
{
	[SerializeField]
	private UnlockableType unlockableType;

	[SerializeField]
	private bool unlockOnEnable = true;

	[SerializeField]
	private bool unlockOnTriggerEnter;

	private void OnEnable()
	{
		if (unlockOnEnable)
		{
			Unlock();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player") && unlockOnTriggerEnter)
		{
			Unlock();
		}
	}

	public void Unlock()
	{
		if (!SceneHelper.IsPlayingCustom)
		{
			MonoSingleton<UnlockablesData>.Instance.SetUnlocked(unlockableType, unlocked: true);
		}
	}
}
