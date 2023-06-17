using UnityEngine;

public class ColorSchemeSetter : MonoBehaviour
{
	public bool replaceDitherUserSetting;

	public float ditheringAmount;

	public bool enforceMapColorPalette;

	public Texture mapDefinedPalette;

	public bool applyOnPlayerTriggerEnter;

	public bool applyOnPlayerTriggerExit;

	public bool oneTime;

	public void Apply()
	{
		if (replaceDitherUserSetting)
		{
			Shader.SetGlobalFloat("_DitherStrength", ditheringAmount);
		}
		if (enforceMapColorPalette)
		{
			MonoSingleton<PostProcessV2_Handler>.Instance.ApplyMapColorPalette(mapDefinedPalette);
		}
		if (oneTime)
		{
			Object.Destroy(this);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (applyOnPlayerTriggerEnter && other.CompareTag("Player"))
		{
			Apply();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (applyOnPlayerTriggerExit && other.CompareTag("Player"))
		{
			Apply();
		}
	}
}
