using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class HUDOptions : MonoSingleton<HUDOptions>
{
	public Dropdown hudType;

	private HudController[] hudCons;

	public Slider bgOpacity;

	public Toggle alwaysOnTop;

	public Material hudMaterial;

	private Mask[] masks;

	public Toggle weaponIcon;

	public Toggle armIcon;

	public Toggle railcannonMeter;

	public Toggle styleMeter;

	public Toggle styleInfo;

	[SerializeField]
	private Dropdown iconPackDropdown;

	private Crosshair crosshair;

	public Dropdown crossHairType;

	public Dropdown crossHairColor;

	public Dropdown crossHairHud;

	public Toggle crossHairHudFade;

	[SerializeField]
	private Toggle powerUpMeter;

	[HideInInspector]
	public static bool powerUpMeterEnabled = true;

	private void Start()
	{
		crosshair = GetComponentInChildren<Crosshair>();
		crossHairType.value = MonoSingleton<PrefsManager>.Instance.GetInt("crossHair");
		crossHairType.RefreshShownValue();
		crossHairColor.value = MonoSingleton<PrefsManager>.Instance.GetInt("crossHairColor");
		crossHairColor.RefreshShownValue();
		crossHairHud.value = MonoSingleton<PrefsManager>.Instance.GetInt("crossHairHud");
		crossHairHud.RefreshShownValue();
		hudType.value = MonoSingleton<PrefsManager>.Instance.GetInt("hudType");
		hudType.RefreshShownValue();
		bgOpacity.value = MonoSingleton<PrefsManager>.Instance.GetFloat("hudBackgroundOpacity");
		hudCons = Object.FindObjectsOfType<HudController>();
		for (int i = 0; i < hudCons.Length; i++)
		{
			if (!hudCons[i].altHud)
			{
				masks = hudCons[i].GetComponentsInChildren<Mask>(includeInactive: true);
				break;
			}
		}
		if (MonoSingleton<PrefsManager>.Instance.GetBool("hudAlwaysOnTop"))
		{
			alwaysOnTop.isOn = true;
			AlwaysOnTop(stuff: true);
		}
		else
		{
			AlwaysOnTop(stuff: false);
		}
		weaponIcon.isOn = MonoSingleton<PrefsManager>.Instance.GetBool("weaponIcons");
		armIcon.isOn = MonoSingleton<PrefsManager>.Instance.GetBool("armIcons");
		railcannonMeter.isOn = MonoSingleton<PrefsManager>.Instance.GetBool("railcannonMeter");
		styleMeter.isOn = MonoSingleton<PrefsManager>.Instance.GetBool("styleMeter");
		styleInfo.isOn = MonoSingleton<PrefsManager>.Instance.GetBool("styleInfo");
		crossHairHudFade.isOn = MonoSingleton<PrefsManager>.Instance.GetBool("crossHairHudFade");
		powerUpMeter.isOn = MonoSingleton<PrefsManager>.Instance.GetBool("powerUpMeter");
		iconPackDropdown.options = (from p in MonoSingleton<IconManager>.Instance.AvailableIconPacks()
			select new Dropdown.OptionData(p)).ToList();
		iconPackDropdown.SetValueWithoutNotify(MonoSingleton<IconManager>.Instance.CurrentIconPackId);
	}

	public void SetIconPack(int packId)
	{
		MonoSingleton<IconManager>.Instance.SetIconPack(packId);
		MonoSingleton<IconManager>.Instance.Reload();
	}

	public void CrossHairType(int stuff)
	{
		if (crosshair == null)
		{
			crosshair = GetComponentInChildren<Crosshair>();
		}
		MonoSingleton<PrefsManager>.Instance.SetInt("crossHair", stuff);
		if (crosshair != null)
		{
			crosshair.CheckCrossHair();
		}
	}

	public void CrossHairColor(int stuff)
	{
		if (crosshair == null)
		{
			crosshair = GetComponentInChildren<Crosshair>();
		}
		MonoSingleton<PrefsManager>.Instance.SetInt("crossHairColor", stuff);
		if (crosshair != null)
		{
			crosshair.CheckCrossHair();
		}
	}

	public void CrossHairHud(int stuff)
	{
		if (crosshair == null)
		{
			crosshair = GetComponentInChildren<Crosshair>();
		}
		MonoSingleton<PrefsManager>.Instance.SetInt("crossHairHud", stuff);
		if (crosshair != null)
		{
			crosshair.CheckCrossHair();
		}
	}

	public void HudType(int stuff)
	{
		if (hudCons == null || hudCons.Length < 4)
		{
			hudCons = Object.FindObjectsOfType<HudController>();
		}
		MonoSingleton<PrefsManager>.Instance.SetInt("hudType", stuff);
		HudController[] array = hudCons;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].CheckSituation();
		}
		GetComponent<OptionsMenuToManager>().CheckEasterEgg();
	}

	public void HudFade(bool stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetBool("crossHairHudFade", stuff);
		FadeOutBars[] array = Object.FindObjectsOfType<FadeOutBars>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].CheckState();
		}
	}

	public void PowerUpMeterEnable(bool stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetBool("powerUpMeter", stuff);
		powerUpMeterEnabled = stuff;
		if ((bool)MonoSingleton<PowerUpMeter>.Instance)
		{
			MonoSingleton<PowerUpMeter>.Instance.UpdateMeter();
		}
	}

	public void BgOpacity(float stuff)
	{
		if (hudCons == null || hudCons.Length < 4)
		{
			hudCons = Object.FindObjectsOfType<HudController>();
		}
		MonoSingleton<PrefsManager>.Instance.SetFloat("hudBackgroundOpacity", stuff);
		HudController[] array = hudCons;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetOpacity(stuff);
		}
	}

	public void AlwaysOnTop(bool stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetBool("hudAlwaysOnTop", stuff);
		if (stuff)
		{
			hudMaterial.SetFloat("_ZTest", 8f);
		}
		else
		{
			hudMaterial.SetFloat("_ZTest", 4f);
		}
		Mask[] array = masks;
		foreach (Mask mask in array)
		{
			if (mask.enabled)
			{
				mask.enabled = false;
				mask.enabled = true;
			}
		}
	}

	public void WeaponIcon(bool stuff)
	{
		if (hudCons == null || hudCons.Length < 4)
		{
			hudCons = Object.FindObjectsOfType<HudController>();
		}
		MonoSingleton<PrefsManager>.Instance.SetBool("weaponIcons", stuff);
		if (stuff)
		{
			HudController[] array = hudCons;
			foreach (HudController hudController in array)
			{
				if (!hudController.altHud)
				{
					hudController.weaponIcon.SetActive(value: true);
					hudController.weaponIcon.transform.localPosition = new Vector3(hudController.weaponIcon.transform.localPosition.x, hudController.weaponIcon.transform.localPosition.y, 45f);
				}
				else
				{
					hudController.weaponIcon.SetActive(value: true);
				}
			}
		}
		else
		{
			HudController[] array = hudCons;
			foreach (HudController hudController2 in array)
			{
				if (!hudController2.altHud)
				{
					hudController2.weaponIcon.transform.localPosition = new Vector3(hudController2.weaponIcon.transform.localPosition.x, hudController2.weaponIcon.transform.localPosition.y, -9999f);
				}
				else
				{
					hudController2.weaponIcon.SetActive(value: false);
				}
			}
		}
		MonoSingleton<RailcannonMeter>.Instance?.CheckStatus();
	}

	public void ArmIcon(bool stuff)
	{
		if (hudCons == null || hudCons.Length < 4)
		{
			hudCons = Object.FindObjectsOfType<HudController>();
		}
		MonoSingleton<PrefsManager>.Instance.SetBool("armIcons", stuff);
		HudController[] array;
		if (stuff)
		{
			array = hudCons;
			foreach (HudController hudController in array)
			{
				if (!hudController.altHud)
				{
					hudController.armIcon.transform.localPosition = new Vector3(hudController.armIcon.transform.localPosition.x, hudController.armIcon.transform.localPosition.y, 0f);
				}
				else
				{
					hudController.armIcon.SetActive(value: true);
				}
			}
			return;
		}
		array = hudCons;
		foreach (HudController hudController2 in array)
		{
			if (!hudController2.altHud)
			{
				hudController2.armIcon.transform.localPosition = new Vector3(hudController2.armIcon.transform.localPosition.x, hudController2.armIcon.transform.localPosition.y, -9999f);
			}
			else
			{
				hudController2.armIcon.SetActive(value: false);
			}
		}
	}

	public void RailcannonMeterOption(bool stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetBool("railcannonMeter", stuff);
		MonoSingleton<RailcannonMeter>.Instance?.CheckStatus();
	}

	public void StyleMeter(bool stuff)
	{
		if (hudCons == null || hudCons.Length < 4)
		{
			hudCons = Object.FindObjectsOfType<HudController>();
		}
		MonoSingleton<PrefsManager>.Instance.SetBool("styleMeter", stuff);
		HudController[] array;
		if (stuff)
		{
			array = hudCons;
			foreach (HudController hudController in array)
			{
				if (!hudController.altHud)
				{
					hudController.styleMeter.transform.localPosition = new Vector3(hudController.styleMeter.transform.localPosition.x, hudController.styleMeter.transform.localPosition.y, 0f);
				}
			}
			return;
		}
		array = hudCons;
		foreach (HudController hudController2 in array)
		{
			if (!hudController2.altHud)
			{
				hudController2.styleMeter.transform.localPosition = new Vector3(hudController2.styleMeter.transform.localPosition.x, hudController2.styleMeter.transform.localPosition.y, -9999f);
			}
		}
	}

	public void StyleInfo(bool stuff)
	{
		if (hudCons == null || hudCons.Length < 4)
		{
			hudCons = Object.FindObjectsOfType<HudController>();
		}
		MonoSingleton<PrefsManager>.Instance.SetBool("styleInfo", stuff);
		HudController[] array;
		if (stuff)
		{
			array = hudCons;
			foreach (HudController hudController in array)
			{
				if (!hudController.altHud)
				{
					hudController.styleInfo.transform.localPosition = new Vector3(hudController.styleInfo.transform.localPosition.x, hudController.styleInfo.transform.localPosition.y, 0f);
					MonoSingleton<StyleHUD>.Instance.GetComponent<AudioSource>().enabled = true;
				}
			}
			return;
		}
		array = hudCons;
		foreach (HudController hudController2 in array)
		{
			if (!hudController2.altHud)
			{
				hudController2.styleInfo.transform.localPosition = new Vector3(hudController2.styleInfo.transform.localPosition.x, hudController2.styleInfo.transform.localPosition.y, -9999f);
				MonoSingleton<StyleHUD>.Instance.GetComponent<AudioSource>().enabled = false;
			}
		}
	}
}
