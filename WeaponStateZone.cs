using UnityEngine;

public class WeaponStateZone : MonoBehaviour
{
	public bool allowWeaponsOnEnter;

	public bool allowWeaponsOnExit = true;

	public bool allowArmOnEnter = true;

	public bool allowArmOnExit = true;

	private GunControl gc;

	private void OnTriggerEnter(Collider other)
	{
		if (!other.gameObject.CompareTag("Player"))
		{
			return;
		}
		gc = MonoSingleton<GunControl>.Instance;
		if (gc == null)
		{
			gc = other.GetComponentInChildren<GunControl>();
		}
		if ((bool)gc)
		{
			if (allowWeaponsOnEnter)
			{
				gc.YesWeapon();
			}
			else
			{
				gc.NoWeapon();
			}
			FistControl instance = MonoSingleton<FistControl>.Instance;
			if (allowArmOnEnter)
			{
				instance.YesFist();
			}
			else
			{
				instance.NoFist();
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!other.gameObject.CompareTag("Player"))
		{
			return;
		}
		if (gc == null)
		{
			gc = other.GetComponentInChildren<GunControl>();
		}
		if ((bool)gc)
		{
			if (allowWeaponsOnExit)
			{
				gc.YesWeapon();
			}
			else
			{
				gc.NoWeapon();
			}
			FistControl instance = MonoSingleton<FistControl>.Instance;
			if (allowArmOnExit)
			{
				instance.YesFist();
			}
			else
			{
				instance.NoFist();
			}
		}
	}
}
