using UnityEngine;

public class SpriteGetVariationColor : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer[] sprites;

	[SerializeField]
	private int variation;

	private void Update()
	{
		SpriteRenderer[] array = sprites;
		foreach (SpriteRenderer spriteRenderer in array)
		{
			spriteRenderer.color = new Color(MonoSingleton<ColorBlindSettings>.Instance.variationColors[variation].r, MonoSingleton<ColorBlindSettings>.Instance.variationColors[variation].g, MonoSingleton<ColorBlindSettings>.Instance.variationColors[variation].b, spriteRenderer.color.a);
		}
	}
}
