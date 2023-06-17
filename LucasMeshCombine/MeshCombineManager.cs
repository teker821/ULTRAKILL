using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace LucasMeshCombine;

public class MeshCombineManager : MonoBehaviour
{
	[SerializeField]
	private Shader[] allowedShadersToBatch;

	[SerializeField]
	private Shader atlasedShader;

	[SerializeField]
	private Texture2D textureAtlas;

	private readonly Dictionary<Mesh, Vector4[]> oldMeshUvs = new Dictionary<Mesh, Vector4[]>();

	private readonly List<List<MeshCombineData>> combineSets = new List<List<MeshCombineData>>();

	private readonly List<Texture2D> textures = new List<Texture2D>();

	private readonly HashSet<MeshRenderer> processedMeshRenderers = new HashSet<MeshRenderer>();

	private readonly List<Mesh> createdMeshes = new List<Mesh>();

	private readonly List<GameObject> createdObjects = new List<GameObject>();

	private Material combinedMeshMaterial;

	public static MeshCombineManager Instance { get; private set; }

	public Shader[] AllowedShadersToBatch => allowedShadersToBatch;

	public HashSet<MeshRenderer> ProcessedMeshRenderers => processedMeshRenderers;

	public void AddCombineDatas(List<MeshCombineData> meshCombineDatas)
	{
		combineSets.Add(meshCombineDatas);
	}

	private void Awake()
	{
		if (Instance != null)
		{
			Object.Destroy(base.gameObject);
		}
		Instance = this;
		processedMeshRenderers.Clear();
	}

	private void Start()
	{
		textureAtlas = new Texture2D(1, 1)
		{
			filterMode = FilterMode.Point
		};
		if (combinedMeshMaterial == null)
		{
			combinedMeshMaterial = new Material(atlasedShader)
			{
				name = "Combined Mesh Material",
				hideFlags = HideFlags.HideAndDontSave,
				mainTexture = textureAtlas
			};
		}
		foreach (List<MeshCombineData> combineSet in combineSets)
		{
			for (int i = 0; i < combineSet.Count; i++)
			{
				Texture2D texture = combineSet[i].Texture;
				if (!textures.Contains(texture))
				{
					if (!texture.isReadable)
					{
						Debug.LogWarning("Mesh Combine: Texture not readable " + texture.name, texture);
					}
					else
					{
						textures.Add(texture);
					}
				}
			}
		}
		Rect[] array = textureAtlas.PackTextures(textures.ToArray(), 0, 8192);
		List<Vector4> list = new List<Vector4>();
		foreach (List<MeshCombineData> combineSet2 in combineSets)
		{
			if (combineSet2.Count == 0)
			{
				continue;
			}
			List<CombineInstance> list2 = new List<CombineInstance>(combineSet2.Count);
			for (int j = 0; j < combineSet2.Count; j++)
			{
				MeshCombineData meshCombineData = combineSet2[j];
				Mesh mesh = meshCombineData.MeshFilter.mesh;
				if (mesh.name.StartsWith("Combined Mesh"))
				{
					continue;
				}
				if (!mesh.isReadable)
				{
					Debug.LogWarning("Mesh Combine: Mesh isn't readable! Couldn't combine mesh " + mesh.name + " on GameObject " + meshCombineData.MeshFilter.gameObject.name, meshCombineData.MeshFilter.gameObject);
					continue;
				}
				int num = textures.IndexOf(meshCombineData.Texture);
				if (num < 0)
				{
					continue;
				}
				Rect rect = array[num];
				SubMeshDescriptor subMesh = mesh.GetSubMesh(meshCombineData.SubMeshIndex);
				Vector4[] array2 = new Vector4[mesh.vertexCount];
				mesh.GetUVs(3, list);
				oldMeshUvs[mesh] = array2.ToArray();
				for (int k = 0; k < array2.Length; k++)
				{
					if (k >= subMesh.firstVertex && k < subMesh.firstVertex + subMesh.vertexCount)
					{
						array2[k] = new Vector4(rect.xMin, rect.yMin, rect.xMax, rect.yMax);
					}
					else if (list.Count > k)
					{
						array2[k] = list[k];
					}
				}
				mesh.SetUVs(3, array2);
				mesh.UploadMeshData(markNoLongerReadable: false);
				list2.Add(new CombineInstance
				{
					mesh = mesh,
					subMeshIndex = meshCombineData.SubMeshIndex,
					transform = meshCombineData.MeshRenderer.localToWorldMatrix
				});
				meshCombineData.MeshRenderer.enabled = false;
			}
			if (list2.Count == 0)
			{
				Debug.LogWarning("Mesh Combine: Mesh combination on GameObject " + combineSet2[0].Parent.name + " is not effective (zero mesh combinations). You may want to remove it, or turn off static batching.", combineSet2[0].Parent);
				for (int l = 0; l < combineSet2.Count; l++)
				{
					MeshCombineData meshCombineData2 = combineSet2[l];
					Mesh mesh2 = meshCombineData2.MeshFilter.mesh;
					if (!mesh2.name.StartsWith("Combined Mesh") && mesh2.isReadable && textures.IndexOf(meshCombineData2.Texture) >= 0)
					{
						meshCombineData2.MeshRenderer.enabled = true;
						mesh2.SetUVs(3, oldMeshUvs[mesh2]);
					}
				}
				continue;
			}
			Mesh mesh3 = new Mesh();
			mesh3.CombineMeshes(list2.ToArray());
			mesh3.Optimize();
			GameObject gameObject = new GameObject("Combined Mesh");
			gameObject.transform.parent = combineSet2[0].Parent.transform;
			gameObject.isStatic = true;
			gameObject.layer = combineSet2[0].MeshRenderer.gameObject.layer;
			createdMeshes.Add(mesh3);
			createdObjects.Add(gameObject);
			MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
			meshRenderer.sharedMaterial = combinedMeshMaterial;
			meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
			meshRenderer.receiveShadows = false;
			meshRenderer.lightProbeUsage = LightProbeUsage.Off;
			meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
			gameObject.AddComponent<MeshFilter>().sharedMesh = mesh3;
			if (list2.Count <= 1)
			{
				Debug.LogWarning("Mesh Combine: Mesh combination on GameObject " + combineSet2[0].Parent.name + " is not effective (less than two mesh combinations). You may want to remove it.", combineSet2[0].Parent);
			}
		}
		textures.Clear();
		combineSets.Clear();
		processedMeshRenderers.Clear();
	}

	private void OnDestroy()
	{
		foreach (Mesh createdMesh in createdMeshes)
		{
			Object.Destroy(createdMesh);
		}
		foreach (GameObject createdObject in createdObjects)
		{
			Object.Destroy(createdObject);
		}
		foreach (MeshRenderer processedMeshRenderer in processedMeshRenderers)
		{
			if (!(processedMeshRenderer == null))
			{
				processedMeshRenderer.enabled = true;
			}
		}
		Instance = null;
		Object.Destroy(textureAtlas);
		Object.Destroy(combinedMeshMaterial);
		Debug.Log("Destroying mesh combine manager");
	}
}
