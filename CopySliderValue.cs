using UnityEngine;
using UnityEngine.UI;

public class CopySliderValue : MonoBehaviour
{
	public Slider target;

	private Slider currentSlider;

	private void Start()
	{
		currentSlider = GetComponent<Slider>();
		currentSlider.value = target.value;
	}
}
