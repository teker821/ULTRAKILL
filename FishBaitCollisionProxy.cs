using UnityEngine;

public class FishBaitCollisionProxy : MonoBehaviour
{
	[SerializeField]
	private FishBait fishBait;

	private Vector3 lastPosition;

	private void OnTriggerExit(Collider other)
	{
		fishBait.OnTriggerExit(other);
	}

	private void OnCollisionEnter(Collision collision)
	{
		fishBait.OnCollisionEnter(collision);
	}

	private void Update()
	{
		if (Physics.Raycast(lastPosition, base.transform.position - lastPosition, out var _, Vector3.Distance(lastPosition, base.transform.position), LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)))
		{
			Debug.Log("Out of water due to col proxy raycast");
			fishBait.OutOfWater();
		}
		lastPosition = base.transform.position;
	}
}
