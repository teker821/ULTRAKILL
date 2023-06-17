using ULTRAKILL.Cheats;
using UnityEngine;

namespace Sandbox.Arm;

public class BuildMode : ArmModeWithHeldPreview
{
	private float tickDelay;

	private bool firstBrushPositionSet;

	private Vector3 firstBlockPos;

	private Vector3 secondBlockPos;

	private Vector3 previousSecondBlockPos;

	private Vector3 brushOffset = Vector3.one;

	private GameObject worldPreviewObject;

	private GameObject pointAIndicatorObject;

	private GameObject pointBIndicatorObject;

	private new static readonly int Punch = Animator.StringToHash("Punch");

	public override string Name => "Build";

	public override void SetPreview(SpawnableObject obj)
	{
		base.SetPreview(obj);
		SetupBlockCreator(obj);
	}

	private void SetupBlockCreator(SpawnableObject template)
	{
		firstBrushPositionSet = false;
		if ((bool)pointAIndicatorObject)
		{
			Object.Destroy(pointAIndicatorObject);
		}
		if ((bool)worldPreviewObject)
		{
			Object.Destroy(worldPreviewObject);
		}
		pointAIndicatorObject = Object.Instantiate(hostArm.axisPoint);
		if ((bool)pointBIndicatorObject)
		{
			Object.Destroy(pointBIndicatorObject);
		}
	}

	public override void Update()
	{
		base.Update();
		if (tickDelay > 0f)
		{
			tickDelay = Mathf.MoveTowards(tickDelay, 0f, Time.deltaTime);
		}
		Transform transform = MonoSingleton<CameraController>.Instance.transform;
		RaycastHit hitInfo;
		bool active = Physics.Raycast(transform.position, transform.forward, out hitInfo, 75f, hostArm.raycastLayers);
		if (!firstBrushPositionSet)
		{
			pointAIndicatorObject.SetActive(active);
			pointAIndicatorObject.transform.position = CalculatePropPosition(hitInfo);
			return;
		}
		RaycastHit hitInfo2;
		bool num = Physics.Raycast(transform.position, transform.forward, out hitInfo2, 5f, hostArm.raycastLayers);
		Vector3 zero = Vector3.zero;
		zero = ((!num) ? (transform.position + transform.forward * 4.5f) : (hitInfo2.point + new Vector3(0f, 0f, 0f)));
		zero = SandboxUtils.SnapPos(zero, brushOffset, ULTRAKILL.Cheats.Snapping.SnappingEnabled ? 0.5f : 7.5f);
		pointBIndicatorObject.SetActive(value: true);
		pointBIndicatorObject.transform.position = zero;
		if (Mathf.Abs(firstBlockPos.x - zero.x) >= 1f)
		{
			secondBlockPos.x = zero.x;
		}
		if (Mathf.Abs(firstBlockPos.y - zero.y) >= 1f)
		{
			secondBlockPos.y = zero.y;
		}
		if (Mathf.Abs(firstBlockPos.z - zero.z) >= 1f)
		{
			secondBlockPos.z = zero.z;
		}
		if (secondBlockPos != previousSecondBlockPos)
		{
			if (tickDelay == 0f)
			{
				hostArm.tickSound.pitch = Random.Range(0.74f, 0.76f);
				hostArm.tickSound.Play();
				tickDelay = 0.05f;
			}
			previousSecondBlockPos = secondBlockPos;
			SandboxUtils.SmallerBigger(firstBlockPos, secondBlockPos, out var smaller, out var bigger);
			Vector3 size = bigger - smaller;
			worldPreviewObject.GetComponent<MeshFilter>().mesh = SandboxUtils.GenerateProceduralMesh(size, simple: true);
			worldPreviewObject.transform.position = smaller;
		}
		worldPreviewObject.SetActive(value: true);
	}

	private Vector3 CalculatePropPosition(RaycastHit hit)
	{
		if (!ULTRAKILL.Cheats.Snapping.SnappingEnabled)
		{
			brushOffset = Vector3.zero;
		}
		Vector3 offset = Vector3.zero;
		if ((bool)hit.transform && (bool)SandboxUtils.GetProp(hit.collider.gameObject))
		{
			offset = -new Vector3(0f, SandboxUtils.SnapPos(hit.transform.position).y - hit.transform.position.y, 0f);
		}
		Vector3 vector = (ULTRAKILL.Cheats.Snapping.SnappingEnabled ? SandboxUtils.SnapPos(hit.point, offset) : hit.point);
		float distanceToPoint = new Plane(hit.normal, vector).GetDistanceToPoint(hit.point);
		vector += hit.normal * distanceToPoint;
		if ((bool)hit.transform && !SandboxUtils.GetProp(hit.collider.gameObject))
		{
			offset = hit.normal * distanceToPoint;
		}
		brushOffset = offset;
		return vector;
	}

	public override void OnPrimaryDown()
	{
		base.OnPrimaryDown();
		if (!currentObject)
		{
			return;
		}
		hostArm.jabSound.Play();
		hostArm.animator.SetTrigger(Punch);
		if (!firstBrushPositionSet && hostArm.hitSomething)
		{
			firstBlockPos = CalculatePropPosition(hostArm.hit);
			secondBlockPos = firstBlockPos + Vector3.one / 0.5f;
			previousSecondBlockPos = secondBlockPos;
			firstBrushPositionSet = true;
			pointBIndicatorObject = Object.Instantiate(hostArm.axisPoint);
			CreateWorldPreview();
			return;
		}
		hostArm.tickSound.Play();
		SandboxUtils.SmallerBigger(firstBlockPos, secondBlockPos, out var smaller, out var bigger);
		Vector3 size = bigger - smaller;
		GameObject gameObject = SandboxUtils.CreateFinalBlock(currentObject, smaller, size, currentObject.isWater);
		if (gameObject.TryGetComponent<BrushBlock>(out var component))
		{
			SandboxSpawnableInstance component2 = gameObject.GetComponent<SandboxSpawnableInstance>();
			component2.frozen = !SpawnPhysics.PhysicsDynamic;
			if (!SpawnPhysics.PhysicsDynamic)
			{
				MonoSingleton<SandboxNavmesh>.Instance.MarkAsDirty(component2);
			}
			if (component.TryGetComponent<Rigidbody>(out var component3))
			{
				component3.isKinematic = !SpawnPhysics.PhysicsDynamic;
			}
		}
		firstBrushPositionSet = false;
		SetupBlockCreator(currentObject);
		MonoSingleton<PresenceController>.Instance.AddToStatInt("sandbox_built_brushes", 1);
	}

	private void CreateWorldPreview()
	{
		worldPreviewObject = new GameObject("World Preview");
		worldPreviewObject.AddComponent<MeshFilter>();
		worldPreviewObject.AddComponent<MeshRenderer>().material = hostArm.previewMaterial;
	}

	public override void OnDisable()
	{
		base.OnDisable();
		if ((bool)worldPreviewObject)
		{
			worldPreviewObject.SetActive(value: false);
		}
		if ((bool)pointAIndicatorObject)
		{
			pointAIndicatorObject.SetActive(value: false);
		}
		if ((bool)pointBIndicatorObject)
		{
			pointBIndicatorObject.SetActive(value: false);
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)worldPreviewObject)
		{
			Object.Destroy(worldPreviewObject);
		}
		if ((bool)pointAIndicatorObject)
		{
			Object.Destroy(pointAIndicatorObject);
		}
		if ((bool)pointBIndicatorObject)
		{
			Object.Destroy(pointBIndicatorObject);
		}
	}
}
