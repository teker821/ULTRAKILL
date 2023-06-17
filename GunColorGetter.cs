using UnityEngine;

public class GunColorGetter : MonoBehaviour
{
	private Renderer rend;

	public Material[] defaultMaterials;

	public Material[] coloredMaterials;

	public int weaponNumber;

	public bool altVersion;

	private void Awake()
	{
		for (int i = 0; i < defaultMaterials.Length; i++)
		{
			defaultMaterials[i] = new Material(defaultMaterials[i]);
		}
		for (int j = 0; j < coloredMaterials.Length; j++)
		{
			coloredMaterials[j] = new Material(coloredMaterials[j]);
		}
	}

	private void OnEnable()
	{
		UpdateColor();
	}

	public void UpdateColor()
	{
		if (rend == null)
		{
			rend = GetComponent<Renderer>();
		}
		if (GetPreset() != 0 || (MonoSingleton<PrefsManager>.Instance.GetBool("gunColorType." + weaponNumber + (altVersion ? ".a" : "")) && GameProgressSaver.HasWeaponCustomization((GameProgressSaver.WeaponCustomizationType)(weaponNumber - 1))))
		{
			rend.materials = coloredMaterials;
			GunColorPreset colors = GetColors();
			Material[] materials = rend.materials;
			foreach (Material material in materials)
			{
				if (material.HasProperty("_CustomColor1"))
				{
					material.SetColor("_CustomColor1", colors.color1);
					material.SetColor("_CustomColor2", colors.color2);
					material.SetColor("_CustomColor3", colors.color3);
				}
			}
		}
		else
		{
			rend.materials = defaultMaterials;
		}
		WeaponIcon[] array = Object.FindObjectsOfType<WeaponIcon>();
		if (array != null && array.Length != 0)
		{
			WeaponIcon[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].UpdateMaterials();
			}
		}
	}

	private int GetPreset()
	{
		switch (weaponNumber)
		{
		case 1:
			if (altVersion)
			{
				return MonoSingleton<GunColorController>.Instance.revolverAltPreset;
			}
			return MonoSingleton<GunColorController>.Instance.revolverPreset;
		case 2:
			if (altVersion)
			{
				return MonoSingleton<GunColorController>.Instance.shotgunAltPreset;
			}
			return MonoSingleton<GunColorController>.Instance.shotgunPreset;
		case 3:
			if (altVersion)
			{
				return MonoSingleton<GunColorController>.Instance.nailgunAltPreset;
			}
			return MonoSingleton<GunColorController>.Instance.nailgunPreset;
		case 4:
			return MonoSingleton<GunColorController>.Instance.railcannonPreset;
		case 5:
			return MonoSingleton<GunColorController>.Instance.rocketLauncherPreset;
		default:
			return 0;
		}
	}

	private GunColorPreset GetColors()
	{
		switch (weaponNumber)
		{
		case 1:
			if (altVersion)
			{
				return MonoSingleton<GunColorController>.Instance.currentRevolverAltColor;
			}
			return MonoSingleton<GunColorController>.Instance.currentRevolverColor;
		case 2:
			if (altVersion)
			{
				return MonoSingleton<GunColorController>.Instance.currentShotgunAltColor;
			}
			return MonoSingleton<GunColorController>.Instance.currentShotgunColor;
		case 3:
			if (altVersion)
			{
				return MonoSingleton<GunColorController>.Instance.currentNailgunAltColor;
			}
			return MonoSingleton<GunColorController>.Instance.currentNailgunColor;
		case 4:
			return MonoSingleton<GunColorController>.Instance.currentRailcannonColor;
		case 5:
			return MonoSingleton<GunColorController>.Instance.currentRocketLauncherColor;
		default:
			return new GunColorPreset(Color.white, Color.white, Color.white);
		}
	}
}
