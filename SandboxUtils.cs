using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using UnityEngine;
using UnityEngine.ProBuilder;

public static class SandboxUtils
{
	public const float SnapGrid = 0.5f;

	private const float MeshDensity = 0.25f;

	private const float UvScale = 1f;

	private const float PreviewBlockUvScale = 0.05f;

	public static void StripForPreview(Transform target, Material newMaterial = null, bool first = true)
	{
		Component[] components = target.GetComponents<Component>();
		if (first)
		{
			List<Component> list = new List<Component>();
			list.AddRange(target.GetComponentsInChildren<Joint>(includeInactive: true));
			list.AddRange(target.GetComponentsInChildren<Sisyphus>(includeInactive: true));
			list.AddRange(target.GetComponentsInChildren<AudioDistortionFilter>(includeInactive: true));
			list.AddRange(target.GetComponentsInChildren<ProBuilderMesh>(includeInactive: true));
			foreach (Component item in list)
			{
				UnityEngine.Object.Destroy(item);
			}
		}
		if ((bool)target.GetComponent<RemoveOnTime>() || (bool)target.GetComponent<SpawnEffect>() || (target.TryGetComponent<BoxCollider>(out var component) && component.isTrigger))
		{
			UnityEngine.Object.Destroy(target.gameObject);
			return;
		}
		Component[] array = components;
		foreach (Component component2 in array)
		{
			if ((bool)newMaterial && component2 is Renderer renderer)
			{
				renderer.materials = ToMaterialArray(newMaterial, renderer.materials.Length);
			}
			else if (!(component2 as Transform) && !(component2 as MeshFilter) && !(component2 as MeshRenderer) && !(component2 as SkinnedMeshRenderer))
			{
				UnityEngine.Object.Destroy(component2);
			}
		}
		foreach (Transform item2 in target.transform)
		{
			StripForPreview(item2, newMaterial, first: false);
		}
	}

	public static SandboxSpawnableInstance GetProp(GameObject from)
	{
		SandboxSpawnableInstance component = from.GetComponent<SandboxSpawnableInstance>();
		if ((bool)component)
		{
			return component;
		}
		SandboxPropPart component2 = from.GetComponent<SandboxPropPart>();
		if ((bool)component2)
		{
			return component2.parent;
		}
		return null;
	}

	public static void SetLayerDeep(Transform target, int layer)
	{
		target.gameObject.layer = layer;
		foreach (Transform item in target.transform)
		{
			SetLayerDeep(item, layer);
			if (item.TryGetComponent<OutdoorsChecker>(out var component))
			{
				component.CancelInvoke("SlowUpdate");
				component.enabled = false;
			}
		}
	}

	private static Material[] ToMaterialArray(Material material, int length)
	{
		return Enumerable.Repeat(material, length).ToArray();
	}

	public static Vector3 SnapPos(Vector3 pos)
	{
		return SnapPos(pos, Vector3.zero);
	}

	public static Vector3 SnapPos(Vector3 pos, Vector3 offset, float snapDensity = 0.5f)
	{
		Vector3 vector = pos;
		vector *= snapDensity;
		vector = new Vector3(Mathf.Round(vector.x), Mathf.Round(vector.y), Mathf.Round(vector.z));
		vector = new Vector3(Mathf.Round(vector.x), Mathf.Round(vector.y), Mathf.Round(vector.z));
		vector /= snapDensity;
		return vector + offset;
	}

	public static Vector3 SnapRotation(Vector3 rot)
	{
		float num = 45f;
		rot /= num;
		rot = new Vector3(Mathf.Round(rot.x), Mathf.Round(rot.y), Mathf.Round(rot.z));
		rot *= num;
		return rot;
	}

