using UnityEngine;

public class CameraTargetInfo
{
	public Vector3 position;

	public Vector3 rotation;

	public GameObject caller;

	public CameraTargetInfo(Vector3 newPosition, GameObject newCaller)
	{
		position = newPosition;
		rotation = new Vector3(20f, 0f, 0f);
		caller = newCaller;
	}

	public CameraTargetInfo(Vector3 newPosition, Vector3 newRotation, GameObject newCaller)
	{
		position = newPosition;
		rotation = newRotation;
		caller = newCaller;
	}
}
