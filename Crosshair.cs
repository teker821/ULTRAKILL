using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
	private int crossHairType;

	private Image mainch;

	public Image[] altchs;

	public Image[] chuds;

	public Sprite[] circles;

	public Material invertMaterial;

	private void Start()
	{
		mainch = GetComponent<Image>();
		MonoSingleton<StatsManager>.Instance.crosshair = base.gameObject;
		CheckCrossHair();
	}

	public void CheckCrossHair()
	{
		if (mainch == null)
		{
			mainch = GetComponent<Image>();
		}
		crossHairType = MonoSingleton<PrefsManager>.Instance.GetInt("crossHair");
		Image[] array;
		switch (crossHairType)
		{
		case 0:
		{
			mainch.enabled = false;
			array = altchs;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = false;
			}
			break;
		}
		case 1:
		{
			mainch.enabled = true;
			array = altchs;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = false;
			}
			break;
		}
		case 2:
		{
			mainch.enabled = true;
			array = altchs;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = true;
			}
			break;
		}
		}
		Color color = Color.white;
		int @int = MonoSingleton<PrefsManager>.Instance.GetInt("crossHairColor");
		switch (@int)
		{
		case 0:
		case 1:
			color = Color.white;
			break;
		case 2:
			color = Color.gray;
			break;
		case 3:
			color = Color.black;
			break;
		case 4:
			color = Color.red;
			break;
		case 5:
			color = Color.green;
			break;
		case 6:
			color = Color.blue;
			break;
		case 7:
			color = Color.cyan;
			break;
		case 8:
			color = Color.yellow;
			break;
		case 9:
			color = Color.magenta;
			break;
		}
		if (@int == 0)
		{
			mainch.material = invertMaterial;
		}
		else
		{
			mainch.material = null;
		}
		mainch.color = color;
		array = altchs;
		foreach (Image image in array)
		{
			image.color = color;
			if (@int == 0)
			{
				image.material = invertMaterial;
			}
			else
			{
				image.material = null;
			}
		}
		int int2 = MonoSingleton<PrefsManager>.Instance.GetInt("crossHairHud");
		if (int2 == 0)
		{
			array = chuds;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = false;
			}
			return;
		}
		array = chuds;
		foreach (Image obj in array)
		{
			obj.enabled = true;
			obj.sprite = circles[int2 - 1];
		}
	}
}
