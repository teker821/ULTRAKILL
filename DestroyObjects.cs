using UnityEngine;

public class DestroyObjects : MonoBehaviour
{
	[SerializeField]
	private bool destroyOnEnable;

	[SerializeField]
	private bool dontDestroyOnTrigger;

	[SerializeField]
	private GameObject[] targets;

	private void OnEnable()
	{
		if (destroyOnEnable)
		{
			Destroy();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player") && !dontDestroyOnTrigger)
		{
			Destroy();
		}
	}

	public void Destroy()
	{
		GameObject[] array = targets;
		foreach (GameObject gameObject in array)
		{
			if (gameObject != null)
			{
				Object.Destroy(gameObject);
			}
		}
	}
}
