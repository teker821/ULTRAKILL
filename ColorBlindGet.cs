using UnityEngine;
using UnityEngine.UI;

public class ColorBlindGet : MonoBehaviour
{
	public HudColorType hct;

	private Image img;

	private Text txt;

	private bool gotTarget;

	public bool variationColor;

	public int variationNumber;

	private void Start()
	{
		UpdateColor();
	}

	private void OnEnable()
	{
		UpdateColor();
	}

	public void UpdateColor()
	{
		if (!gotTarget)
		{
			GetTarget();
		}
		if ((bool)img)
		{
			if (!variationColor)
			{
				img.color = MonoSingleton<ColorBlindSettings>.Instance.GetHudColor(hct);
			}
			else
			{
				img.color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[variationNumber];
			}
		}
		else if (!variationColor)
		{
			txt.color = MonoSingleton<ColorBlindSettings>.Instance.GetHudColor(hct);
		}
		else
		{
			txt.color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[variationNumber];
		}
	}

	private void GetTarget()
	{
		gotTarget = true;
		img = GetComponent<Image>();
		txt = GetComponent<Text>();
	}
}
