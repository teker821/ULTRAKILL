using UnityEngine;
using UnityEngine.UI;

public class AssistOptions : MonoBehaviour
{
	public Toggle majorEnable;

	public Slider gameSpeed;

	public Slider damageTaken;

	public Toggle infiniteStamina;

	public Toggle disableWhiplashHardDamage;

	public Toggle disableHardDamage;

	public Toggle disableWeaponFreshness;

	public Toggle autoAim;

	public Slider autoAimSlider;

	public Dropdown bossDifficultyOverride;

	public Toggle hidePopup;

	public GameObject autoAimGroup;

	public GameObject majorPopup;

	public GameObject majorBlocker;

	public static float autoAimAmount;

	private void Start()
	{
		if (MonoSingleton<PrefsManager>.Instance.GetBool("majorAssist"))
		{
			majorEnable.isOn = true;
			majorBlocker.SetActive(value: false);
		}
		else
		{
			majorEnable.isOn = false;
			majorBlocker.SetActive(value: true);
		}
		gameSpeed.value = MonoSingleton<PrefsManager>.Instance.GetFloat("gameSpeed") * 100f;
		damageTaken.value = MonoSingleton<PrefsManager>.Instance.GetFloat("damageTaken") * 100f;
		infiniteStamina.isOn = MonoSingleton<PrefsManager>.Instance.GetBool("infiniteStamina");
		disableWhiplashHardDamage.isOn = MonoSingleton<PrefsManager>.Instance.GetBool("disableWhiplashHardDamage");
		disableHardDamage.isOn = MonoSingleton<PrefsManager>.Instance.GetBool("disableHardDamage");
		disableWeaponFreshness.isOn = MonoSingleton<PrefsManager>.Instance.GetBool("disableWeaponFreshness");
		autoAim.isOn = MonoSingleton<PrefsManager>.Instance.GetBool("autoAim");
		bossDifficultyOverride.value = MonoSingleton<PrefsManager>.Instance.GetInt("bossDifficultyOverride");
		bossDifficultyOverride.RefreshShownValue();
		MonoSingleton<AssistController>.Instance.difficultyOverride = bossDifficultyOverride.value - 1;
		hidePopup.isOn = MonoSingleton<PrefsManager>.Instance.GetBool("hideMajorAssistPopup");
		autoAimSlider.value = MonoSingleton<PrefsManager>.Instance.GetFloat("autoAimAmount") * 100f;
		MonoSingleton<CameraFrustumTargeter>.Instance.maxHorAim = autoAimSlider.value / 100f;
	}

	public void MajorCheck()
	{
		if (!MonoSingleton<PrefsManager>.Instance.GetBool("majorAssist"))
		{
			majorPopup.SetActive(value: true);
			return;
		}
		MonoSingleton<PrefsManager>.Instance.SetBool("majorAssist", content: false);
		MonoSingleton<AssistController>.Instance.majorEnabled = false;
		majorEnable.isOn = false;
		majorBlocker.SetActive(value: true);
	}

	public void MajorEnable()
	{
		MonoSingleton<PrefsManager>.Instance.SetBool("majorAssist", content: true);
		MonoSingleton<AssistController>.Instance.MajorEnabled();
		majorEnable.isOn = true;
		majorBlocker.SetActive(value: false);
	}

	public void GameSpeed(float stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetFloat("gameSpeed", stuff / 100f);
		MonoSingleton<AssistController>.Instance.gameSpeed = stuff / 100f;
	}

	public void DamageTaken(float stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetFloat("damageTaken", stuff / 100f);
		MonoSingleton<AssistController>.Instance.damageTaken = stuff / 100f;
	}

	public void InfiniteStamina(bool stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetBool("infiniteStamina", stuff);
		MonoSingleton<AssistController>.Instance.infiniteStamina = stuff;
	}

	public void DisableWhiplashHardDamage(bool stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetBool("disableWhiplashHardDamage", stuff);
		MonoSingleton<AssistController>.Instance.disableWhiplashHardDamage = stuff;
	}

	public void DisableHardDamage(bool stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetBool("disableHardDamage", stuff);
		MonoSingleton<AssistController>.Instance.disableHardDamage = stuff;
	}

	public void DisableWeaponFreshness(bool stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetBool("disableWeaponFreshness", stuff);
		MonoSingleton<AssistController>.Instance.disableWeaponFreshness = stuff;
	}

	public void HidePopup(bool stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetBool("hideMajorAssistPopup", stuff);
		MonoSingleton<AssistController>.Instance.hidePopup = stuff;
	}

	public void AutoAim(bool stuff)
	{
		CameraFrustumTargeter.IsEnabled = stuff;
		autoAimGroup.SetActive(stuff);
	}

	public void AutoAimAmount(float stuff)
	{
		if (CameraFrustumTargeter.IsEnabled && (bool)MonoSingleton<CameraFrustumTargeter>.Instance)
		{
			MonoSingleton<CameraFrustumTargeter>.Instance.maxHorAim = stuff / 100f;
		}
		MonoSingleton<PrefsManager>.Instance.SetFloat("autoAimAmount", stuff / 100f);
	}

	public void BossDifficultyOverride(int stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetInt("bossDifficultyOverride", stuff);
		MonoSingleton<AssistController>.Instance.difficultyOverride = stuff - 1;
	}
}
