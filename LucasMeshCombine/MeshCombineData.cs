using UnityEngine;

namespace LucasMeshCombine;

public class MeshCombineData
{
	public readonly GameObject Parent;

	public readonly MeshRenderer MeshRenderer;

	public readonly MeshFilter MeshFilter;

	public readonly Texture2D Texture;

	public readonly int SubMeshIndex;

	public MeshCombineData(GameObject parent, MeshRenderer meshRenderer, MeshFilter meshFilter, Texture2D texture, int subMeshIndex)
	{
		Parent = parent;
		MeshRenderer = meshRenderer;
		MeshFilter = meshFilter;
		Texture = texture;
		SubMeshIndex = subMeshIndex;
	}
}
