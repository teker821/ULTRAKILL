using UnityEngine;

public class WeaponStalenessLocker : MonoBehaviour
{
	public LockerType type;

	public int slot;

	public float minValue;

	public float maxValue;

	public StyleFreshnessState minState;

	public StyleFreshnessState maxState;

	public bool oneTime;

	private bool beenActivated;

	private bool colliderless;

	private void Start()
	{
		if (GetComponent<Collider>() == null && GetComponent<Rigidbody>() == null)
		{
			colliderless = true;
			Activate();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject == MonoSingleton<NewMovement>.Instance.gameObject)
		{
			Activate();
		}
	}

	public void Activate()
	{
		if (!beenActivated || !oneTime)
		{
			beenActivated = true;
			switch (type)
			{
			case LockerType.Unlocker:
				MonoSingleton<StyleHUD>.Instance.UnlockFreshness(slot);
				break;
			case LockerType.State:
				MonoSingleton<StyleHUD>.Instance.LockFreshness(slot, minState, maxState);
				break;
			case LockerType.Value:
				MonoSingleton<StyleHUD>.Instance.LockFreshness(slot, minValue, maxValue);
				break;
			}
		}
	}
}
