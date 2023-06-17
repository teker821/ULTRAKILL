using UnityEngine;

public class billboard : MonoBehaviour
{
	public static Transform cam;

	public Vector3 freeRotation = Vector3.one;

	private Vector3 eangles = Vector3.zero;

	private void Start()
	{
		if (cam == null)
		{
			cam = MonoSingleton<CameraController>.Instance.GetComponent<Transform>();
		}
	}

	private void LateUpdate()
	{
		base.transform.LookAt(cam);
		base.transform.Rotate(0f, 180f, 0f);
		eangles = base.transform.eulerAngles;
		eangles.x *= freeRotation.x;
		eangles.y *= freeRotation.y;
		eangles.z *= freeRotation.z;
		base.transform.eulerAngles = eangles;
	}
}
