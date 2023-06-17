using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class CameraFrustumTargeter : MonoSingleton<CameraFrustumTargeter>
{
	private const int MaxPotentialTargets = 256;

	[SerializeField]
	private RectTransform crosshair;

	[SerializeField]
	private LayerMask mask;

	[SerializeField]
	private LayerMask occlusionMask;

	[SerializeField]
	private float maximumRange = 1000f;

	public float maxHorAim = 1f;

	private RaycastHit[] occluders;

	private Plane[] frustum;

	private Vector3[] corners;

	private Bounds bounds;

	private Collider[] targets;

	private Camera camera;

	public Collider CurrentTarget { get; private set; }

	public bool IsAutoAimed { get; private set; }

	public static bool IsEnabled
	{
		get
		{
			return MonoSingleton<PrefsManager>.Instance.GetBool("autoAim");
		}
		set
		{
			MonoSingleton<PrefsManager>.Instance.SetBool("autoAim", value);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		frustum = new Plane[6];
		corners = new Vector3[4];
		targets = new Collider[256];
		camera = GetComponent<Camera>();
		occluders = new RaycastHit[16];
	}

	private bool RaycastFromViewportCenter(out RaycastHit hit)
	{
		return Physics.Raycast(camera.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out hit, maximumRange, mask.value);
	}

	private void CalculateFrustumInformation()
	{
		camera.CalculateFrustumCorners(new Rect(0f, 0f, 1f, 1f), maximumRange, Camera.MonoOrStereoscopicEye.Mono, corners);
		bounds = GeometryUtility.CalculateBounds(corners, camera.transform.localToWorldMatrix);
		bounds.size = new Vector3(bounds.size.x, bounds.size.y, maximumRange);
		bounds.center = base.transform.position;
	}

	private void Update()
	{
		if (!IsEnabled || maxHorAim == 0f)
		{
			CurrentTarget = null;
			IsAutoAimed = false;
			return;
		}
		if (RaycastFromViewportCenter(out var hit) && !Physics.Raycast(camera.ViewportPointToRay(new Vector2(0.5f, 0.5f)), hit.distance, occlusionMask) && (!hit.collider.isTrigger || hit.collider.gameObject.layer == 22))
		{
			CurrentTarget = hit.collider;
			IsAutoAimed = false;
			return;
		}
		CalculateFrustumInformation();
		int num = Physics.OverlapBoxNonAlloc(bounds.center, bounds.extents, targets, base.transform.rotation, mask.value);
		float num2 = float.PositiveInfinity;
		Collider currentTarget = null;
		for (int i = 0; i < num; i++)
		{
			Vector3 position = base.transform.position;
			Vector3 direction = targets[i].bounds.center - position;
			if ((targets[i].gameObject.layer != 22 && targets[i].isTrigger) || (targets[i].gameObject.layer == 22 && (!targets[i].TryGetComponent<HookPoint>(out var component) || !component.active)) || (targets[i].gameObject.layer == 10 && !targets[i].TryGetComponent<Coin>(out var _)) || (targets[i].gameObject.layer == 14 && !targets[i].TryGetComponent<Grenade>(out var _)))
			{
				continue;
			}
			int num3 = Physics.RaycastNonAlloc(position, direction, occluders, direction.magnitude, occlusionMask.value, QueryTriggerInteraction.Ignore);
			int num4 = 0;
			while (true)
			{
				if (num4 < num3)
				{
					if (!(occluders[num4].collider == null))
					{
						break;
					}
					num4++;
					continue;
				}
				Vector3 a = camera.WorldToViewportPoint(targets[i].bounds.center);
				float num5 = Vector3.Distance(a, new Vector2(0.5f, 0.5f));
				if (!(a.x > 0.5f + maxHorAim / 2f) && !(a.x < 0.5f - maxHorAim / 2f) && !(a.y > 0.5f + maxHorAim / 2f) && !(a.y < 0.5f - maxHorAim / 2f) && !(a.z < 0f) && num5 < num2)
				{
					num2 = num5;
					currentTarget = targets[i];
				}
				break;
			}
		}
		CurrentTarget = currentTarget;
		IsAutoAimed = true;
	}

	private void LateUpdate()
	{
		if (CurrentTarget == null || !IsAutoAimed)
		{
			crosshair.anchoredPosition = Vector2.zero;
			return;
		}
		Vector2 a = (Vector2)camera.WorldToViewportPoint(CurrentTarget.bounds.center) - new Vector2(0.5f, 0.5f);
		Vector2 referenceResolution = crosshair.GetParentCanvas().GetComponent<CanvasScaler>().referenceResolution;
		crosshair.anchoredPosition = Vector2.Scale(a, referenceResolution);
	}
}
