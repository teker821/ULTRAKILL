using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LucasMeshCombine;

public class CombineMeshes : MonoBehaviour
{
	private void Awake()
	{
		List<MeshCombineData> list = new List<MeshCombineData>();
		MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer meshRenderer in componentsInChildren)
		{
			if (!meshRenderer.gameObject.isStatic || MeshCombineManager.Instance.ProcessedMeshRenderers.Contains(meshRenderer) || !meshRenderer.gameObject.TryGetComponent<MeshFilter>(out var component) || meshRenderer.sharedMaterials.Length != component.sharedMesh.subMeshCount)
			{
				continue;
			}
			for (int j = 0; j < component.sharedMesh.subMeshCount; j++)
			{
				Material material = meshRenderer.sharedMaterials[j];
				if (MeshCombineManager.Instance.AllowedShadersToBatch.Contains(material.shader))
				{
					list.Add(new MeshCombineData(base.gameObject, meshRenderer, component, (Texture2D)material.mainTexture, j));
				}
			}
		}
		MeshCombineManager.Instance.AddCombineDatas(list);
	}
}
