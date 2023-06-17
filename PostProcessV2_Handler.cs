using System;
using UnityEngine;
using UnityEngine.Rendering;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class PostProcessV2_Handler : MonoSingleton<PostProcessV2_Handler>
{
	public Shader outlineCreate;

	private Material outlineProcessor;

	public Material postProcessV2_VSRM;

	[Space(10f)]
	public Texture sandTex;

	public Texture buffTex;

	public Texture ditherTexture;

	public int distance = 1;

	private Camera mainCam;

	public Camera hudCam;

	public Camera virtualCam;

	private RenderBuffer[] buffers = new RenderBuffer[2];

	private RenderTexture mainTex;

	private RenderTexture hudTex;

	private RenderTexture outlineTex_BufferA;

	private RenderTexture outlineTex_BufferB;

	private int width;

	private int height;

	private int lastWidth;

	private int lastHeight;

	private bool reinitializeTextures;

	private bool mainCameraOnly;

	[HideInInspector]
	public bool enableJFA;

	[HideInInspector]
	public float downscaleResolution;

	public Texture CurrentTexture;

	public Texture CurrentMapPaletteOverride;

	public Material radiantBuff;

	[SerializeField]
	private Material conversionMaterial;

	private void Start()
	{
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore)
		{
			Shader.EnableKeyword("NOOUTLINES");
		}
		else
		{
			Shader.DisableKeyword("NOOUTLINES");
		}
		outlineProcessor = new Material(outlineCreate);
		mainCam = MonoSingleton<CameraController>.Instance.cam;
		hudCam = mainCam.transform.Find("HUD Camera").GetComponent<Camera>();
		virtualCam = mainCam.transform.Find("Virtual Camera").GetComponent<Camera>();
		ReinitializeCameras();
		postProcessV2_VSRM.SetTexture("_Dither", ditherTexture);
		if (sandTex != null)
		{
			Shader.SetGlobalTexture("_SandTex", sandTex);
		}
		if (buffTex != null)
		{
			Shader.SetGlobalTexture("_BuffTex", buffTex);
		}
	}

	public void ColorPalette(bool stuff)
	{
		if (!(CurrentMapPaletteOverride != null))
		{
			MonoSingleton<PrefsManager>.Instance.SetBoolLocal("colorPalette", stuff);
			if (stuff && CurrentTexture != null)
			{
				Shader.EnableKeyword("PALETTIZE");
				Shader.SetGlobalInt("_ColorPrecision", 2048);
				MonoSingleton<ConvertPaletteToLUT>.Instance.ConvertPalette((Texture2D)CurrentTexture, conversionMaterial);
			}
			else
			{
				Shader.DisableKeyword("PALETTIZE");
				GraphicsOptions.ColorCompressionApply(MonoSingleton<PrefsManager>.Instance.GetInt("colorCompression"));
			}
		}
	}

	public void ApplyUserColorPalette(Texture tex)
	{
		CurrentTexture = tex;
		if (!(CurrentMapPaletteOverride != null))
		{
			MonoSingleton<ConvertPaletteToLUT>.Instance.ConvertPalette((Texture2D)tex, conversionMaterial);
		}
	}

	public void ApplyMapColorPalette(Texture tex)
	{
		if (tex == null)
		{
			CurrentMapPaletteOverride = null;
			ColorPalette(MonoSingleton<PrefsManager>.Instance.GetBoolLocal("colorPalette"));
			return;
		}
		MonoSingleton<ConvertPaletteToLUT>.Instance.ConvertPalette((Texture2D)tex, conversionMaterial);
		CurrentMapPaletteOverride = tex;
		Shader.SetGlobalTexture("_PaletteTex", tex);
		Shader.EnableKeyword("PALETTIZE");
		Shader.SetGlobalInt("_ColorPrecision", 2048);
	}

	private void ReinitializeCameras()
	{
		if (Application.isPlaying)
		{
			Camera.onPreRender = null;
			Camera.onPreRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPreRender, new Camera.CameraCallback(OnPreRenderCallback));
			Camera.onPostRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPostRender, new Camera.CameraCallback(OnPostRenderCallback));
		}
	}

	private void SetupRTs()
	{
		if ((bool)outlineTex_BufferA)
		{
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = outlineTex_BufferA;
			GL.Clear(clearDepth: false, clearColor: true, Color.clear);
			RenderTexture.active = active;
		}
		width = Screen.width;
		height = Screen.height;
		if (downscaleResolution != 0f)
		{
			float num = width;
			float num2 = height;
			float num3 = Mathf.Min(num, num2);
			Vector2 vector = new Vector2(num / num3, num2 / num3) * downscaleResolution;
			width = (int)vector.x;
			height = (int)vector.y;
		}
		bool flag = width != lastWidth || height != lastHeight;
		reinitializeTextures |= flag;
		lastWidth = width;
		lastHeight = height;
		Vector2 vector2 = new Vector2(width, height);
		postProcessV2_VSRM.SetVector("_VirtualRes", vector2);
		if (!reinitializeTextures)
		{
			return;
		}
		if ((bool)mainTex)
		{
			mainTex.Release();
			if ((bool)mainTex)
			{
				UnityEngine.Object.Destroy(mainTex);
			}
		}
		if ((bool)hudTex)
		{
			hudTex.Release();
			if ((bool)hudTex)
			{
				UnityEngine.Object.Destroy(hudTex);
			}
		}
		if ((bool)outlineTex_BufferA)
		{
			outlineTex_BufferA.Release();
			if ((bool)outlineTex_BufferA)
			{
				UnityEngine.Object.Destroy(outlineTex_BufferA);
			}
		}
		if ((bool)outlineTex_BufferB)
		{
			outlineTex_BufferB.Release();
			if ((bool)outlineTex_BufferB)
			{
				UnityEngine.Object.Destroy(outlineTex_BufferB);
			}
		}
		mainTex = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
		{
			antiAliasing = 1,
			filterMode = FilterMode.Point
		};
		hudTex = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
		{
			antiAliasing = 1,
			filterMode = FilterMode.Point
		};
		outlineTex_BufferA = new RenderTexture(width, height, 0, RenderTextureFormat.RG16)
		{
			name = "A",
			antiAliasing = 1,
			filterMode = FilterMode.Point
		};
		outlineTex_BufferB = new RenderTexture(width, height, 0, RenderTextureFormat.RG16)
		{
			name = "B",
			antiAliasing = 1,
			filterMode = FilterMode.Point
		};
		buffers[0] = mainTex.colorBuffer;
		buffers[1] = outlineTex_BufferA.colorBuffer;
		mainCam.SetTargetBuffers(buffers, mainTex.depthBuffer);
		hudCam.targetTexture = hudTex;
		postProcessV2_VSRM.SetTexture("_MainTex", mainTex);
		postProcessV2_VSRM.SetTexture("_HudTex", hudTex);
		reinitializeTextures = false;
	}

	private void ComputeOutlines()
	{
		Vector2 vector = new Vector2(outlineTex_BufferB.width, outlineTex_BufferB.height);
		Vector2 vector2 = vector / new Vector2(Screen.width, Screen.height);
		float num = (float)distance * Mathf.Max(vector2.x, vector2.y);
		postProcessV2_VSRM.SetFloat("_OutlineDist", num);
		if (!MonoSingleton<OptionsManager>.Instance.simplifyEnemies)
		{
			Shader.DisableKeyword("USEOUTLINES");
		}
		else
		{
			Shader.EnableKeyword("USEOUTLINES");
		}
		if (distance > 1 && num > 1f && MonoSingleton<OptionsManager>.Instance.simplifyEnemies)
		{
			distance = Mathf.Min(distance, 16);
			outlineProcessor.SetVector("_ScreenRes", new Vector2(width, height));
			Graphics.Blit(outlineTex_BufferA, outlineTex_BufferB, outlineProcessor, 0);
			float num2 = 8f;
			int num3 = 0;
			while (num2 >= 0.5f)
			{
				outlineProcessor.SetFloat("_Distance", num2);
				outlineProcessor.SetTexture("_MainTex", outlineTex_BufferB);
				outlineProcessor.SetVector("_Resolution", vector);
				Graphics.Blit(null, outlineTex_BufferA, outlineProcessor, 1);
				RenderTexture renderTexture = outlineTex_BufferA;
				outlineTex_BufferA = outlineTex_BufferB;
				outlineTex_BufferB = renderTexture;
				num2 *= 0.5f;
				num3++;
			}
			if (outlineTex_BufferA.name == "B")
			{
				RenderTexture renderTexture2 = outlineTex_BufferB;
				RenderTexture renderTexture3 = outlineTex_BufferA;
				outlineTex_BufferA = renderTexture2;
				outlineTex_BufferB = renderTexture3;
			}
			outlineProcessor.SetFloat("_Distance", distance);
			outlineProcessor.SetVector("_ResolutionDiff", vector2);
			Graphics.Blit(outlineTex_BufferB, outlineTex_BufferA, outlineProcessor, 2);
		}
		postProcessV2_VSRM.SetTexture("_OutlineTex", outlineTex_BufferA);
	}

	public void ChangeCamera(bool hudless)
	{
		mainCameraOnly = hudless;
		mainCam.targetTexture = null;
		MonoSingleton<CameraController>.Instance.cam.clearFlags = mainCam.clearFlags;
		mainCam = MonoSingleton<CameraController>.Instance.cam;
		virtualCam = mainCam.transform.Find("Virtual Camera").GetComponent<Camera>();
		reinitializeTextures = true;
		SetupRTs();
		Debug.Log("MainCamera: " + mainCam.name, mainCam);
	}

	public void OnPreRenderCallback(Camera cam)
	{
		if (cam == mainCam)
		{
			SetupRTs();
		}
	}

	private void OnPostRenderCallback(Camera cam)
	{
		if (cam == mainCam)
		{
			ComputeOutlines();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Camera.onPreRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPreRender, new Camera.CameraCallback(OnPreRenderCallback));
		Camera.onPostRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPostRender, new Camera.CameraCallback(OnPostRenderCallback));
	}
}
