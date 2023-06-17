using UnityEngine;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class RailcannonMeter : MonoSingleton<RailcannonMeter>
{
	public Image meterBackground;

	public Image[] meters;

	private Image[] trueMeters;

	public Image colorlessMeter;

	private Image self;

	public GameObject[] altHudPanels;

	private float flashAmount;

	public GameObject miniVersion;

	private bool hasFlashed;

	private void Start()
	{
		CheckStatus();
	}

	private new void OnEnable()
	{
		CheckStatus();
	}

	private void Update()
	{
		if (self.enabled || miniVersion.activeSelf)
		{
			for (int i = 0; i < trueMeters.Length; i++)
			{
				if (self.enabled || i != 0)
				{
					trueMeters[i].enabled = true;
				}
				else
				{
					trueMeters[i].enabled = false;
				}
			}
			if (MonoSingleton<WeaponCharges>.Instance.raicharge > 4f)
			{
				if (!hasFlashed && Time.timeScale > 0f)
				{
					flashAmount = 1f;
				}
				hasFlashed = true;
				if (!MonoSingleton<ColorBlindSettings>.Instance)
				{
					return;
				}
				Color color = MonoSingleton<ColorBlindSettings>.Instance.GetHudColor(HudColorType.railcannonFull);
				if (flashAmount > 0f)
				{
					color = Color.Lerp(color, Color.white, flashAmount);
					flashAmount = Mathf.MoveTowards(flashAmount, 0f, Time.deltaTime);
				}
				Image[] array = trueMeters;
				foreach (Image image in array)
				{
					image.fillAmount = 1f;
					if (image != colorlessMeter)
					{
						image.color = color;
					}
					else
					{
						image.color = Color.white;
					}
				}
			}
			else
			{
				flashAmount = 0f;
				hasFlashed = false;
				Image[] array = trueMeters;
				foreach (Image obj in array)
				{
					obj.color = MonoSingleton<ColorBlindSettings>.Instance.GetHudColor(HudColorType.railcannonCharging);
					obj.fillAmount = MonoSingleton<WeaponCharges>.Instance.raicharge / 4f;
				}
			}
			if (MonoSingleton<WeaponCharges>.Instance.raicharge > 4f || !self.enabled)
			{
				meterBackground.enabled = false;
			}
			else
			{
				meterBackground.enabled = true;
			}
		}
		else
		{
			flashAmount = 0f;
			meterBackground.enabled = false;
			hasFlashed = false;
			Image[] array = trueMeters;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].enabled = false;
			}
		}
	}

	public void CheckStatus()
	{
		if (trueMeters == null || trueMeters.Length == 0)
		{
			trueMeters = new Image[meters.Length + 1];
			for (int i = 0; i < trueMeters.Length; i++)
			{
				if (i < meters.Length)
				{
					trueMeters[i] = meters[i];
				}
				else
				{
					trueMeters[i] = colorlessMeter;
				}
			}
		}
		if (!self)
		{
			self = GetComponent<Image>();
		}
		if (!MonoSingleton<HUDOptions>.Instance || !MonoSingleton<HUDOptions>.Instance.railcannonMeter)
		{
			return;
		}
		if (MonoSingleton<HUDOptions>.Instance.railcannonMeter.isOn && RailcannonStatus())
		{
			if (MonoSingleton<HUDOptions>.Instance.weaponIcon.isOn)
			{
				self.enabled = true;
				miniVersion.SetActive(value: false);
			}
			else
			{
				self.enabled = false;
				miniVersion.SetActive(value: true);
			}
			GameObject[] array = altHudPanels;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].SetActive(value: true);
			}
		}
		else
		{
			self.enabled = false;
			miniVersion.SetActive(value: false);
			GameObject[] array = altHudPanels;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].SetActive(value: false);
			}
		}
	}

	private bool RailcannonStatus()
	{
		for (int i = 0; i < 4; i++)
		{
			string text = "rai" + i;
			if (GameProgressSaver.CheckGear(text) == 1 && MonoSingleton<PrefsManager>.Instance.GetInt("weapon." + text, 1) == 1 && !MonoSingleton<GunControl>.Instance.noWeapons)
			{
				return true;
			}
		}
		return false;
	}
}
