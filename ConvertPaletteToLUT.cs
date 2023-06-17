using System.Collections;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.PersistAutoInstance)]
public class ConvertPaletteToLUT : MonoSingleton<ConvertPaletteToLUT>
{
	public RenderTexture processedLUT;

	public RenderTexture lastLUT;

	public Texture2D lastPalette;

	private IEnumerator coroutine;

	public void ApplyLastPalette()
	{
		MonoBehaviour.print("reached");
		Shader.EnableKeyword("PALETTIZE");
		Shader.SetGlobalInt("_ColorPrecision", 2048);
		if (coroutine != null)
		{
			Shader.SetGlobalTexture("_LUT", processedLUT);
		}
	}

	public void ConvertPalette(Texture2D inputPalette, Material conversionMaterial)
	{
		if (!processedLUT)
		{
			processedLUT = new RenderTexture(4096, 4096, 0, RenderTextureFormat.ARGB32, 0);
		}
		Shader.SetGlobalTexture("_LUT", processedLUT);
		bool flag = true;
		if ((bool)lastPalette)
		{
			flag = lastPalette.name != inputPalette.name;
		}
		if (flag)
		{
			if (coroutine != null)
			{
				StopCoroutine(coroutine);
			}
			lastPalette = inputPalette;
			conversionMaterial.SetTexture("_Palette", inputPalette);
			lastLUT = new RenderTexture(processedLUT);
			processedLUT.antiAliasing = 1;
			processedLUT.filterMode = FilterMode.Point;
			lastLUT.antiAliasing = 1;
			lastLUT.filterMode = FilterMode.Point;
			coroutine = paletteToLUT(conversionMaterial);
			StartCoroutine(coroutine);
		}
	}

	private IEnumerator paletteToLUT(Material conversionMaterial)
	{
		int progress = 0;
		while (progress < 33)
		{
			conversionMaterial.SetInt("progress", progress);
			conversionMaterial.SetTexture("_LastLUT", lastLUT);
			Graphics.Blit(null, processedLUT, conversionMaterial);
			RenderTexture renderTexture = lastLUT;
			lastLUT = processedLUT;
			processedLUT = renderTexture;
			progress++;
			yield return null;
		}
		lastLUT.Release();
		Object.Destroy(lastLUT);
		Shader.SetGlobalTexture("_LUT", processedLUT);
		coroutine = null;
	}
}
