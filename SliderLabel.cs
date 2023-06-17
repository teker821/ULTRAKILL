using UnityEngine;
using UnityEngine.UI;

public class SliderLabel : MonoBehaviour
{
	public bool floor = true;

	public float multiplier = 1f;

	[SerializeField]
	private Slider slider;

	[SerializeField]
	private string postfix = "";

	private Text text;

	private void Awake()
	{
		text = GetComponent<Text>();
	}

	private void Update()
	{
		if (floor)
		{
			text.text = $"{Mathf.FloorToInt(slider.value * multiplier)}{postfix}";
		}
		else
		{
			text.text = $"{(float)Mathf.FloorToInt(slider.value * multiplier * 10f) / 10f}{postfix}";
		}
	}
}
