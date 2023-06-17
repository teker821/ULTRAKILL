using UnityEngine;

public class BreakOnImpact : MonoBehaviour
{
	[SerializeField]
	private float minImpactForce = 1f;

	private void OnCollisionEnter(Collision collision)
	{
		if (!collision.collider.CompareTag("Player") && !(collision.relativeVelocity.magnitude < minImpactForce))
		{
			GetComponent<Breakable>().Break();
		}
	}
}
