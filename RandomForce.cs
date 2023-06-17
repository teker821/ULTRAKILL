using UnityEngine;

public class RandomForce : MonoBehaviour
{
	public float force;

	public bool onEnable = true;

	public bool oneTime = true;

	private bool applied;

	private void OnEnable()
	{
		if (onEnable && (!oneTime || !applied))
		{
			ApplyForce(force);
		}
	}

	public void ApplyForce()
	{
		ApplyForce(force);
	}

	public void ApplyForce(float tempForce)
	{
		applied = true;
		GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * tempForce, ForceMode.VelocityChange);
	}
}
