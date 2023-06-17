using UnityEngine;
using UnityEngine.UI;

public class ActivateOnSliderValues : MonoBehaviour
{
	public Slider[] sliders;

	public float[] values;

	public GameObject[] activateOnValues;

	public GameObject[] deactivateOnValues;

	private void Start()
	{
		CheckSliders();
	}

	private void Update()
	{
		CheckSliders();
	}

	private void CheckSliders()
	{
		int num = 0;
		for (int i = 0; i < sliders.Length; i++)
		{
			if (sliders[i].value == values[i])
			{
				num++;
			}
		}
		if (num == sliders.Length)
		{
			GameObject[] array = activateOnValues;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].SetActive(value: true);
			}
			array = deactivateOnValues;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].SetActive(value: false);
			}
		}
		else
		{
			GameObject[] array = activateOnValues;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].SetActive(value: false);
			}
			array = deactivateOnValues;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].SetActive(value: true);
			}
		}
	}
}
