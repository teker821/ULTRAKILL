using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class WheelSegment
{
	public WeaponDescriptor descriptor;

	public UICircle segment;

	public UICircle divider;

	public Image icon;

	public Image iconGlow;

	public void SetActive(bool active)
	{
		segment.color = (active ? Color.red : Color.black);
		icon.color = (active ? Color.red : Color.white);
		Color color = (active ? Color.red : Color.black);
		color.a = 0.7f;
		iconGlow.color = color;
	}

	public void DestroySegment()
	{
		Object.Destroy(segment.gameObject);
		Object.Destroy(divider.gameObject);
	}
}
