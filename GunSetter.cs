using System.Collections.Generic;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class GunSetter : MonoSingleton<GunSetter>
{
	[HideInInspector]
	public GunControl gunc;

	[HideInInspector]
	public ForcedLoadout forcedLoadout;

	[Header("Revolver")]
	public GameObject[] revolverPierce;

	public GameObject[] revolverRicochet;

	public GameObject[] revolverTwirl;

	[Header("Shotgun")]
	public GameObject[] shotgunGrenade;

	public GameObject[] shotgunPump;

	public GameObject[] shotgunRed;

	[Header("Nailgun")]
	public GameObject[] nailMagnet;

	public GameObject[] nailOverheat;

	public GameObject[] nailRed;

	[Header("Railcannon")]
	public GameObject[] railCannon;

	public GameObject[] railHarpoon;

	public GameObject[] railMalicious;

	[Header("Rocket Launcher")]
	public GameObject[] rocketBlue;

	public GameObject[] rocketGreen;

	public GameObject[] rocketRed;

	private void Start()
	{
		gunc = GetComponent<GunControl>();
		if (base.enabled)
		{
			ResetWeapons(firstTime: true);
		}
	}

	public void ResetWeapons(bool firstTime = false)
	{
		if (gunc == null)
		{
			gunc = GetComponent<GunControl>();
		}
		int b = 5;
		for (int i = 0; i < Mathf.Min(gunc.slots.Count, b); i++)
		{
			List<GameObject> list = gunc.slots[i];
			foreach (GameObject item in list)
			{
				Object.Destroy(item);
			}
			list.Clear();
		}
		List<int> list2 = CheckWeaponOrder("rev");
		for (int j = 1; j < 5; j++)
		{
			switch (list2.IndexOf(j))
			{
			case 0:
				CheckWeapon("rev0", gunc.slot1, revolverPierce);
				break;
			case 1:
				CheckWeapon("rev1", gunc.slot1, revolverTwirl);
				break;
			case 2:
				CheckWeapon("rev2", gunc.slot1, revolverRicochet);
				break;
			}
		}
		list2 = CheckWeaponOrder("sho");
		for (int k = 1; k < 5; k++)
		{
			switch (list2.IndexOf(k))
			{
			case 0:
				CheckWeapon("sho0", gunc.slot2, shotgunGrenade);
				break;
			case 1:
				CheckWeapon("sho1", gunc.slot2, shotgunPump);
				break;
			case 2:
				CheckWeapon("sho2", gunc.slot2, shotgunRed);
				break;
			}
		}
		list2 = CheckWeaponOrder("nai");
		for (int l = 1; l < 5; l++)
		{
			switch (list2.IndexOf(l))
			{
			case 0:
				CheckWeapon("nai0", gunc.slot3, nailMagnet);
				break;
			case 1:
				CheckWeapon("nai1", gunc.slot3, nailOverheat);
				break;
			case 2:
				CheckWeapon("nai2", gunc.slot3, nailRed);
				break;
			}
		}
		list2 = CheckWeaponOrder("rai");
		for (int m = 1; m < 5; m++)
		{
			switch (list2.IndexOf(m))
			{
			case 0:
				CheckWeapon("rai0", gunc.slot4, railCannon);
				break;
			case 1:
				CheckWeapon("rai1", gunc.slot4, railHarpoon);
				break;
			case 2:
				CheckWeapon("rai2", gunc.slot4, railMalicious);
				break;
			}
		}
		list2 = CheckWeaponOrder("rock");
		for (int n = 1; n < 5; n++)
		{
			switch (list2.IndexOf(n))
			{
			case 0:
				CheckWeapon("rock0", gunc.slot5, rocketBlue);
				break;
			case 1:
				CheckWeapon("rock1", gunc.slot5, rocketGreen);
				break;
			case 2:
				CheckWeapon("rock2", gunc.slot5, rocketRed);
				break;
			}
		}
		gunc.UpdateWeaponList(firstTime);
	}

	private List<int> CheckWeaponOrder(string weaponType)
	{
		string text = "1234";
		if (weaponType == "rev")
		{
			text = "1324";
		}
		string text2 = MonoSingleton<PrefsManager>.Instance.GetString("weapon." + weaponType + ".order", text);
		if (text2.Length != 4)
		{
			Debug.LogError("Faulty WeaponOrder: " + weaponType);
			text2 = text;
			MonoSingleton<PrefsManager>.Instance.SetString("weapon." + weaponType + ".order", text);
		}
		List<int> list = new List<int>();
		for (int i = 0; i < text2.Length; i++)
		{
			list.Add(text2[i] - 48);
		}
		return list;
	}

	private void CheckWeapon(string name, List<GameObject> slot, GameObject[] prefabs)
	{
		if (prefabs == null || prefabs.Length == 0)
		{
			return;
		}
		int @int = MonoSingleton<PrefsManager>.Instance.GetInt("weapon." + name, 1);
		VariantOption variantOption = VariantOption.IfEquipped;
		VariantOption variantOption2 = VariantOption.IfEquipped;
		switch (name)
		{
		case "rev0":
			variantOption = forcedLoadout?.revolver.blueVariant ?? VariantOption.IfEquipped;
			variantOption2 = forcedLoadout?.altRevolver.blueVariant ?? VariantOption.IfEquipped;
			break;
		case "rev1":
			variantOption = forcedLoadout?.revolver.redVariant ?? VariantOption.IfEquipped;
			variantOption2 = forcedLoadout?.altRevolver.redVariant ?? VariantOption.IfEquipped;
			break;
		case "rev2":
			variantOption = forcedLoadout?.revolver.greenVariant ?? VariantOption.IfEquipped;
			variantOption2 = forcedLoadout?.altRevolver.greenVariant ?? VariantOption.IfEquipped;
			break;
		case "sho0":
			variantOption = forcedLoadout?.shotgun.blueVariant ?? VariantOption.IfEquipped;
			break;
		case "sho1":
			variantOption = forcedLoadout?.shotgun.greenVariant ?? VariantOption.IfEquipped;
			break;
		case "sho2":
			variantOption = forcedLoadout?.shotgun.redVariant ?? VariantOption.IfEquipped;
			break;
		case "nai0":
			variantOption = forcedLoadout?.nailgun.blueVariant ?? VariantOption.IfEquipped;
			variantOption2 = forcedLoadout?.altNailgun.blueVariant ?? VariantOption.IfEquipped;
			break;
		case "nai1":
			variantOption = forcedLoadout?.nailgun.greenVariant ?? VariantOption.IfEquipped;
			variantOption2 = forcedLoadout?.altNailgun.greenVariant ?? VariantOption.IfEquipped;
			break;
		case "nai2":
			variantOption = forcedLoadout?.nailgun.redVariant ?? VariantOption.IfEquipped;
			variantOption2 = forcedLoadout?.altNailgun.redVariant ?? VariantOption.IfEquipped;
			break;
		case "rai0":
			variantOption = forcedLoadout?.railcannon.blueVariant ?? VariantOption.IfEquipped;
			break;
		case "rai1":
			variantOption = forcedLoadout?.railcannon.greenVariant ?? VariantOption.IfEquipped;
			break;
		case "rai2":
			variantOption = forcedLoadout?.railcannon.redVariant ?? VariantOption.IfEquipped;
			break;
		case "rock0":
			variantOption = forcedLoadout?.rocketLauncher.blueVariant ?? VariantOption.IfEquipped;
			break;
		case "rock1":
			variantOption = forcedLoadout?.rocketLauncher.greenVariant ?? VariantOption.IfEquipped;
			break;
		}
		switch (variantOption)
		{
		case VariantOption.ForceOn:
			if (prefabs[0] != null)
			{
				slot.Add(Object.Instantiate(prefabs[0], base.transform));
			}
			break;
		case VariantOption.IfEquipped:
			if (@int > 0 && GameProgressSaver.CheckGear(name) > 0 && (@int <= 1 || prefabs.Length < @int || GameProgressSaver.CheckGear(name.Substring(0, name.Length - 1) + "alt") <= 0) && prefabs[0] != null)
			{
				slot.Add(Object.Instantiate(prefabs[0], base.transform));
			}
			break;
		}
		switch (variantOption2)
		{
		case VariantOption.ForceOn:
			if (prefabs.Length >= 2)
			{
				slot.Add(Object.Instantiate(prefabs[1], base.transform));
			}
			break;
		case VariantOption.IfEquipped:
			if (@int > 0 && GameProgressSaver.CheckGear(name) > 0 && @int > 1 && prefabs.Length >= @int && GameProgressSaver.CheckGear(name.Substring(0, name.Length - 1) + "alt") > 0)
			{
				slot.Add(Object.Instantiate(prefabs[@int - 1], base.transform));
			}
			break;
		}
	}

	public void ForceWeapon(string weaponName)
	{
		if (gunc == null)
		{
			gunc = GetComponent<GunControl>();
		}
		int num = 0;
		if (MonoSingleton<PrefsManager>.Instance.GetInt("weapon." + weaponName) == 2)
		{
			num = 1;
		}
		switch (weaponName)
		{
		case "rev0":
			gunc.ForceWeapon(revolverPierce[num]);
			break;
		case "rev2":
			gunc.ForceWeapon(revolverRicochet[num]);
			break;
		case "sho0":
			gunc.ForceWeapon(shotgunGrenade[num]);
			break;
		case "sho1":
			gunc.ForceWeapon(shotgunPump[num]);
			break;
		case "nai0":
			gunc.ForceWeapon(nailMagnet[num]);
			break;
		case "nai1":
			gunc.ForceWeapon(nailOverheat[num]);
			break;
		case "rai0":
			gunc.ForceWeapon(railCannon[num]);
			break;
		case "rai1":
			gunc.ForceWeapon(railHarpoon[num]);
			break;
		case "rai2":
			gunc.ForceWeapon(railMalicious[num]);
			break;
		case "rock0":
			gunc.ForceWeapon(rocketBlue[num]);
			break;
		case "rock1":
			gunc.ForceWeapon(rocketGreen[num]);
			break;
		}
	}
}
