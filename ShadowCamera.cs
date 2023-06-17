using UnityEngine;
using UnityEngine.Rendering;

public class ShadowCamera
{
	public static (Matrix4x4, Matrix4x4, RenderTexture) RenderShadowMap(Light light, Bounds groupBounds, LayerMask shadowCullingMask)
	{
		GameObject gameObject = new GameObject("Shadow Camera");
		Camera camera = gameObject.AddComponent<Camera>();
		camera.transform.SetPositionAndRotation(light.transform.position, light.transform.rotation);
		camera.nearClipPlane = 0.01f;
		camera.cullingMask = shadowCullingMask;
		camera.clearFlags = CameraClearFlags.Color;
		camera.backgroundColor = Color.black;
		RenderTexture renderTexture;
		if (light.type == LightType.Directional)
		{
			renderTexture = new RenderTexture(2048, 2048, 24, RenderTextureFormat.RFloat)
			{
				dimension = TextureDimension.Tex2D,
				wrapMode = TextureWrapMode.Clamp,
				filterMode = FilterMode.Point,
				useMipMap = false
			};
			var (position, vector) = CalculateCameraParams(light.transform, groupBounds);
			camera.transform.position = position;
			camera.orthographic = true;
			camera.orthographicSize = vector.x;
			camera.nearClipPlane = vector.y;
			camera.farClipPlane = vector.z;
			camera.targetTexture = renderTexture;
			camera.backgroundColor = new Color(-9999f, -9999f, -9999f);
			Shader shader = Shader.Find("ULTRAKILL/Shadowmap_Directional");
			camera.SetReplacementShader(shader, "");
			camera.Render();
		}
		else
		{
			renderTexture = new RenderTexture(2048, 2048, 24, RenderTextureFormat.RFloat)
			{
				dimension = TextureDimension.Cube,
				wrapMode = TextureWrapMode.Clamp,
				filterMode = FilterMode.Point,
				useMipMap = false
			};
			camera.farClipPlane = light.range * 2f;
			camera.backgroundColor = new Color(9999f, 9999f, 9999f);
			Shader shader2 = Shader.Find("ULTRAKILL/Shadowmap_PointSpot");
			camera.SetReplacementShader(shader2, "");
			Shader.SetGlobalVector("bakeLightPos", camera.transform.position);
			camera.RenderToCubemap(renderTexture);
		}
		Matrix4x4 worldToCameraMatrix = camera.worldToCameraMatrix;
		Matrix4x4 projectionMatrix = camera.projectionMatrix;
		camera.targetTexture = null;
		camera.ResetReplacementShader();
		Object.DestroyImmediate(gameObject);
		return (worldToCameraMatrix, projectionMatrix, renderTexture);
	}

	public static Bounds CalculateGroupBounds(Renderer[] rends)
	{
		Bounds result = default(Bounds);
		foreach (Renderer renderer in rends)
		{
			result.Encapsulate(renderer.bounds);
		}
		return result;
	}

	public static (Vector3, Vector3) CalculateCameraParams(Transform lightTransform, Bounds groupBounds)
	{
		Vector3[] boundsVertices = GetBoundsVertices(groupBounds);
		Bounds bounds = new Bounds(lightTransform.InverseTransformPoint(boundsVertices[0]), Vector3.zero);
		Vector3[] array = boundsVertices;
		foreach (Vector3 position in array)
		{
			Vector3 point = lightTransform.InverseTransformPoint(position);
			bounds.Encapsulate(point);
		}
		Vector3 item = lightTransform.TransformPoint(bounds.center);
		float x = Mathf.Max(bounds.extents.x, bounds.extents.y);
		float y = 0f - bounds.extents.z;
		float z = bounds.extents.z;
		Vector3 item2 = new Vector3(x, y, z);
		return (item, item2);
	}

	private static Vector3[] GetBoundsVertices(Bounds bounds)
	{
		return new Vector3[8]
		{
			bounds.min,
			new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
			new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
			new Vector3(bounds.min.x, bounds.max.y, bounds.max.z),
			new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
			new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
			new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
			bounds.max
		};
	}
}
