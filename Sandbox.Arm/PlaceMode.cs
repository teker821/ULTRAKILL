using ULTRAKILL.Cheats;
using UnityEngine;

namespace Sandbox.Arm;

public class PlaceMode : ArmModeWithHeldPreview
{
	private GameObject worldPreviewObject;

	public override string Name => "Place";

	public override bool Raycast => true;

	public override void SetPreview(SpawnableObject obj)
	{
		base.SetPreview(obj);
		if (worldPreviewObject != null)
		{
			Object.Destroy(worldPreviewObject);
		}
		CreateWorldPreview(obj);
	}

	private void CreateWorldPreview(SpawnableObject obj)
	{
		if (obj.gameObject.TryGetComponent<MeshFilter>(out var component) && obj.gameObject.TryGetComponent<SandboxProp>(out var component2) && !component2.forceFullWorldPreview)
		{
			Mesh sharedMesh = component.sharedMesh;
			CreateWorldPreview();
			worldPreviewObject.transform.localRotation = obj.gameObject.transform.localRotation;
			worldPreviewObject.GetComponent<MeshFilter>().mesh = sharedMesh;
			worldPreviewObject.GetComponent<MeshRenderer>().material = hostArm.previewMaterial;
		}
		else
		{
			worldPreviewObject = Object.Instantiate(obj.gameObject);
			SandboxUtils.StripForPreview(worldPreviewObject.transform, hostArm.previewMaterial);
		}
	}

	private void CreateWorldPreview()
	{
		worldPreviewObject = new GameObject("World Preview");
		worldPreviewObject.AddComponent<MeshFilter>();
		worldPreviewObject.AddComponent<MeshRenderer>().material = hostArm.previewMaterial;
	}

	public override void Update()
	{
		base.Update();
		if ((bool)worldPreviewObject)
		{
			worldPreviewObject.SetActive(hostArm.hitSomething);
			if (hostArm.hitSomething)
			{
				worldPreviewObject.transform.position = CalculatePropPosition(hostArm.hit);
				Vector3 normal = ((currentObject.spawnableObjectType == SpawnableObject.SpawnableObjectDataType.Enemy) ? Vector3.up : hostArm.hit.normal);
				worldPreviewObject.transform.rotation = CalculatePropRotation(normal, currentObject.gameObject.transform.rotation);
			}
		}
	}

	private Quaternion CalculatePropRotation(Vector3 normal, Quaternion baseRotation)
	{
		float num = MonoSingleton<CameraController>.Instance.rotationY;
		if (ULTRAKILL.Cheats.Snapping.SnappingEnabled)
		{
			num /= 90f;
			num = Mathf.Round(num);
			num *= 90f;
		}
		return Quaternion.FromToRotation(Vector3.up, normal) * Quaternion.AngleAxis(num, Vector3.up) * baseRotation;
	}

	private Vector3 CalculatePropPosition(RaycastHit hit)
	{
		if (!ULTRAKILL.Cheats.Snapping.SnappingEnabled)
		{
			if ((bool)currentObject && currentObject.spawnOffset != 0f)
			{
				return hit.point + hit.normal * currentObject.spawnOffset;
			}
			return hit.point;
		}
		Vector3 offset = Vector3.zero;
		if ((bool)hit.transform && (bool)SandboxUtils.GetProp(hit.collider.gameObject))
		{
			offset = -new Vector3(0f, SandboxUtils.SnapPos(hit.transform.position).y - hit.transform.position.y, 0f);
		}
		Vector3 vector = SandboxUtils.SnapPos(hit.point, offset);
		float distanceToPoint = new Plane(hit.normal, vector).GetDistanceToPoint(hit.point);
		vector += hit.normal * distanceToPoint;
		if ((bool)hit.transform && !SandboxUtils.GetProp(hit.collider.gameObject))
		{
			offset = hit.normal * distanceToPoint;
		}
		if ((bool)currentObject && currentObject.spawnOffset != 0f)
		{
			vector += hit.normal * currentObject.spawnOffset;
		}
		return vector;
	}

	public override void OnPrimaryDown()
	{
		base.OnPrimaryDown();
		if (!currentObject)
		{
			return;
		}
		Vector3 normal = ((currentObject.spawnableObjectType == SpawnableObject.SpawnableObjectDataType.Enemy) ? Vector3.up : hostArm.hit.normal);
		GameObject gameObject = Object.Instantiate(currentObject.gameObject, CalculatePropPosition(hostArm.hit), CalculatePropRotation(normal, currentObject.gameObject.transform.localRotation), hostArm.GetGoreZone().transform);
		if (gameObject.TryGetComponent<SandboxProp>(out var component))
		{
			component.sourceObject = currentObject;
		}
		bool flag = currentObject.spawnableObjectType == SpawnableObject.SpawnableObjectDataType.Enemy;
		if (flag)
		{
			gameObject.AddComponent<SandboxEnemy>().sourceObject = currentObject;
			MonoSingleton<PresenceController>.Instance.AddToStatInt("sandbox_spawned_enemies", 1);
		}
		else
		{
			MonoSingleton<PresenceController>.Instance.AddToStatInt("sandbox_spawned_props", 1);
		}
		if (gameObject.TryGetComponent<SandboxSpawnableInstance>(out var component2))
		{
			component2.frozen = !flag && !SpawnPhysics.PhysicsDynamic;
		}
		gameObject.SetActive(value: true);
		if (currentObject.spawnableObjectType != SpawnableObject.SpawnableObjectDataType.Enemy && gameObject.TryGetComponent<Rigidbody>(out var component3))
		{
			component3.isKinematic = !SpawnPhysics.PhysicsDynamic;
			if (currentObject.spawnableType == SpawnableType.Prop && !SpawnPhysics.PhysicsDynamic)
			{
				MonoSingleton<SandboxNavmesh>.Instance.MarkAsDirty(component);
			}
		}
	}

	public override void OnEnable(SandboxArm arm)
	{
		base.OnEnable(arm);
		if (worldPreviewObject != null)
		{
			worldPreviewObject.SetActive(value: true);
		}
	}

	public override void OnDisable()
	{
		base.OnDisable();
		if (worldPreviewObject != null)
		{
			worldPreviewObject.SetActive(value: false);
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (worldPreviewObject != null)
		{
			Object.Destroy(worldPreviewObject);
		}
	}
}
