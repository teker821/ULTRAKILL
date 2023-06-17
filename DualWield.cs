using UnityEngine;

public class DualWield : MonoBehaviour
{
	private GunControl gc;

	private PowerUpMeter meter;

	public float juiceAmount;

	private bool juiceGiven;

	private GameObject copyTarget;

	private GameObject currentWeapon;

	public float delay;

	private void Start()
	{
		gc = MonoSingleton<GunControl>.Instance;
		meter = MonoSingleton<PowerUpMeter>.Instance;
		if (juiceAmount == 0f)
		{
			juiceAmount = 30f;
		}
		if (meter.juice < juiceAmount)
		{
			meter.latestMaxJuice = juiceAmount;
			meter.juice = juiceAmount;
		}
		meter.powerUpColor = new Color(1f, 0.6f, 0f);
		juiceGiven = true;
		MonoSingleton<FistControl>.Instance.forceNoHold++;
		if ((bool)gc.currentWeapon)
		{
			WeaponPos componentInChildren = gc.currentWeapon.GetComponentInChildren<WeaponPos>();
			if ((bool)componentInChildren)
			{
				componentInChildren.CheckPosition();
			}
			UpdateWeapon();
		}
	}

	private void Update()
	{
		if (juiceGiven && meter.juice <= 0f)
		{
			EndPowerUp();
			return;
		}
		if (!copyTarget || copyTarget != gc.currentWeapon)
		{
			UpdateWeapon();
		}
		if ((bool)currentWeapon)
		{
			if (!gc.currentWeapon.activeInHierarchy && currentWeapon.activeSelf)
			{
				currentWeapon.SetActive(value: false);
			}
			else if (gc.currentWeapon.activeInHierarchy && !currentWeapon.activeSelf)
			{
				currentWeapon.SetActive(value: true);
			}
		}
	}

	private void UpdateWeapon()
	{
		if ((bool)currentWeapon)
		{
			Object.Destroy(currentWeapon);
		}
		if ((bool)gc.currentWeapon && gc.currentWeapon.TryGetComponent<WeaponIdentifier>(out var component))
		{
			copyTarget = gc.currentWeapon;
			currentWeapon = Object.Instantiate(gc.currentWeapon, base.transform);
			if (currentWeapon.TryGetComponent<WeaponIdentifier>(out component))
			{
				component.delay = delay;
				component.duplicate = true;
			}
		}
		else
		{
			copyTarget = null;
		}
	}

	public void EndPowerUp()
	{
		if ((bool)gc.currentWeapon)
		{
			WeaponPos componentInChildren = gc.currentWeapon.GetComponentInChildren<WeaponPos>();
			if ((bool)componentInChildren)
			{
				componentInChildren.CheckPosition();
			}
		}
		if (MonoSingleton<FistControl>.Instance.forceNoHold > 0)
		{
			MonoSingleton<FistControl>.Instance.forceNoHold--;
		}
		Object.Destroy(base.gameObject);
	}
}
