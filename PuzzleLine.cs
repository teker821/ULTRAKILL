using UnityEngine;
using UnityEngine.UI;

public class PuzzleLine : MonoBehaviour
{
	private RectTransform imageRectTransform;

	private Image img;

	public int length;

	public void DrawLine(Vector3 pointA, Vector3 pointB, TileColor color)
	{
		if (imageRectTransform == null)
		{
			imageRectTransform = GetComponent<RectTransform>();
		}
		if (img == null)
		{
			img = GetComponent<Image>();
		}
		Vector3 vector = pointB - pointA;
		imageRectTransform.sizeDelta = new Vector2(length, 8f);
		imageRectTransform.pivot = new Vector2(0f, 0.5f);
		float z = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
		imageRectTransform.localRotation = Quaternion.Euler(0f, 0f, z);
		img.color = TranslateColor(color);
		img.enabled = true;
	}

	public void Hide()
	{
		if (img == null)
		{
			img = GetComponent<Image>();
		}
		img.enabled = false;
	}

	public Color TranslateColor(TileColor color)
	{
		Color result = Color.white;
		switch (color)
		{
		case TileColor.None:
			result = Color.white;
			break;
		case TileColor.Red:
			result = Color.red;
			break;
		case TileColor.Green:
			result = Color.green;
			break;
		case TileColor.Blue:
			result = new Color(0f, 0.25f, 1f);
			break;
		}
		return result;
	}
}
