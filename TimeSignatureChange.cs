using System;
using UnityEngine;

[Serializable]
public class TimeSignatureChange
{
	public float onMeasure;

	[HideInInspector]
	public float time;

	public float timeSignature;
}
