using Sandbox;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class SandboxNavmesh : MonoSingleton<SandboxNavmesh>
{
	[SerializeField]
	private NavMeshSurface surface;

	public bool isDirty;

	public UnityAction navmeshBuilt;

	public void MarkAsDirty(SandboxSpawnableInstance instance)
	{
		if (!isDirty && (!instance || (instance.frozen && (!(instance.sourceObject != null) || !instance.sourceObject.isWater))))
		{
			MonoSingleton<SandboxHud>.Instance.NavmeshDirty();
			isDirty = true;
			MonoSingleton<CheatsManager>.Instance.RenderCheatsInfo();
		}
	}

	public void Rebake()
	{
		surface.BuildNavMesh();
		Debug.Log("Navmesh built");
		isDirty = false;
		MonoSingleton<SandboxHud>.Instance.HideNavmeshNotice();
		if (navmeshBuilt != null)
		{
			navmeshBuilt();
		}
		MonoSingleton<CheatsManager>.Instance.RenderCheatsInfo();
	}
}
