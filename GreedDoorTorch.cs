using UnityEngine;

public class GreedDoorTorch : MonoBehaviour
{
	private Door dr;

	private Light lt;

	private SpriteRenderer spr;

	private Color clr;

	private void Start()
	{
		dr = GetComponentInParent<Door>();
		lt = GetComponent<Light>();
		spr = GetComponentInChildren<SpriteRenderer>();
		UpdateColor();
	}

	private void Update()
	{
		if (clr != dr.currentLightsColor)
		{
			UpdateColor();
		}
	}

	private void UpdateColor()
	{
		clr = dr.currentLightsColor;
		if ((bool)spr)
		{
			spr.color = clr;
		}
		if ((bool)lt)
		{
			lt.color = clr;
		}
	}
}
