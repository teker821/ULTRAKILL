using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Fishing;

public class FishIconGlow : MonoBehaviour
{
	private Image image;

	private void Awake()
	{
		image = GetComponent<Image>();
	}

	public void Blink()
	{
		StartCoroutine(BlinkCoroutine());
	}

	private IEnumerator BlinkCoroutine()
	{
		Color color = Color.white;
		color.a = 0.7f;
		image.color = color;
		while (color.a < 1f)
		{
			color.a += Time.deltaTime;
			image.color = color;
			yield return null;
		}
		while (color.a > 0f)
		{
			color.a -= Time.deltaTime;
			image.color = color;
			yield return null;
		}
	}
}
