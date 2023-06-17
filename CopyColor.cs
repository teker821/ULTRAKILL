using UnityEngine;
using UnityEngine.UI;

public class CopyColor : MonoBehaviour
{
	private Image img;

	public Image target;

	private void Start()
	{
		img = GetComponent<Image>();
	}

	private void Update()
	{
		if ((bool)img && (bool)target)
		{
			img.color = target.color;
		}
	}
}
