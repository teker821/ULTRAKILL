using System;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarTemplate : MonoBehaviour
{
	public Action onDestroy;

	public BossHealthSliderTemplate sliderTemplate;

	public Text bossNameText;

	public BossHealthSliderTemplate thinSliderTemplate;

	private Text[] textInstances;

	public void Initialize()
	{
		textInstances = GetComponentsInChildren<Text>(includeInactive: false);
	}

	public void ScaleChanged(float scale)
	{
		Text[] array = textInstances;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].transform.localScale = new Vector3(scale, 1f, 1f);
		}
	}

	private void OnDestroy()
	{
		onDestroy?.Invoke();
	}
}
