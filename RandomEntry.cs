using System;
using UnityEngine;

[Serializable]
public class RandomEntry
{
	[Min(0f)]
	[Tooltip("The bigger the weight, the bigger the chance.")]
	public int weight = 1;
}
