using UnityEngine;

public class TeleportItem : MonoBehaviour
{
	public Vector3 position;

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == 22)
		{
			other.transform.position = position;
		}
	}
}
