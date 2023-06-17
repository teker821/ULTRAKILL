using UnityEngine;

public class LateUpdateTest : MonoBehaviour
{
	private void LateUpdate()
	{
		base.transform.position = MonoSingleton<NewMovement>.Instance.transform.position;
	}

	private void OnTriggerEnter(Collider other)
	{
		Debug.Log("Fuck");
	}
}
