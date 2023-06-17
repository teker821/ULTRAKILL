using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sandbox;

public class AlterMenuElements : MonoBehaviour
{
	[SerializeField]
	private Transform container;

	[Header("Templates")]
	[SerializeField]
	private GameObject titleTemplate;

	[SerializeField]
	private GameObject boolRowTemplate;

	[SerializeField]
	private GameObject floatRowTemplate;

	private readonly List<int> createdRows = new List<int>();

	public void CreateTitle(string name)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(titleTemplate, container, worldPositionStays: false);
		gameObject.SetActive(value: true);
		gameObject.GetComponentInChildren<Text>().text = name;
		createdRows.Add(gameObject.GetInstanceID());
	}

	public void CreateBoolRow(string name, bool initialState, Action<bool> callback)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(boolRowTemplate, container, worldPositionStays: false);
		gameObject.SetActive(value: true);
		gameObject.GetComponentInChildren<Text>().text = name;
		Toggle componentInChildren = gameObject.GetComponentInChildren<Toggle>();
		componentInChildren.SetIsOnWithoutNotify(initialState);
		componentInChildren.onValueChanged.AddListener(delegate(bool state)
		{
			callback(state);
		});
		createdRows.Add(gameObject.GetInstanceID());
	}

	public void CreateFloatRow(string name, float initialState, Action<float> callback, IConstraints constraints = null)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(floatRowTemplate, container, worldPositionStays: false);
		gameObject.SetActive(value: true);
		gameObject.GetComponentInChildren<Text>().text = name;
		Slider componentInChildren = gameObject.GetComponentInChildren<Slider>();
		if (constraints is SliderConstraints sliderConstraints)
		{
			componentInChildren.minValue = sliderConstraints.min;
			componentInChildren.maxValue = sliderConstraints.max;
			componentInChildren.wholeNumbers = sliderConstraints.step == 1f;
			if (!componentInChildren.wholeNumbers)
			{
				gameObject.GetComponentInChildren<SliderLabel>().floor = false;
			}
		}
		componentInChildren.SetValueWithoutNotify(initialState);
		componentInChildren.onValueChanged.AddListener(delegate(float value)
		{
			callback(value);
		});
		createdRows.Add(gameObject.GetInstanceID());
	}

	public void Reset()
	{
		foreach (Transform item in container)
		{
			if (item.gameObject.activeSelf && !(item.gameObject == titleTemplate) && !(item.gameObject == boolRowTemplate) && !(item.gameObject == floatRowTemplate) && createdRows.Contains(item.gameObject.GetInstanceID()))
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
		createdRows.Clear();
	}
}
