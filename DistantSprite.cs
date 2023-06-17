using UnityEngine;

public class DistantSprite : MonoBehaviour
{
	private SpriteRenderer sr;

	private Color clr;

	private float distance;

	private float originalAlpha;

	public float mininumDistance;

	public float maximumDistance;

	private void Start()
	{
		sr = GetComponent<SpriteRenderer>();
		originalAlpha = sr.color.a;
	}

	private void Update()
	{
		clr = sr.color;
		distance = Vector3.Distance(base.transform.position, MonoSingleton<CameraController>.Instance.transform.position);
		if (distance > mininumDistance)
		{
			if (distance > maximumDistance)
			{
				clr.a = originalAlpha;
			}
			else
			{
				clr.a = Mathf.Lerp(0f, originalAlpha, (distance - mininumDistance) / (maximumDistance - mininumDistance));
			}
		}
		else
		{
			clr.a = 0f;
		}
		sr.color = clr;
	}
}
