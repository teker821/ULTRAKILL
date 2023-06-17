using UnityEngine;
using UnityEngine.UI;

public class MenuActSelect : MonoBehaviour
{
	public int requiredLevels;

	public bool forceOff;

	public bool hideWhenOff;

	public bool primeLevels;

	private Transform[] children;

	private Image img;

	private void OnEnable()
	{
		int @int = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		bool flag = false;
		if (primeLevels)
		{
			for (int i = 1; i < 4; i++)
			{
				if (GameProgressSaver.GetPrime(@int, i) > 0)
				{
					Debug.Log("Found Primes");
					flag = true;
				}
			}
		}
		if (forceOff || (GameProgressSaver.GetProgress(@int) <= requiredLevels && !flag))
		{
			GetComponent<Button>().interactable = false;
			base.transform.GetChild(0).GetComponent<Text>().color = new Color(0.3f, 0.3f, 0.3f);
			base.transform.GetChild(1).gameObject.SetActive(value: false);
			if (!hideWhenOff)
			{
				return;
			}
			if (!img)
			{
				img = GetComponent<Image>();
				children = base.transform.GetComponentsInChildren<Transform>();
			}
			img.enabled = false;
			Transform[] array = children;
			foreach (Transform transform in array)
			{
				if (transform != base.transform)
				{
					transform.gameObject.SetActive(value: false);
				}
			}
		}
		else
		{
			GetComponent<Button>().interactable = true;
			base.transform.GetChild(0).GetComponent<Text>().color = new Color(1f, 1f, 1f);
			base.transform.GetChild(1).gameObject.SetActive(value: true);
		}
	}

	private void OnDisable()
	{
		if (!hideWhenOff)
		{
			return;
		}
		if (!img)
		{
			img = GetComponent<Image>();
			children = base.transform.GetComponentsInChildren<Transform>();
		}
		img.enabled = true;
		Transform[] array = children;
		foreach (Transform transform in array)
		{
			if (transform != base.transform)
			{
				transform.gameObject.SetActive(value: true);
			}
		}
	}
}
