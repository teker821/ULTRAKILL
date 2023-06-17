using UnityEngine;
using UnityEngine.UI;

public class SliderGetPlayerPref : MonoBehaviour
{
	public bool isFloat;

	public string playerPref;

	public float defaultFloat;

	public int defaultInt;

	private void Awake()
	{
		if (isFloat)
		{
			GetComponent<Slider>().value = PlayerPrefs.GetFloat(playerPref, defaultFloat);
		}
		else
		{
			GetComponent<Slider>().value = PlayerPrefs.GetInt(playerPref, defaultInt);
		}
	}
}
