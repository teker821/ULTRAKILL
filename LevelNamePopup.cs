using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class LevelNamePopup : MonoSingleton<LevelNamePopup>
{
	public Text[] layerText;

	private string layerString;

	public Text[] nameText;

	private string nameString;

	private bool activated;

	private bool fadingOut;

	private AudioSource aud;

	private void Start()
	{
		MapInfoBase instanceAnyType = MapInfoBase.InstanceAnyType;
		if ((bool)instanceAnyType)
		{
			layerString = instanceAnyType.layerName;
			nameString = instanceAnyType.levelName;
		}
		aud = GetComponent<AudioSource>();
		Text[] array = layerText;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].text = "";
		}
		array = nameText;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].text = "";
		}
	}

	private void Update()
	{
		if (fadingOut)
		{
			Color color = Color.white;
			Text[] array = layerText;
			foreach (Text obj in array)
			{
				color = obj.color;
				color.a = Mathf.MoveTowards(color.a, 0f, Time.deltaTime);
				obj.color = color;
			}
			array = nameText;
			foreach (Text obj2 in array)
			{
				color = obj2.color;
				color.a = Mathf.MoveTowards(color.a, 0f, Time.deltaTime);
				obj2.color = color;
			}
			if (color.a <= 0f)
			{
				fadingOut = false;
			}
		}
	}

	public void NameAppear()
	{
		if (!activated)
		{
			activated = true;
			aud.Play();
			StartCoroutine(ShowLayerText());
		}
	}

	private IEnumerator ShowLayerText()
	{
		for (int i = 0; i <= layerString.Length; i++)
		{
			Text[] array = layerText;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].text = layerString.Substring(0, i);
			}
			yield return new WaitForSeconds(0.001f);
		}
		aud.Stop();
		yield return new WaitForSeconds(0.5f);
		StartCoroutine(ShowNameText());
	}

	private IEnumerator ShowNameText()
	{
		aud.Play();
		for (int i = 0; i <= nameString.Length; i++)
		{
			Text[] array = nameText;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].text = nameString.Substring(0, i);
			}
			yield return new WaitForSeconds(0.001f);
		}
		aud.Stop();
		yield return new WaitForSeconds(3f);
		fadingOut = true;
	}
}
