using System;
using UnityEngine;

[Serializable]
public class GunColorPreset
{
	public Color color1;

	public Color color2;

	public Color color3;

	public GunColorPreset(Color a, Color b, Color c)
	{
		color1 = a;
		color2 = b;
		color3 = c;
	}
}
