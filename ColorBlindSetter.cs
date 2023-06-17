using UnityEngine;
using UnityEngine.UI;

public class ColorBlindSetter : MonoBehaviour
{
	private Text nameText;

	private string originalName;

	public new string name;

	public bool enemyColor;

	public bool variationColor;

	public HudColorType hct;

	public EnemyType ect;

	public int variationNumber;

	private Color originalColor;

	private Color newColor;

	public Image colorExample;

	private float redAmount;

	private float greenAmount;

	private float blueAmount;

	public Slider redSlider;

	public Slider greenSlider;

	public Slider blueSlider;

	private void OnEnable()
	{
		if ((bool)nameText && enemyColor)
		{
			if (MonoSingleton<BestiaryData>.Instance.GetEnemy(ect) < 1)
			{
				nameText.text = "???";
			}
			else
			{
				nameText.text = originalName;
			}
		}
	}

	public void Prepare()
	{
		if (variationColor)
		{
			originalColor = MonoSingleton<ColorBlindSettings>.Instance.variationColors[variationNumber];
		}
		else if (!enemyColor)
		{
			originalColor = MonoSingleton<ColorBlindSettings>.Instance.GetHudColor(hct);
		}
		else
		{
			originalColor = MonoSingleton<ColorBlindSettings>.Instance.GetEnemyColor(ect);
		}
		redAmount = MonoSingleton<PrefsManager>.Instance.GetFloat((enemyColor ? "enemyColor." : "hudColor.") + name + ".r", originalColor.r);
		greenAmount = MonoSingleton<PrefsManager>.Instance.GetFloat((enemyColor ? "enemyColor." : "hudColor.") + name + ".g", originalColor.g);
		blueAmount = MonoSingleton<PrefsManager>.Instance.GetFloat((enemyColor ? "enemyColor." : "hudColor.") + name + ".b", originalColor.b);
		newColor = new Color(redAmount, greenAmount, blueAmount, originalColor.a);
		colorExample.color = newColor;
		if (newColor != originalColor)
		{
			if (variationColor)
			{
				MonoSingleton<ColorBlindSettings>.Instance.variationColors[variationNumber] = newColor;
				MonoSingleton<ColorBlindSettings>.Instance.UpdateWeaponColors();
			}
			else if (!enemyColor)
			{
				MonoSingleton<ColorBlindSettings>.Instance.SetHudColor(hct, newColor);
			}
			else
			{
				MonoSingleton<ColorBlindSettings>.Instance.SetEnemyColor(ect, newColor);
			}
		}
		redSlider.value = redAmount;
		greenSlider.value = greenAmount;
		blueSlider.value = blueAmount;
		nameText = base.transform.GetChild(0).GetComponent<Text>();
		originalName = nameText.text;
		if (MonoSingleton<BestiaryData>.Instance.GetEnemy(ect) < 1 && enemyColor)
		{
			nameText.text = "???";
		}
	}

	private void UpdateColor()
	{
		_ = newColor;
		if (newColor.a == 0f)
		{
			return;
		}
		bool flag = false;
		if (newColor.r != redAmount)
		{
			newColor.r = redAmount;
			MonoSingleton<PrefsManager>.Instance.SetFloat((enemyColor ? "enemyColor." : "hudColor.") + name + ".r", redAmount);
			flag = true;
		}
		if (newColor.g != greenAmount)
		{
			newColor.g = greenAmount;
			MonoSingleton<PrefsManager>.Instance.SetFloat((enemyColor ? "enemyColor." : "hudColor.") + name + ".g", greenAmount);
			flag = true;
		}
		if (newColor.b != blueAmount)
		{
			newColor.b = blueAmount;
			MonoSingleton<PrefsManager>.Instance.SetFloat((enemyColor ? "enemyColor." : "hudColor.") + name + ".b", blueAmount);
			flag = true;
		}
		colorExample.color = newColor;
		if (flag)
		{
			if (variationColor)
			{
				MonoSingleton<ColorBlindSettings>.Instance.variationColors[variationNumber] = newColor;
				MonoSingleton<ColorBlindSettings>.Instance.UpdateWeaponColors();
			}
			else if (!enemyColor)
			{
				MonoSingleton<ColorBlindSettings>.Instance.SetHudColor(hct, newColor);
			}
			else
			{
				MonoSingleton<ColorBlindSettings>.Instance.SetEnemyColor(ect, newColor);
			}
		}
	}

	public void ChangeRed(float amount)
	{
		redAmount = amount;
		UpdateColor();
	}

	public void ChangeGreen(float amount)
	{
		greenAmount = amount;
		UpdateColor();
	}

	public void ChangeBlue(float amount)
	{
		blueAmount = amount;
		UpdateColor();
	}

	public void ResetToDefault()
	{
		redAmount = originalColor.r;
		greenAmount = originalColor.g;
		blueAmount = originalColor.b;
		redSlider.value = redAmount;
		greenSlider.value = greenAmount;
		blueSlider.value = blueAmount;
		UpdateColor();
	}
}
