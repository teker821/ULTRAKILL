using UnityEngine;
using UnityEngine.UI;

public class GunColorTypeGetter : MonoBehaviour
{
	public int weaponNumber;

	public bool altVersion;

	public GameObject template;

	public GameObject custom;

	public GameObject altButton;

	public GunColorGetter[] previewModel;

	public Image[] templateButtons;

	public Text[] templateTexts;

	private string[] originalTemplateTexts;

	private void Awake()
	{
		originalTemplateTexts = new string[templateTexts.Length];
		for (int i = 0; i < templateTexts.Length; i++)
		{
			originalTemplateTexts[i] = templateTexts[i].text;
		}
	}

	private void OnEnable()
	{
		if (MonoSingleton<PrefsManager>.Instance.GetBool("gunColorType." + weaponNumber + (altVersion ? ".a" : "")) && GameProgressSaver.HasWeaponCustomization((GameProgressSaver.WeaponCustomizationType)(weaponNumber - 1)))
		{
			template.SetActive(value: false);
			custom.SetActive(value: true);
		}
		else
		{
			template.SetActive(value: true);
			custom.SetActive(value: false);
		}
		UpdateButtons(MonoSingleton<PrefsManager>.Instance.GetInt("gunColorPreset." + weaponNumber + (altVersion ? ".a" : "")));
		if ((bool)altButton)
		{
			string gear = "";
			switch (weaponNumber)
			{
			case 1:
				gear = "revalt";
				break;
			case 2:
				gear = "shoalt";
				break;
			case 3:
				gear = "naialt";
				break;
			}
			if (GameProgressSaver.CheckGear(gear) >= 1)
			{
				altButton.SetActive(value: true);
			}
			else
			{
				altButton.SetActive(value: false);
			}
		}
		for (int i = 1; i < 5; i++)
		{
			bool flag = GameProgressSaver.GetTotalSecretsFound() >= GunColorController.requiredSecrets[i];
			templateButtons[i].GetComponent<Button>().interactable = flag;
			templateButtons[i].GetComponent<ShopButton>().failure = !flag;
			if (flag)
			{
				templateTexts[i].text = originalTemplateTexts[i];
				if (templateTexts[i].color == Color.gray)
				{
					templateTexts[i].color = Color.white;
				}
				continue;
			}
			if (MonoSingleton<PrefsManager>.Instance.GetInt("gunColorPreset." + weaponNumber + (altVersion ? ".a" : "")) == i)
			{
				if (MonoSingleton<PrefsManager>.Instance.GetBool("gunColorType." + weaponNumber + (altVersion ? ".a" : "")))
				{
					MonoSingleton<PrefsManager>.Instance.SetInt("gunColorPreset." + weaponNumber + (altVersion ? ".a" : ""), 0);
					UpdateButtons(0);
				}
				else
				{
					SetPreset(0);
				}
			}
			templateTexts[i].text = "SOUL ORBS: " + GameProgressSaver.GetTotalSecretsFound() + " / " + GunColorController.requiredSecrets[i];
			templateTexts[i].color = Color.gray;
		}
	}

	public void SetType(bool isCustom)
	{
		if (!GameProgressSaver.HasWeaponCustomization((GameProgressSaver.WeaponCustomizationType)(weaponNumber - 1)))
		{
			isCustom = false;
		}
		MonoSingleton<PrefsManager>.Instance.SetBool("gunColorType." + weaponNumber + (altVersion ? ".a" : ""), isCustom);
		MonoSingleton<GunColorController>.Instance.UpdateGunColors();
		UpdatePreview();
	}

	public void SetPreset(int target)
	{
		MonoSingleton<PrefsManager>.Instance.SetInt("gunColorPreset." + weaponNumber + (altVersion ? ".a" : ""), target);
		MonoSingleton<GunColorController>.Instance.UpdateGunColors();
		UpdateButtons(target);
		UpdatePreview();
	}

	public void UpdatePreview()
	{
		GunColorGetter[] array = previewModel;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].UpdateColor();
		}
	}

	public void UpdateButtons(int target)
	{
		for (int i = 0; i < templateButtons.Length; i++)
		{
			templateButtons[i].fillCenter = i == target;
			if (templateTexts[i].color != Color.gray)
			{
				templateTexts[i].color = ((i == target) ? Color.black : Color.white);
			}
		}
	}
}
