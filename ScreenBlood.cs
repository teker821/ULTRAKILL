using UnityEngine;
using UnityEngine.UI;

public class ScreenBlood : MonoBehaviour
{
	private Image img;

	private Color clr;

	private RectTransform rct;

	public Sprite[] sprites;

	private void Start()
	{
		rct = GetComponent<RectTransform>();
		rct.anchoredPosition = new Vector2(Random.Range(-400, 400), Random.Range(-250, 250));
		img = GetComponent<Image>();
		img.sprite = sprites[Random.Range(0, sprites.Length)];
		clr = img.color;
	}

	private void Update()
	{
		if (clr.a > 0f)
		{
			clr.a -= Time.deltaTime;
			img.color = clr;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}
}
