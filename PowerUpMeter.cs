using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class PowerUpMeter : MonoSingleton<PowerUpMeter>
{
	public float juice;

	public float latestMaxJuice;

	private Image meter;

	public Image vignette;

	public Color powerUpColor;

	private Color currentColor;

	public GameObject endEffect;

	private bool hasPowerUp;

	private void Start()
	{
		meter = GetComponent<Image>();
		meter.fillAmount = 0f;
	}

	private void Update()
	{
		UpdateMeter();
	}

	public void UpdateMeter()
	{
		if (juice > 0f)
		{
			hasPowerUp = true;
			if (!InfinitePowerUps.Enabled)
			{
				juice -= Time.deltaTime;
			}
			if (HUDOptions.powerUpMeterEnabled)
			{
				meter.fillAmount = juice / latestMaxJuice;
			}
			else
			{
				meter.fillAmount = 0f;
			}
			if (currentColor != powerUpColor)
			{
				currentColor = powerUpColor;
				currentColor.a = juice / latestMaxJuice;
				vignette.color = currentColor;
			}
		}
		else if (hasPowerUp)
		{
			EndPowerUp();
		}
	}

	public void EndPowerUp()
	{
		hasPowerUp = false;
		juice = 0f;
		latestMaxJuice = 0f;
		meter.fillAmount = 0f;
		if (vignette.color.a != 0f)
		{
			currentColor.a = 0f;
			vignette.color = currentColor;
		}
		if ((bool)endEffect)
		{
			Object.Instantiate(endEffect, MonoSingleton<NewMovement>.Instance.transform.position, Quaternion.identity);
		}
	}
}