	public static Mesh GenerateProceduralMesh(Vector3 size, bool simple)
	{
		Vector3 zero = Vector3.zero;
		Vector3 vector = size;
		float num = 0.25f;
		float num2 = 1f;
		float num3 = size.x * size.y * size.z;
		if (num3 > 5000000f)
		{
			num *= 0.5f;
			num2 *= 2f;
		}
		if (num3 > 50000000f)
		{
			num *= 0.5f;
			num2 *= 2f;
		}
		Vector3 vector2 = size * num;
		List<Vector3> vertices = new List<Vector3>();
		List<int> tris = new List<int>();
		List<Vector2> uvs = new List<Vector2>();
		if (simple)
		{
			Vector3 vector3 = zero;
			Vector3 vector4 = new Vector3(vector.x, zero.y, zero.z);
			Vector3 vector5 = new Vector3(zero.x, vector.y, zero.z);
			Vector3 vector6 = new Vector3(vector.x, vector.y, zero.z);
			Vector3 vector7 = new Vector3(zero.x, zero.y, vector.z);
			Vector3 vector8 = new Vector3(vector.x, zero.y, vector.z);
			Vector3 vector9 = new Vector3(zero.x, vector.y, vector.z);
			Vector3 vector10 = vector;
			AddFaceToMesh(ref vertices, ref tris, ref uvs, new Vector3[4] { vector4, vector3, vector5, vector6 }, repeatUVs: true, Vector2.zero, new Vector2(vector.x, vector.y), 0.05f);
			AddFaceToMesh(ref vertices, ref tris, ref uvs, new Vector3[4] { vector8, vector4, vector6, vector10 }, repeatUVs: true, Vector2.zero, new Vector2(vector.z, vector.y), 0.05f);
			AddFaceToMesh(ref vertices, ref tris, ref uvs, new Vector3[4] { vector3, vector7, vector9, vector5 }, repeatUVs: true, Vector2.zero, new Vector2(vector.z, vector.y), 0.05f);
			AddFaceToMesh(ref vertices, ref tris, ref uvs, new Vector3[4] { vector7, vector8, vector10, vector9 }, repeatUVs: true, Vector2.zero, new Vector2(vector.x, vector.y), 0.05f);
			AddFaceToMesh(ref vertices, ref tris, ref uvs, new Vector3[4] { vector6, vector5, vector9, vector10 }, repeatUVs: true, Vector2.zero, new Vector2(vector.x, vector.z), 0.05f);
			AddFaceToMesh(ref vertices, ref tris, ref uvs, new Vector3[4] { vector8, vector7, vector3, vector4 }, repeatUVs: true, Vector2.zero, new Vector2(vector.x, vector.z), 0.05f);
		}
		else
		{
			float num4 = 1f / num;
			for (int i = 0; (float)i < vector2.x; i++)
			{
				for (int j = 0; (float)j < vector2.y; j++)
				{
					Vector3 vector11 = new Vector3(PolyClamp(zero.x, vector.x, i, num4), PolyClamp(zero.y, vector.y, j, num4), num4);
					Vector3 vector12 = zero + new Vector3(i, j, 0f) * num4;
					Vector3 vector13 = vector12 + new Vector3(vector11.x, 0f, 0f);
					Vector3 vector14 = vector12 + new Vector3(0f, vector11.y, 0f);
					Vector3 vector15 = vector12 + new Vector3(vector11.x, vector11.y, 0f);
					AddFaceToMesh(ref vertices, ref tris, ref uvs, new Vector3[4] { vector13, vector12, vector14, vector15 }, repeatUVs: true, new Vector2(i, j), new Vector2(vector11.x, vector11.y) / num4, num2);
					vector12 = new Vector3(zero.x, zero.y, vector.z) + new Vector3(i, j, 0f) * num4;
					vector13 = vector12 + new Vector3(vector11.x, 0f, 0f);
					vector14 = vector12 + new Vector3(0f, vector11.y, 0f);
					vector15 = vector12 + new Vector3(vector11.x, vector11.y, 0f);
					AddFaceToMesh(ref vertices, ref tris, ref uvs, new Vector3[4] { vector12, vector13, vector15, vector14 }, repeatUVs: true, new Vector2((float)i - vector11.x / num4, j), new Vector2(vector11.x, vector11.y) / num4, num2);
				}
				for (int k = 0; (float)k < vector2.z; k++)
				{
					Vector3 vector16 = new Vector3(PolyClamp(zero.x, vector.x, i, num4), num4, PolyClamp(zero.z, vector.z, k, num4));
					Vector3 vector17 = new Vector3(zero.x, vector.y, zero.z) + new Vector3(i, 0f, k) * num4;
					Vector3 vector18 = vector17 + new Vector3(vector16.x, 0f, 0f);
					Vector3 vector19 = vector17 + new Vector3(0f, 0f, vector16.z);
					Vector3 vector20 = vector17 + new Vector3(vector16.x, 0f, vector16.z);
					AddFaceToMesh(ref vertices, ref tris, ref uvs, new Vector3[4] { vector18, vector17, vector19, vector20 }, repeatUVs: true, new Vector2(i, k), new Vector2(vector16.x, vector16.z) / num4, num2);
					vector17 = zero + new Vector3(i, 0f, k) * num4;
					vector18 = vector17 + new Vector3(vector16.x, 0f, 0f);
					vector19 = vector17 + new Vector3(0f, 0f, vector16.z);
					vector20 = vector17 + new Vector3(vector16.x, 0f, vector16.z);
					AddFaceToMesh(ref vertices, ref tris, ref uvs, new Vector3[4] { vector17, vector18, vector20, vector19 }, repeatUVs: true, new Vector2((float)i - vector16.x / num4, k), new Vector2(vector16.x, vector16.z) / num4, num2);
				}
			}
			for (int l = 0; (float)l < vector2.z; l++)
			{
				for (int m = 0; (float)m < vector2.y; m++)
				{
					Vector3 vector21 = new Vector3(num4, PolyClamp(zero.y, vector.y, m, num4), PolyClamp(zero.z, vector.z, l, num4));
					Vector3 vector22 = zero + new Vector3(0f, m, l) * num4;
					Vector3 vector23 = vector22 + new Vector3(0f, 0f, vector21.z);
					Vector3 vector24 = vector22 + new Vector3(0f, vector21.y, 0f);
					Vector3 vector25 = vector22 + new Vector3(0f, vector21.y, vector21.z);
					AddFaceToMesh(ref vertices, ref tris, ref uvs, new Vector3[4] { vector22, vector23, vector25, vector24 }, repeatUVs: true, new Vector2((float)l - vector21.z / num4, m), new Vector2(vector21.z, vector21.y) / num4, num2);
					vector22 = new Vector3(vector.x, zero.y, zero.z) + new Vector3(0f, m, l) * num4;
					vector23 = vector22 + new Vector3(0f, 0f, vector21.z);
					vector24 = vector22 + new Vector3(0f, vector21.y, 0f);
					vector25 = vector22 + new Vector3(0f, vector21.y, vector21.z);
					AddFaceToMesh(ref vertices, ref tris, ref uvs, new Vector3[4] { vector23, vector22, vector24, vector25 }, repeatUVs: true, new Vector2(l, m), new Vector2(vector21.z, vector21.y) / num4, num2);
				}
			}
		}
		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = tris.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.Optimize();
		mesh.RecalculateNormals();
		mesh.RecalculateTangents();
		mesh.RecalculateBounds();
		return mesh;
	}

