using System.Collections.Generic;
using ULTRAKILL.Cheats;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class WeaponCharges : MonoSingleton<WeaponCharges>
{
	private GunControl gc;

	public float rev0charge = 100f;

	public bool rev0alt;

	public float rev1charge = 400f;

	public float rev2charge = 300f;

	public bool rev2alt;

	[HideInInspector]
	public bool nai0set;

	public float naiHeatsinks = 2f;

	public float naiSawHeatsinks = 1f;

	public float naiheatUp;

	public float naiAmmo = 100f;

	public float naiSaws = 10f;

	public bool naiAmmoDontCharge;

	[HideInInspector]
	public List<Magnet> magnets = new List<Magnet>();

	public float naiMagnetCharge = 3f;

	public float raicharge = 5f;

	public GameObject railCannonFullChargeSound;

	public bool railChargePlayed;

	[HideInInspector]
	public bool rocketset;

	public float rocketcharge;

	[HideInInspector]
	public bool rocketFrozen;

	public float rocketFreezeTime = 5f;

	[HideInInspector]
	public RocketLauncher rocketLauncher;

	public int rocketCount;

	[HideInInspector]
	public bool canAutoUnfreeze;

	public TimeSince timeSinceIdleFrozen;

	public float rocketCannonballCharge = 1f;

	[HideInInspector]
	public bool infiniteRocketRide;

	public float[] revaltpickupcharges = new float[3];

	private void Update()
	{
		if (NoWeaponCooldown.NoCooldown)
		{
			MaxCharges();
		}
		else
		{
			Charge(Time.deltaTime);
		}
		if (rocketFrozen)
		{
			if (rocketCount > 0)
			{
				canAutoUnfreeze = true;
			}
			if (canAutoUnfreeze && rocketCount == 0 && (float)timeSinceIdleFrozen > 0.5f)
			{
				rocketLauncher.UnfreezeRockets();
			}
		}
	}

	public void Charge(float amount)
	{
		if (rev0charge < 100f)
		{
			if (rev0alt)
			{
				rev0charge = Mathf.MoveTowards(rev0charge, 100f, 20f * amount);
			}
			else
			{
				rev0charge = Mathf.MoveTowards(rev0charge, 100f, 40f * amount);
			}
		}
		if (rev1charge < 400f)
		{
			rev1charge = Mathf.MoveTowards(rev1charge, 400f, 25f * amount);
		}
		if (rev2charge < 300f)
		{
			if (rev2alt)
			{
				rev2charge = Mathf.MoveTowards(rev2charge, 300f, 35f * amount);
			}
			else
			{
				rev2charge = Mathf.MoveTowards(rev2charge, 300f, 15f * amount);
			}
		}
		if (naiHeatsinks < 2f)
		{
			naiHeatsinks = Mathf.MoveTowards(naiHeatsinks, 2f, amount * 0.075f);
		}
		if (naiSawHeatsinks < 1f)
		{
			naiSawHeatsinks = Mathf.MoveTowards(naiSawHeatsinks, 1f, amount * 0.075f);
		}
		if (naiheatUp > 0f)
		{
			naiheatUp = Mathf.MoveTowards(naiheatUp, 0f, amount * 0.3f);
		}
		if (raicharge < 5f)
		{
			raicharge = Mathf.MoveTowards(raicharge, 5f, amount * 0.25f);
			if (raicharge >= 4f && (bool)railCannonFullChargeSound)
			{
				raicharge = 5f;
				PlayRailCharge();
			}
		}
		if (rocketcharge > 0f)
		{
			rocketcharge = Mathf.MoveTowards(rocketcharge, 0f, amount);
		}
		if (rocketCannonballCharge < 1f)
		{
			rocketCannonballCharge = Mathf.MoveTowards(rocketCannonballCharge, 1f, amount * 0.125f);
		}
		for (int i = 0; i < revaltpickupcharges.Length; i++)
		{
			if (revaltpickupcharges[i] > 0f)
			{
				revaltpickupcharges[i] = Mathf.MoveTowards(revaltpickupcharges[i], 0f, amount);
			}
		}
		if (!naiAmmoDontCharge)
		{
			naiAmmo = Mathf.MoveTowards(naiAmmo, 100f, amount * 3.5f);
			naiSaws = Mathf.MoveTowards(naiSaws, 10f, amount * 0.5f);
		}
		if (magnets.Count > 0)
		{
			for (int num = magnets.Count - 1; num >= 0; num--)
			{
				if (magnets[num] == null)
				{
					magnets.RemoveAt(num);
				}
			}
			if (magnets.Count < 3 && naiMagnetCharge < (float)(3 - magnets.Count))
			{
				naiMagnetCharge = Mathf.MoveTowards(naiMagnetCharge, 3 - magnets.Count, amount * 3f);
			}
		}
		else if (naiMagnetCharge < 3f)
		{
			naiMagnetCharge = Mathf.MoveTowards(naiMagnetCharge, 3f, amount * 3f);
		}
		rocketFreezeTime = Mathf.MoveTowards(rocketFreezeTime, (!rocketFrozen) ? 5 : 0, rocketFrozen ? Time.deltaTime : (amount / 2f));
		if (rocketFrozen && (bool)rocketLauncher && (bool)rocketLauncher.currentTimerTickSound)
		{
			rocketLauncher.currentTimerTickSound.pitch = Mathf.Lerp(0.75f, 1f, rocketFreezeTime / 5f);
		}
		if (rocketFrozen && rocketFreezeTime <= 0f)
		{
			rocketLauncher.UnfreezeRockets();
		}
	}

	public void MaxCharges()
	{
		rev0charge = 100f;
		rev1charge = 400f;
		rev2charge = 300f;
		naiHeatsinks = 2f;
		naiSawHeatsinks = 1f;
		naiheatUp = 0f;
		rocketcharge = 0f;
		for (int i = 0; i < revaltpickupcharges.Length; i++)
		{
			revaltpickupcharges[i] = 0f;
		}
		naiAmmo = 100f;
		naiSaws = 10f;
		magnets.Clear();
		naiMagnetCharge = 3f;
		if (raicharge < 5f)
		{
			raicharge = 5f;
			PlayRailCharge();
		}
		rocketFreezeTime = 5f;
		rocketCannonballCharge = 1f;
		if (!gc)
		{
			gc = GetComponent<GunControl>();
		}
		if ((bool)gc && (bool)gc.currentWeapon)
		{
			gc.currentWeapon.SendMessage("MaxCharge", SendMessageOptions.DontRequireReceiver);
		}
	}

	public void PlayRailCharge()
	{
		railChargePlayed = true;
		Object.Instantiate(railCannonFullChargeSound);
	}
}
