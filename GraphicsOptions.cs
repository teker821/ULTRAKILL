using UnityEngine;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class GraphicsOptions : MonoSingleton<GraphicsOptions>
{
	public Dropdown pixelization;

	public Slider textureWarping;

	public Dropdown vertexWarping;

	public Dropdown colorCompression;

	public Toggle vSync;

	public Slider dithering;

	public Toggle colorPalette;

	private new void Awake()
	{
		Initialize();
	}

	private void Start()
	{
		Initialize();
	}

	public void ApplyPalette(Texture2D palette)
	{
		MonoSingleton<PostProcessV2_Handler>.Instance.ApplyUserColorPalette(palette);
		ColorPalette(stuff: true);
	}

	public void PCPreset()
	{
		pixelization.value = 0;
		pixelization.RefreshShownValue();
		textureWarping.value = 0f;
		vertexWarping.value = 0;
		vertexWarping.RefreshShownValue();
		colorCompression.value = 2;
		colorCompression.RefreshShownValue();
		dithering.value = 10f;
	}

	public void PSXPreset()
	{
		pixelization.value = 3;
		pixelization.RefreshShownValue();
		textureWarping.value = 100f;
		vertexWarping.value = 2;
		vertexWarping.RefreshShownValue();
		colorCompression.value = 2;
		colorCompression.RefreshShownValue();
		dithering.value = 10f;
	}

	public void Initialize()
	{
		int @int = MonoSingleton<PrefsManager>.Instance.GetInt("pixelization");
		pixelization.value = @int;
		pixelization.RefreshShownValue();
		Pixelization(@int);
		float @float = MonoSingleton<PrefsManager>.Instance.GetFloat("textureWarping");
		textureWarping.value = @float;
		TextureWarping(@float);
		@int = MonoSingleton<PrefsManager>.Instance.GetInt("vertexWarping");
		vertexWarping.value = @int;
		vertexWarping.RefreshShownValue();
		VertexWarping(@int);
		@int = MonoSingleton<PrefsManager>.Instance.GetInt("colorCompression");
		colorCompression.value = @int;
		colorCompression.RefreshShownValue();
		ColorCompression(@int);
		@float = MonoSingleton<PrefsManager>.Instance.GetFloat("dithering");
		dithering.value = @float * 50f;
		Dithering(@float * 50f);
		if (!MonoSingleton<PrefsManager>.Instance.GetBoolLocal("colorPalette"))
		{
			colorPalette.isOn = false;
			ColorPalette(stuff: false);
		}
		else
		{
			Texture2D texture2D = CustomPaletteSelector.LoadSavedPalette();
			if ((bool)texture2D)
			{
				ApplyPalette(texture2D);
				colorPalette.isOn = false;
				colorPalette.isOn = true;
				ColorPalette(stuff: true);
				Shader.SetGlobalInt("_ColorPrecision", 2048);
			}
			else
			{
				colorPalette.isOn = false;
				ColorPalette(stuff: false);
			}
		}
		if (MonoSingleton<PrefsManager>.Instance.GetBoolLocal("vSync"))
		{
			QualitySettings.vSyncCount = 1;
			vSync.isOn = true;
		}
		else
		{
			QualitySettings.vSyncCount = 0;
			vSync.isOn = false;
		}
	}

	public void Pixelization(int stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetInt("pixelization", stuff);
		float num = 0f;
		switch (stuff)
		{
		case 0:
			num = 0f;
			break;
		case 1:
			num = 720f;
			break;
		case 2:
			num = 480f;
			break;
		case 3:
			num = 360f;
			break;
		case 4:
			num = 240f;
			break;
		case 5:
			num = 144f;
			break;
		case 6:
			num = 36f;
			break;
		}
		Shader.SetGlobalFloat("_ResY", num);
		PostProcessV2_Handler postProcessV2_Handler = MonoSingleton<PostProcessV2_Handler>.Instance;
		if ((bool)postProcessV2_Handler)
		{
			postProcessV2_Handler.downscaleResolution = num;
		}
		DownscaleChangeSprite[] array = Object.FindObjectsOfType<DownscaleChangeSprite>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].CheckScale();
		}
	}

	public void TextureWarping(float stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetFloat("textureWarping", stuff);
		float value = stuff * 0.003f;
		Shader.SetGlobalFloat("_TextureWarping", value);
	}

	public void VertexWarping(int stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetInt("vertexWarping", stuff);
		float value = 0f;
		switch (stuff)
		{
		case 0:
			value = 0f;
			break;
		case 1:
			value = 400f;
			break;
		case 2:
			value = 160f;
			break;
		case 3:
			value = 80f;
			break;
		case 4:
			value = 40f;
			break;
		case 5:
			value = 16f;
			break;
		}
		Shader.SetGlobalFloat("_VertexWarping", value);
	}

	public void ColorCompression(int stuff)
	{
		ColorCompressionApply(stuff);
	}

	public static void ColorCompressionApply(int stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetInt("colorCompression", stuff);
		if (!MonoSingleton<PrefsManager>.Instance.GetBoolLocal("colorPalette"))
		{
			switch (stuff)
			{
			case 0:
				Shader.SetGlobalInt("_ColorPrecision", 2048);
				break;
			case 1:
				Shader.SetGlobalInt("_ColorPrecision", 64);
				break;
			case 2:
				Shader.SetGlobalInt("_ColorPrecision", 32);
				break;
			case 3:
				Shader.SetGlobalInt("_ColorPrecision", 16);
				break;
			case 4:
				Shader.SetGlobalInt("_ColorPrecision", 8);
				break;
			case 5:
				Shader.SetGlobalInt("_ColorPrecision", 3);
				break;
			}
		}
	}

	public void Dithering(float stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetFloat("dithering", stuff / 50f);
		Shader.SetGlobalFloat("_DitherStrength", stuff / 50f);
	}

	public void VSync(bool stuff)
	{
		if (stuff)
		{
			QualitySettings.vSyncCount = 1;
			MonoSingleton<PrefsManager>.Instance.SetBoolLocal("vSync", content: true);
		}
		else
		{
			QualitySettings.vSyncCount = 0;
			MonoSingleton<PrefsManager>.Instance.SetBoolLocal("vSync", content: false);
		}
	}

	public void ColorPalette(bool stuff)
	{
		MonoSingleton<PostProcessV2_Handler>.Instance.ColorPalette(stuff);
	}
}
