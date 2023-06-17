using UnityEngine;

public class DoorOpener : MonoBehaviour
{
	public Door door;

	public bool oneTime;

	private bool colliderless;

	private void Awake()
	{
		colliderless = GetComponent<Collider>() == null && GetComponent<Rigidbody>() == null;
	}

	private void OnEnable()
	{
		if (colliderless)
		{
			Open();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			Open();
		}
	}

	private void Open()
	{
		door.Open(enemy: false, skull: true);
		if (oneTime)
		{
			base.enabled = false;
		}
	}
}
