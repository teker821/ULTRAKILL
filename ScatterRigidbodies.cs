using UnityEngine;

public class ScatterRigidbodies : MonoBehaviour
{
	private Rigidbody[] rbs;

	public Vector3 minForce;

	public Vector3 maxForce;

	public float rotationForce;

	private void Start()
	{
		rbs = GetComponentsInChildren<Rigidbody>();
		Rigidbody[] array = rbs;
		foreach (Rigidbody obj in array)
		{
			obj.AddForce(Random.Range(minForce.x, maxForce.x), Random.Range(minForce.y, maxForce.y), Random.Range(minForce.z, maxForce.z), ForceMode.VelocityChange);
			obj.AddTorque(Random.Range(0f - rotationForce, rotationForce), Random.Range(0f - rotationForce, rotationForce), Random.Range(0f - rotationForce, rotationForce), ForceMode.VelocityChange);
		}
	}
}
