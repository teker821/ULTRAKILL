using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class StyleFreshnessData
{
	public StyleFreshnessState state;

	public string text;

	public float scoreMultiplier;

	public float min;

	public float max;

	public Slider slider;

	public float span => Mathf.Abs(max - min);

	public float justAboveMin => min + span * 0.05f;
}
