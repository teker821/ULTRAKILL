using UnityEngine;

public class CannonballExtraCollider : MonoBehaviour
{
	public Cannonball source;

	private void OnTriggerEnter(Collider other)
	{
		if (source.launched)
		{
			source.Collide(other);
		}
	}
}
