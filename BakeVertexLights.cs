using System.Collections.Generic;
using UnityEngine;

public class BakeVertexLights : MonoBehaviour
{
	[SerializeField]
	[HideInInspector]
	private List<Renderer> bakedRenderers;

	[HideInInspector]
	public int UVTargetChannel = 2;

	private float _strength;

	public float Strength
	{
		get
		{
			return _strength;
		}
		set
		{
			if (_strength != value)
			{
				_strength = value;
				UpdateChannelStrength(UVTargetChannel, _strength);
			}
		}
	}

	private void UpdateChannelStrength(int targetChannel, float strength)
	{
		int num = Mathf.Clamp(targetChannel - 2, 1, 6);
		string text = $"_BakedLights{num}Strength";
		strength = Mathf.Clamp01(strength);
		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
		foreach (Renderer bakedRenderer in bakedRenderers)
		{
			bakedRenderer.GetPropertyBlock(materialPropertyBlock);
			materialPropertyBlock.SetFloat(text, strength);
			bakedRenderer.SetPropertyBlock(materialPropertyBlock);
		}
	}
}
