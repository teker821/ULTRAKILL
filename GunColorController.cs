using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class GunColorController : MonoSingleton<GunColorController>
{
	public static int[] requiredSecrets = new int[5] { 0, 10, 25, 50, 86 };

	public GunColorPreset[] revolverColors;

	public GunColorPreset[] shotgunColors;

	public GunColorPreset[] nailgunColors;

	public GunColorPreset[] railcannonColors;

	public GunColorPreset[] rocketLauncherColors;

	[HideInInspector]
	public GunColorPreset currentRevolverColor;

	[HideInInspector]
	public GunColorPreset currentRevolverAltColor;

	[HideInInspector]
	public GunColorPreset currentShotgunColor;

	[HideInInspector]
	public GunColorPreset currentShotgunAltColor;

	[HideInInspector]
	public GunColorPreset currentNailgunColor;

	[HideInInspector]
	public GunColorPreset currentNailgunAltColor;

	[HideInInspector]
	public GunColorPreset currentRailcannonColor;

	[HideInInspector]
	public GunColorPreset currentRocketLauncherColor;

	[HideInInspector]
	public int revolverPreset;

	[HideInInspector]
	public int revolverAltPreset;

	[HideInInspector]
	public int shotgunPreset;

	[HideInInspector]
	public int shotgunAltPreset;

	[HideInInspector]
	public int nailgunPreset;

	[HideInInspector]
	public int nailgunAltPreset;

	[HideInInspector]
	public int railcannonPreset;

	[HideInInspector]
	public int rocketLauncherPreset;

	private void Start()
	{
		UpdateGunColors();
	}

	public void UpdateGunColors()
	{
		if (MonoSingleton<PrefsManager>.Instance.GetBool("gunColorType.1") && GameProgressSaver.HasWeaponCustomization(GameProgressSaver.WeaponCustomizationType.Revolver))
		{
			currentRevolverColor = CustomGunColorPreset(1, altVersion: false);
		}
		else
		{
			revolverPreset = MonoSingleton<PrefsManager>.Instance.GetInt("gunColorPreset.1");
			if (GameProgressSaver.GetTotalSecretsFound() < requiredSecrets[revolverPreset])
			{
				revolverPreset = 0;
				MonoSingleton<PrefsManager>.Instance.SetInt("gunColorPreset.1", 0);
			}
			currentRevolverColor = revolverColors[revolverPreset];
		}
		if (MonoSingleton<PrefsManager>.Instance.GetBool("gunColorType.1.a") && GameProgressSaver.HasWeaponCustomization(GameProgressSaver.WeaponCustomizationType.Revolver))
		{
			currentRevolverAltColor = CustomGunColorPreset(1, altVersion: true);
		}
		else
		{
			revolverAltPreset = MonoSingleton<PrefsManager>.Instance.GetInt("gunColorPreset.1.a");
			if (GameProgressSaver.GetTotalSecretsFound() < requiredSecrets[revolverAltPreset])
			{
				revolverAltPreset = 0;
				MonoSingleton<PrefsManager>.Instance.SetInt("gunColorPreset.1.a", 0);
			}
			currentRevolverAltColor = revolverColors[revolverAltPreset];
		}
		if (MonoSingleton<PrefsManager>.Instance.GetBool("gunColorType.2") && GameProgressSaver.HasWeaponCustomization(GameProgressSaver.WeaponCustomizationType.Shotgun))
		{
			currentShotgunColor = CustomGunColorPreset(2, altVersion: false);
		}
		else
		{
			shotgunPreset = MonoSingleton<PrefsManager>.Instance.GetInt("gunColorPreset.2");
			if (GameProgressSaver.GetTotalSecretsFound() < requiredSecrets[shotgunPreset])
			{
				shotgunPreset = 0;
				MonoSingleton<PrefsManager>.Instance.SetInt("gunColorPreset.2", 0);
			}
			currentShotgunColor = shotgunColors[shotgunPreset];
		}
		if (MonoSingleton<PrefsManager>.Instance.GetBool("gunColorType.2.a") && GameProgressSaver.HasWeaponCustomization(GameProgressSaver.WeaponCustomizationType.Shotgun))
		{
			currentShotgunAltColor = CustomGunColorPreset(2, altVersion: true);
		}
		else
		{
			shotgunAltPreset = MonoSingleton<PrefsManager>.Instance.GetInt("gunColorPreset.2.a");
			if (GameProgressSaver.GetTotalSecretsFound() < requiredSecrets[shotgunAltPreset])
			{
				shotgunAltPreset = 0;
				MonoSingleton<PrefsManager>.Instance.SetInt("gunColorPreset.2.a", 0);
			}
			currentShotgunAltColor = shotgunColors[shotgunAltPreset];
		}
		if (MonoSingleton<PrefsManager>.Instance.GetBool("gunColorType.3") && GameProgressSaver.HasWeaponCustomization(GameProgressSaver.WeaponCustomizationType.Nailgun))
		{
			currentNailgunColor = CustomGunColorPreset(3, altVersion: false);
		}
		else
		{
			nailgunPreset = MonoSingleton<PrefsManager>.Instance.GetInt("gunColorPreset.3");
			if (GameProgressSaver.GetTotalSecretsFound() < requiredSecrets[nailgunPreset])
			{
				nailgunPreset = 0;
				MonoSingleton<PrefsManager>.Instance.SetInt("gunColorPreset.3", 0);
			}
			currentNailgunColor = nailgunColors[nailgunPreset];
		}
		if (MonoSingleton<PrefsManager>.Instance.GetBool("gunColorType.3.a") && GameProgressSaver.HasWeaponCustomization(GameProgressSaver.WeaponCustomizationType.Nailgun))
		{
			currentNailgunAltColor = CustomGunColorPreset(3, altVersion: true);
		}
		else
		{
			nailgunAltPreset = MonoSingleton<PrefsManager>.Instance.GetInt("gunColorPreset.3.a");
			if (GameProgressSaver.GetTotalSecretsFound() < requiredSecrets[nailgunAltPreset])
			{
				nailgunAltPreset = 0;
				MonoSingleton<PrefsManager>.Instance.SetInt("gunColorPreset.3.a", 0);
			}
			currentNailgunAltColor = nailgunColors[nailgunAltPreset];
		}
		if (MonoSingleton<PrefsManager>.Instance.GetBool("gunColorType.4") && GameProgressSaver.HasWeaponCustomization(GameProgressSaver.WeaponCustomizationType.Railcannon))
		{
			currentRailcannonColor = CustomGunColorPreset(4, altVersion: false);
		}
		else
		{
			railcannonPreset = MonoSingleton<PrefsManager>.Instance.GetInt("gunColorPreset.4");
			if (GameProgressSaver.GetTotalSecretsFound() < requiredSecrets[railcannonPreset])
			{
				railcannonPreset = 0;
				MonoSingleton<PrefsManager>.Instance.SetInt("gunColorPreset.4", 0);
			}
			currentRailcannonColor = railcannonColors[railcannonPreset];
		}
		if (MonoSingleton<PrefsManager>.Instance.GetBool("gunColorType.5") && GameProgressSaver.HasWeaponCustomization(GameProgressSaver.WeaponCustomizationType.RocketLauncher))
		{
			currentRocketLauncherColor = CustomGunColorPreset(5, altVersion: false);
			return;
		}
		rocketLauncherPreset = MonoSingleton<PrefsManager>.Instance.GetInt("gunColorPreset.5");
		if (GameProgressSaver.GetTotalSecretsFound() < requiredSecrets[rocketLauncherPreset])
		{
			rocketLauncherPreset = 0;
			MonoSingleton<PrefsManager>.Instance.SetInt("gunColorPreset.5", 0);
		}
		currentRocketLauncherColor = rocketLauncherColors[rocketLauncherPreset];
	}

	private GunColorPreset CustomGunColorPreset(int gunNumber, bool altVersion)
	{
		return new GunColorPreset(GetGunColor(1, gunNumber, altVersion), GetGunColor(2, gunNumber, altVersion), GetGunColor(3, gunNumber, altVersion));
	}

	private Color GetGunColor(int number, int gunNumber, bool altVersion)
	{
		return new Color(MonoSingleton<PrefsManager>.Instance.GetFloat("gunColor." + gunNumber + "." + number + (altVersion ? ".a" : ".") + "r", 1f), MonoSingleton<PrefsManager>.Instance.GetFloat("gunColor." + gunNumber + "." + number + (altVersion ? ".a" : ".") + "g", 1f), MonoSingleton<PrefsManager>.Instance.GetFloat("gunColor." + gunNumber + "." + number + (altVersion ? ".a" : ".") + "b", 1f), MonoSingleton<PrefsManager>.Instance.GetFloat("gunColor." + gunNumber + "." + number + (altVersion ? ".a" : ".") + "a"));
	}
}
