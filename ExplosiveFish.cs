using UnityEngine;

public class ExplosiveFish : MonoBehaviour
{
	private Rigidbody rb;

	private bool activated;

	private TimeSince timeSinceActivated;

	[SerializeField]
	private GameObject fire;

	[SerializeField]
	private GameObject explosion;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}

	private void Update()
	{
		if (!rb.isKinematic)
		{
			if (!activated)
			{
				timeSinceActivated = 0f;
			}
			activated = true;
			fire.SetActive(value: true);
			if ((float)timeSinceActivated > 3f)
			{
				Object.Instantiate(explosion, base.transform.position, Quaternion.identity);
				Object.Destroy(base.gameObject);
			}
		}
	}
}
