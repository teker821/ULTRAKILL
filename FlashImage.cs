using UnityEngine;
using UnityEngine.UI;

public class FlashImage : MonoBehaviour
{
	private Image img;

	private bool flashing;

	public float speed;

	public float flashAlpha;

	public bool dontFlashOnEnable;

	public bool oneTime;

	private bool flashed;

	private void OnEnable()
	{
		if (!dontFlashOnEnable && (!oneTime || !flashed))
		{
			Flash(flashAlpha);
		}
	}

	public void Flash(float amount)
	{
		if (oneTime)
		{
			if (flashed)
			{
				return;
			}
			flashed = true;
		}
		if (!img)
		{
			img = GetComponent<Image>();
		}
		img.color = new Color(img.color.r, img.color.g, img.color.b, amount);
		flashing = true;
	}

	private void Update()
	{
		if (flashing && (bool)img)
		{
			img.color = new Color(img.color.r, img.color.g, img.color.b, Mathf.MoveTowards(img.color.a, 0f, Time.deltaTime * speed));
			if (img.color.a <= 0f)
			{
				flashing = false;
			}
		}
	}
}
