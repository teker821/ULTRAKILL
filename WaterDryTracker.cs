using UnityEngine;

public class WaterDryTracker
{
	public Transform transform;

	public Vector3 closestPosition;

	public WaterDryTracker(Transform tf, Vector3 clopo)
	{
		transform = tf;
		closestPosition = clopo;
	}
}
