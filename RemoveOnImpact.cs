using UnityEngine;

public class RemoveOnImpact : MonoBehaviour
{
	public string otherTag;

	public float timeUntilRemove;

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == otherTag)
		{
			Invoke("RemoveSelf", timeUntilRemove);
		}
	}

	private void RemoveSelf()
	{
		Object.Destroy(base.gameObject);
	}
}
