using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
	private NewMovement nmov;

	public Slider[] hpSliders;

	public Slider[] afterImageSliders;

	public Slider antiHpSlider;

	public Text hpText;

	private float hp;

	private float antiHp;

	public bool changeTextColor;

	public Color normalTextColor;

	public bool yellowColor;

	public bool antiHpText;

	private int difficulty;

	private void Start()
	{
		nmov = MonoSingleton<NewMovement>.Instance;
		difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
	}

	private void Update()
	{
		if (hp < (float)nmov.hp)
		{
			hp = Mathf.MoveTowards(hp, nmov.hp, Time.deltaTime * (((float)nmov.hp - hp) * 5f + 5f));
		}
		else if (hp > (float)nmov.hp)
		{
			hp = nmov.hp;
		}
		if (hpSliders.Length != 0)
		{
			Slider[] array = hpSliders;
			foreach (Slider slider in array)
			{
				if (slider.value != hp)
				{
					slider.value = hp;
				}
			}
		}
		if (afterImageSliders != null)
		{
			Slider[] array = afterImageSliders;
			foreach (Slider slider2 in array)
			{
				if (slider2.value < hp)
				{
					slider2.value = hp;
				}
				else if (slider2.value > hp)
				{
					slider2.value = Mathf.MoveTowards(slider2.value, hp, Time.deltaTime * ((slider2.value - hp) * 5f + 5f));
				}
			}
		}
		if (antiHpSlider != null && antiHpSlider.value != nmov.antiHp)
		{
			antiHpSlider.value = Mathf.MoveTowards(antiHpSlider.value, nmov.antiHp, Time.deltaTime * (Mathf.Abs(antiHpSlider.value - nmov.antiHp) * 5f + 5f));
		}
		if (!(hpText != null))
		{
			return;
		}
		if (!antiHpText)
		{
			hpText.text = hp.ToString("F0");
			if (changeTextColor)
			{
				if (hp <= 30f)
				{
					hpText.color = Color.red;
				}
				else if (hp <= 50f && yellowColor)
				{
					hpText.color = Color.yellow;
				}
				else
				{
					hpText.color = normalTextColor;
				}
			}
			else if (normalTextColor == Color.white)
			{
				if (hp <= 30f)
				{
					hpText.color = Color.red;
				}
				else
				{
					hpText.color = MonoSingleton<ColorBlindSettings>.Instance.GetHudColor(HudColorType.healthText);
				}
			}
		}
		else if (difficulty == 0)
		{
			hpText.text = "/200";
		}
		else
		{
			antiHp = Mathf.MoveTowards(antiHp, nmov.antiHp, Time.deltaTime * (Mathf.Abs(antiHp - nmov.antiHp) * 5f + 5f));
			float num = 100f - antiHp;
			hpText.text = "/" + num.ToString("F0");
		}
	}
}
