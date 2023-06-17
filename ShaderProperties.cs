using System.Runtime.CompilerServices;
using UnityEngine;

public static class ShaderProperties
{
	public static readonly int BaseMap = PrefixedPropertyToID("", "BaseMap");

	public static readonly int Color = PrefixedPropertyToID("", "Color");

	public static readonly int MainTex = PrefixedPropertyToID("", "MainTex");

	public static readonly int Cutoff = PrefixedPropertyToID("", "Cutoff");

	public static readonly int Glossiness = PrefixedPropertyToID("", "Glossiness");

	public static readonly int GlossMapScale = PrefixedPropertyToID("", "GlossMapScale");

	public static readonly int SmoothnessTextureChannel = PrefixedPropertyToID("", "SmoothnessTextureChannel");

	public static readonly int Metallic = PrefixedPropertyToID("", "Metallic");

	public static readonly int MetallicGlossMap = PrefixedPropertyToID("", "MetallicGlossMap");

	public static readonly int SpecularHighlights = PrefixedPropertyToID("", "SpecularHighlights");

	public static readonly int GlossyReflections = PrefixedPropertyToID("", "GlossyReflections");

	public static readonly int BumpScale = PrefixedPropertyToID("", "BumpScale");

	public static readonly int BumpMap = PrefixedPropertyToID("", "BumpMap");

	public static readonly int Parallax = PrefixedPropertyToID("", "Parallax");

	public static readonly int ParallaxMap = PrefixedPropertyToID("", "ParallaxMap");

	public static readonly int OcclusionStrength = PrefixedPropertyToID("", "OcclusionStrength");

	public static readonly int OcclusionMap = PrefixedPropertyToID("", "OcclusionMap");

	public static readonly int EmissionColor = PrefixedPropertyToID("", "EmissionColor");

	public static readonly int EmissionMap = PrefixedPropertyToID("", "EmissionMap");

	public static readonly int DetailMask = PrefixedPropertyToID("", "DetailMask");

	public static readonly int DetailAlbedoMap = PrefixedPropertyToID("", "DetailAlbedoMap");

	public static readonly int DetailNormalMapScale = PrefixedPropertyToID("", "DetailNormalMapScale");

	public static readonly int DetailNormalMap = PrefixedPropertyToID("", "DetailNormalMap");

	public static readonly int UVSec = PrefixedPropertyToID("", "UVSec");

	public static readonly int Mode = PrefixedPropertyToID("", "Mode");

	public static readonly int SrcBlend = PrefixedPropertyToID("", "SrcBlend");

	public static readonly int DstBlend = PrefixedPropertyToID("", "DstBlend");

	public static readonly int ZWrite = PrefixedPropertyToID("", "ZWrite");

	public static readonly int WorldSpaceCameraPos = PrefixedPropertyToID("", "WorldSpaceCameraPos");

	public static readonly int ProjectionParams = PrefixedPropertyToID("", "ProjectionParams");

	public static readonly int ScreenParams = PrefixedPropertyToID("", "ScreenParams");

	public static readonly int ZBufferParams = PrefixedPropertyToID("", "ZBufferParams");

	public static readonly int Time = PrefixedPropertyToID("", "Time");

	public static readonly int SinTime = PrefixedPropertyToID("", "SinTime");

	public static readonly int CosTime = PrefixedPropertyToID("", "CosTime");

	public static readonly int LightColor0 = PrefixedPropertyToID("", "LightColor0");

	public static readonly int WorldSpaceLightPos0 = PrefixedPropertyToID("", "WorldSpaceLightPos0");

	public static readonly int LightMatrix0 = PrefixedPropertyToID("", "LightMatrix0");

	public static readonly int TextureSampleAdd = PrefixedPropertyToID("", "TextureSampleAdd");

	public static readonly int OpacScale = PrefixedPropertyToID("", "OpacScale");

	public static int GetMainTexID(Material material)
	{
		if (!material.HasProperty(BaseMap))
		{
			return MainTex;
		}
		return BaseMap;
	}

	public static int PrefixedPropertyToID(string prefix = "", [CallerMemberName] string name = null)
	{
		if (!name.StartsWith("_"))
		{
			prefix += "_";
		}
		return Shader.PropertyToID(prefix + name);
	}
}
