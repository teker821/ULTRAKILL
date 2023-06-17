using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class WeaponIcon : MonoBehaviour
{
	[FormerlySerializedAs("descriptor")]
	public WeaponDescriptor weaponDescriptor;

	[SerializeField]
	private Renderer[] variationColoredRenderers;

	[SerializeField]
	private Material[] variationColoredMaterials;

	[SerializeField]
	private Image[] variationColoredImages;

	private int variationColor
	{
		get
		{
			if (!(weaponDescriptor == null))
			{
				return (int)weaponDescriptor.variationColor;
			}
			return -1;
		}
	}

	private void OnEnable()
	{
		UpdateMaterials();
	}

	public void UpdateMaterials()
	{
		variationColoredMaterials = new Material[variationColoredRenderers.Length];
		if (variationColoredRenderers.Length != 0)
		{
			for (int i = 0; i < variationColoredRenderers.Length; i++)
			{
				variationColoredMaterials[i] = new Material(variationColoredRenderers[i].material);
				if (variationColoredMaterials[i].HasProperty("_EmissiveColor"))
				{
					variationColoredMaterials[i].SetColor("_EmissiveColor", MonoSingleton<ColorBlindSettings>.Instance.variationColors[variationColor]);
				}
				else
				{
					variationColoredMaterials[i].color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[variationColor];
				}
				variationColoredRenderers[i].material = variationColoredMaterials[i];
			}
		}
		UpdateIcon();
	}

	public void UpdateIcon()
	{
		if ((bool)MonoSingleton<WeaponHUD>.Instance)
		{
			MonoSingleton<WeaponHUD>.Instance.UpdateImage(weaponDescriptor.icon, weaponDescriptor.glowIcon, variationColor);
		}
		Material[] array = variationColoredMaterials;
		foreach (Material material in array)
		{
			if (material.HasProperty("_EmissiveColor"))
			{
				material.SetColor("_EmissiveColor", MonoSingleton<ColorBlindSettings>.Instance.variationColors[variationColor]);
			}
			else
			{
				material.color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[variationColor];
			}
		}
		Image[] array2 = variationColoredImages;
		foreach (Image image in array2)
		{
			image.color = new Color(MonoSingleton<ColorBlindSettings>.Instance.variationColors[variationColor].r, MonoSingleton<ColorBlindSettings>.Instance.variationColors[variationColor].g, MonoSingleton<ColorBlindSettings>.Instance.variationColors[variationColor].b, image.color.a);
		}
	}
}
