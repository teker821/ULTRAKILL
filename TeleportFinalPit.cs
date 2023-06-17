using UnityEngine;

public class TeleportFinalPit : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Player")
		{
			other.transform.position = other.transform.position + base.transform.forward * 20f + Vector3.up * 20f;
		}
	}
}
