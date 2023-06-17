using UnityEngine;

public class AlwaysLookAtCamera : MonoBehaviour
{
	private Transform cam;

	public bool faceScreenInsteadOfCamera;

	public float speed;

	public bool easeIn;

	public Transform overrideTarget;

	public float maxAngle;

	[Space]
	public bool useXAxis = true;

	public bool useYAxis = true;

	public bool useZAxis = true;

	[Space]
	public Vector3 rotationOffset;

	[Space]
	public float maxXAxisFromParent;

	public float maxYAxisFromParent;

	public float maxZAxisFromParent;

	private void Start()
	{
		if ((bool)overrideTarget)
		{
			cam = overrideTarget;
		}
		else
		{
			cam = MonoSingleton<CameraController>.Instance.gameObject.transform;
		}
		SlowUpdate();
	}

	private void SlowUpdate()
	{
		Invoke("SlowUpdate", 0.5f);
		if (!overrideTarget && !cam.gameObject.activeInHierarchy)
		{
			cam = MonoSingleton<CameraController>.Instance.gameObject.transform;
		}
	}

	private void LateUpdate()
	{
		if (speed == 0f && useXAxis && useYAxis && useZAxis)
		{
			if (faceScreenInsteadOfCamera)
			{
				base.transform.rotation = cam.rotation;
				base.transform.Rotate(Vector3.up * 180f, Space.Self);
			}
			else
			{
				base.transform.LookAt(cam);
			}
		}
		else
		{
			Vector3 position = cam.position;
			if (!useXAxis)
			{
				position.x = base.transform.position.x;
			}
			if (!useYAxis)
			{
				position.y = base.transform.position.y;
			}
			if (!useZAxis)
			{
				position.z = base.transform.position.z;
			}
			Quaternion quaternion = Quaternion.LookRotation(position - base.transform.position);
			if (maxAngle != 0f && Quaternion.Angle(base.transform.rotation, quaternion) > maxAngle)
			{
				return;
			}
			if (speed == 0f)
			{
				base.transform.rotation = quaternion;
			}
			if (easeIn)
			{
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, quaternion, Time.deltaTime * speed * Quaternion.Angle(base.transform.rotation, quaternion));
			}
			else
			{
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, quaternion, Time.deltaTime * speed);
			}
		}
		if (maxXAxisFromParent != 0f)
		{
			base.transform.localRotation = Quaternion.Euler(Mathf.Clamp(base.transform.localRotation.eulerAngles.x, 0f - maxXAxisFromParent, maxXAxisFromParent), base.transform.localRotation.eulerAngles.y, base.transform.localRotation.eulerAngles.z);
		}
		if (maxYAxisFromParent != 0f)
		{
			base.transform.localRotation = Quaternion.Euler(base.transform.localRotation.eulerAngles.x, Mathf.Clamp(base.transform.localRotation.eulerAngles.y, 0f - maxYAxisFromParent, maxYAxisFromParent), base.transform.localRotation.eulerAngles.z);
		}
		if (maxZAxisFromParent != 0f)
		{
			base.transform.localRotation = Quaternion.Euler(base.transform.localRotation.eulerAngles.x, base.transform.localRotation.eulerAngles.y, Mathf.Clamp(base.transform.localRotation.eulerAngles.z, 0f - maxZAxisFromParent, maxZAxisFromParent));
		}
		if (rotationOffset != Vector3.zero)
		{
			base.transform.localRotation = Quaternion.Euler(base.transform.localRotation.eulerAngles + rotationOffset);
		}
	}

	public void ChangeOverrideTarget(Transform newTarget)
	{
		cam = newTarget;
	}

	public void SnapToTarget()
	{
		if (!cam)
		{
			if ((bool)overrideTarget)
			{
				cam = overrideTarget;
			}
			else
			{
				cam = MonoSingleton<CameraController>.Instance.gameObject.transform;
			}
		}
		Vector3 position = cam.position;
		if (!useXAxis)
		{
			position.x = base.transform.position.x;
		}
		if (!useYAxis)
		{
			position.y = base.transform.position.y;
		}
		if (!useZAxis)
		{
			position.z = base.transform.position.z;
		}
		Quaternion rotation = Quaternion.LookRotation(position - base.transform.position);
		base.transform.rotation = rotation;
	}
}
