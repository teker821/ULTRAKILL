using UnityEngine;

public class TeleportObject : MonoBehaviour
{
	public Vector3 position;

	public void Teleport(Transform target)
	{
		target.position = position;
	}
}
