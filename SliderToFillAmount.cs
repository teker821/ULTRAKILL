using UnityEngine;
using UnityEngine.UI;

public class SliderToFillAmount : MonoBehaviour
{
	public Slider targetSlider;

	public float maxFill;

	public bool copyColor;

	private Image img;

	public FadeOutBars mama;

	public bool dontFadeUntilEmpty;

	private void Update()
	{
		if (img == null)
		{
			img = GetComponent<Image>();
		}
		float fillAmount = img.fillAmount;
		img.fillAmount = (targetSlider.value - targetSlider.minValue) / (targetSlider.maxValue - targetSlider.minValue) * maxFill;
		if (img.fillAmount != fillAmount || (dontFadeUntilEmpty && targetSlider.value != targetSlider.minValue))
		{
			ResetFadeTimer();
		}
		if (copyColor)
		{
			img.color = targetSlider.targetGraphic.color;
		}
		if (mama != null)
		{
			Color color = img.color;
			if (mama.fadeOutTime < 1f)
			{
				color.a = mama.fadeOutTime;
			}
			else
			{
				color.a = 1f;
			}
			img.color = color;
		}
	}

	private void ResetFadeTimer()
	{
		if ((bool)mama)
		{
			mama.fadeOutTime = 2f;
		}
	}
}