	public static void AddFaceToMesh(ref List<Vector3> vertices, ref List<int> tris, ref List<Vector2> uvs, Vector3[] quadPoints, bool repeatUVs = false, Vector2? uvCords = null, Vector2? uvSizeMod = null, float uvScaleOverride = 1f)
	{
		if (quadPoints.Length < 4)
		{
			throw new Exception("Missing quad points");
		}
		Vector2 vector = Vector2.zero;
		if (uvCords.HasValue)
		{
			vector = uvCords.Value;
		}
		Vector2 vector2 = Vector2.one;
		if (uvSizeMod.HasValue)
		{
			vector2 = uvSizeMod.Value;
		}
		int count = vertices.Count;
		vertices.AddRange(new Vector3[4]
		{
			quadPoints[0],
			quadPoints[1],
			quadPoints[2],
			quadPoints[3]
		});
		uvs.AddRange(new Vector2[4]
		{
			(vector + new Vector2(vector2.x, 0f)) * uvScaleOverride,
			vector * uvScaleOverride,
			(vector + new Vector2(0f, vector2.y)) * uvScaleOverride,
			(vector + vector2) * uvScaleOverride
		});
		tris.AddRange(new int[6]
		{
			count,
			count + 1,
			count + 2,
			count + 2,
			count + 3,
			count
		});
	}

	public static float PolyClamp(float a, float b, float i, float units)
	{
		float num = a + i * units;
		float num2 = num + units;
		if (num2 > b)
		{
			return num2 - num - (num2 - b);
		}
		return units;
	}

	public static void SmallerBigger(Vector3 a, Vector3 b, out Vector3 smaller, out Vector3 bigger)
	{
		smaller = new Vector3((a.x > b.x) ? b.x : a.x, (a.y > b.y) ? b.y : a.y, (a.z > b.z) ? b.z : a.z);
		bigger = new Vector3((a.x > b.x) ? a.x : b.x, (a.y > b.y) ? a.y : b.y, (a.z > b.z) ? a.z : b.z);
	}

	public static GameObject CreateFinalBlock(SpawnableObject proceduralTemplate, Vector3 position, Vector3 size, bool liquid = false)
	{
		Debug.Log($"Creating block {size}");
		GameObject gameObject = UnityEngine.Object.Instantiate(proceduralTemplate.gameObject);
		gameObject.transform.position = position;
		BrushBlock component = gameObject.GetComponent<BrushBlock>();
		component.sourceObject = proceduralTemplate;
		component.DataSize = size;
		Mesh mesh = GenerateProceduralMesh(size, liquid);
		SandboxProp component2 = gameObject.GetComponent<SandboxProp>();
		component2.sourceObject = proceduralTemplate;
		gameObject.GetComponent<MeshFilter>().mesh = mesh;
		BoxCollider boxCollider = (component.OverrideCollider ? component.OverrideCollider : gameObject.GetComponent<BoxCollider>());
		boxCollider.size = size;
		boxCollider.center = boxCollider.size / 2f;
		if (liquid)
		{
			GameObject gameObject2 = new GameObject("LiquidTrigger");
			gameObject2.layer = LayerMask.NameToLayer("SandboxGrabbable");
			gameObject2.transform.parent = gameObject.transform;
			gameObject2.transform.localPosition = Vector3.zero;
			gameObject2.transform.localRotation = Quaternion.identity;
			gameObject2.transform.localScale = Vector3.one;
			BoxCollider boxCollider2 = gameObject2.AddComponent<BoxCollider>();
			boxCollider2.isTrigger = true;
			boxCollider2.size = size;
			boxCollider2.center = boxCollider.size / 2f;
			gameObject2.AddComponent<SandboxPropPart>().parent = component2;
		}
		return gameObject;
	}
}
