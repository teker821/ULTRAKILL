using UnityEngine;

public class SpriteController : MonoBehaviour
{
	public Vector3 randomRotation;

	public bool blinking;

	public float fadeSpeed;

	public float shrinkSpeed;

	private SpriteRenderer spr;

	private float originalAlpha;

	private Vector3 originalScale;

	private void Awake()
	{
		if (randomRotation != Vector3.zero)
		{
			base.transform.Rotate(randomRotation * Random.Range(0, 360));
		}
		spr = GetComponent<SpriteRenderer>();
		originalAlpha = spr.color.a;
		originalScale = base.transform.localScale;
	}

	private void Update()
	{
		if (fadeSpeed != 0f)
		{
			if (blinking)
			{
				if (spr.color.a >= originalAlpha && fadeSpeed < 0f)
				{
					fadeSpeed = Mathf.Abs(fadeSpeed);
				}
				else if (spr.color.a <= 0f && fadeSpeed > 0f)
				{
					fadeSpeed = Mathf.Abs(fadeSpeed) * -1f;
				}
			}
			Color color = spr.color;
			color.a -= fadeSpeed * Time.deltaTime;
			spr.color = color;
		}
		if (shrinkSpeed > 0f)
		{
			base.transform.localScale = base.transform.localScale - Vector3.one * shrinkSpeed * Time.deltaTime;
		}
	}
}
