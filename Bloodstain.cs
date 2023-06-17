using UnityEngine;

public class Bloodstain : MonoBehaviour
{
	private bool checking = true;

	private void Start()
	{
		Invoke("StopChecking", 0.01f);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (checking && other.gameObject.layer == 18)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void StopChecking()
	{
		checking = false;
	}
}
