using UnityEngine;
using UnityEngine.UI;

public class CopyImage : MonoBehaviour
{
	private Image img;

	public Image imgToCopy;

	public CopyType copyType;

	public bool copyColor;

	private void Update()
	{
		if (img == null)
		{
			img = GetComponent<Image>();
		}
		if (imgToCopy == null && copyType != 0 && MonoSingleton<WeaponHUD>.Instance != null)
		{
			if (copyType == CopyType.WeaponIcon)
			{
				imgToCopy = MonoSingleton<WeaponHUD>.Instance.GetComponent<Image>();
			}
			else if (copyType == CopyType.WeaponShadow)
			{
				imgToCopy = MonoSingleton<WeaponHUD>.Instance.transform.GetChild(0).GetComponent<Image>();
			}
		}
		if (imgToCopy != null)
		{
			img.sprite = imgToCopy.sprite;
			if (copyColor)
			{
				img.color = imgToCopy.color;
			}
		}
	}
}
