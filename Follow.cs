using UnityEngine;

public class Follow : MonoBehaviour
{
	public float speed;

	public Transform target;

	public bool mimicPosition = true;

	public bool applyPositionLocally;

	public bool followX = true;

	public bool followY = true;

	public bool followZ = true;

	public bool mimicRotation;

	public bool applyRotationLocally;

	public bool rotX = true;

	public bool rotY = true;

	public bool rotZ = true;

	private bool followingPlayer;

	public Collider[] restrictToColliderBounds;

	private Bounds area;

	public bool destroyIfNoTarget;

	private void Awake()
	{
		if (restrictToColliderBounds != null && restrictToColliderBounds.Length != 0)
		{
			for (int i = 0; i < restrictToColliderBounds.Length; i++)
			{
				if (!(restrictToColliderBounds[i] == null))
				{
					_ = area;
					if (area.size == Vector3.zero)
					{
						area = restrictToColliderBounds[i].bounds;
					}
					else
					{
						area.Encapsulate(restrictToColliderBounds[i].bounds);
					}
				}
			}
		}
		if (!(target != null))
		{
			target = MonoSingleton<NewMovement>.Instance.transform;
			followingPlayer = true;
		}
	}

	private void Update()
	{
		if (!target)
		{
			if (destroyIfNoTarget)
			{
				Object.Destroy(base.gameObject);
			}
			return;
		}
		if (mimicRotation)
		{
			Vector3 eulerAngles = base.transform.eulerAngles;
			if (followingPlayer)
			{
				if (rotX)
				{
					eulerAngles.x = 0f - MonoSingleton<CameraController>.Instance.rotationX;
				}
			}
			else if (rotX)
			{
				eulerAngles.x = target.eulerAngles.x;
			}
			if (rotY)
			{
				eulerAngles.y = target.eulerAngles.y;
			}
			if (rotZ)
			{
				eulerAngles.z = target.eulerAngles.z;
			}
			if (applyRotationLocally)
			{
				base.transform.localEulerAngles = eulerAngles;
			}
			else
			{
				base.transform.eulerAngles = eulerAngles;
			}
		}
		if (!mimicPosition)
		{
			return;
		}
		Vector3 vector = new Vector3(followX ? target.position.x : base.transform.position.x, followY ? target.position.y : base.transform.position.y, followZ ? target.position.z : base.transform.position.z);
		if (speed == 0f)
		{
			if (restrictToColliderBounds != null && restrictToColliderBounds.Length != 0)
			{
				base.transform.position = area.ClosestPoint(vector);
			}
			else if (applyPositionLocally)
			{
				base.transform.localPosition = vector;
			}
			else
			{
				base.transform.position = vector;
			}
			return;
		}
		float maxDistanceDelta = speed * Time.deltaTime;
		if (applyPositionLocally)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, vector, maxDistanceDelta);
		}
		else
		{
			base.transform.position = Vector3.MoveTowards(base.transform.position, vector, maxDistanceDelta);
		}
		if (restrictToColliderBounds != null && restrictToColliderBounds.Length != 0)
		{
			base.transform.position = area.ClosestPoint(base.transform.position);
		}
	}
}
