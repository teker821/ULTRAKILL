using System.Globalization;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class StaminaMeter : MonoBehaviour
{
	private NewMovement nmov;

	private float stamina;

	private Slider stm;

	private Text stmText;

	public bool changeTextColor;

	public Color normalTextColor;

	private Image staminaFlash;

	private Color flashColor;

	private Image staminaBar;

	private bool full = true;

	private AudioSource aud;

	private Color emptyColor;

	private Color origColor;

	public bool redEmpty;

	private bool intro = true;

	private void Start()
	{
		Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
		stm = GetComponent<Slider>();
		if (stm != null)
		{
			staminaBar = base.transform.GetChild(1).GetChild(0).GetComponent<Image>();
			staminaFlash = staminaBar.transform.GetChild(0).GetComponent<Image>();
			flashColor = staminaFlash.color;
			origColor = staminaBar.color;
		}
		stmText = GetComponent<Text>();
		nmov = MonoSingleton<NewMovement>.Instance;
		UpdateColors();
	}

	private void OnEnable()
	{
		UpdateColors();
	}

	private void Update()
	{
		if (intro)
		{
			stamina = Mathf.MoveTowards(stamina, nmov.boostCharge, Time.deltaTime * ((nmov.boostCharge - stamina) * 5f + 10f));
			if (stamina >= nmov.boostCharge)
			{
				intro = false;
			}
		}
		else if (stamina < nmov.boostCharge)
		{
			stamina = Mathf.MoveTowards(stamina, nmov.boostCharge, Time.deltaTime * ((nmov.boostCharge - stamina) * 25f + 25f));
		}
		else if (stamina > nmov.boostCharge)
		{
			stamina = Mathf.MoveTowards(stamina, nmov.boostCharge, Time.deltaTime * ((stamina - nmov.boostCharge) * 25f + 25f));
		}
		if (stm != null)
		{
			stm.value = stamina;
			if (stm.value >= stm.maxValue && !full)
			{
				full = true;
				staminaBar.color = origColor;
				Flash();
			}
			if (flashColor.a > 0f)
			{
				if (flashColor.a - Time.deltaTime > 0f)
				{
					flashColor.a -= Time.deltaTime;
				}
				else
				{
					flashColor.a = 0f;
				}
				staminaFlash.color = flashColor;
			}
			if (stm.value < stm.maxValue)
			{
				full = false;
				staminaBar.color = emptyColor;
			}
		}
		if (!(stmText != null))
		{
			return;
		}
		stmText.text = (stamina / 100f).ToString("0.00");
		if (changeTextColor)
		{
			if (stamina < 100f)
			{
				stmText.color = Color.red;
			}
			else
			{
				stmText.color = MonoSingleton<ColorBlindSettings>.Instance.GetHudColor(HudColorType.stamina);
			}
		}
		else if (normalTextColor == Color.white)
		{
			if (stamina < 100f)
			{
				stmText.color = Color.red;
			}
			else
			{
				stmText.color = MonoSingleton<ColorBlindSettings>.Instance.GetHudColor(HudColorType.healthText);
			}
		}
	}

	public void Flash(bool red = false)
	{
		if (stm != null)
		{
			if (aud == null)
			{
				aud = GetComponent<AudioSource>();
			}
			aud.Play();
			if (red)
			{
				flashColor = Color.red;
			}
			else
			{
				flashColor = Color.white;
			}
			staminaFlash.color = flashColor;
		}
	}

	public void UpdateColors()
	{
		origColor = MonoSingleton<ColorBlindSettings>.Instance.staminaColor;
		if (redEmpty)
		{
			emptyColor = MonoSingleton<ColorBlindSettings>.Instance.staminaEmptyColor;
		}
		else
		{
			emptyColor = MonoSingleton<ColorBlindSettings>.Instance.staminaChargingColor;
		}
		if ((bool)staminaBar)
		{
			if (full)
			{
				staminaBar.color = origColor;
			}
			else
			{
				staminaBar.color = emptyColor;
			}
		}
	}
}
