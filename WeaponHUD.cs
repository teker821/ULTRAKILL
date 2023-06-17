using UnityEngine;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class WeaponHUD : MonoSingleton<WeaponHUD>
{
	private Image img;

	private Image glowImg;

	protected override void Awake()
	{
		base.Awake();
		WeaponIcon weaponIcon = Object.FindObjectOfType<WeaponIcon>();
		if ((bool)weaponIcon)
		{
			weaponIcon.UpdateIcon();
		}
	}

	public void UpdateImage(Sprite icon, Sprite glowIcon, int variation)
	{
		if (img == null)
		{
			img = GetComponent<Image>();
		}
		if (glowImg == null)
		{
			glowImg = base.transform.GetChild(0).GetComponent<Image>();
		}
		img.sprite = icon;
		img.color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[variation];
		glowImg.sprite = glowIcon;
		glowImg.color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[variation];
	}
}
