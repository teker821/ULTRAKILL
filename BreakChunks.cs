using UnityEngine;

public class BreakChunks : MonoBehaviour
{
	public GameObject[] chunks;

	private void Start()
	{
		if (chunks.Length == 0)
		{
			return;
		}
		GoreZone componentInParent = base.transform.GetComponentInParent<GoreZone>();
		GameObject[] array = chunks;
		for (int i = 0; i < array.Length; i++)
		{
			GameObject gameObject = Object.Instantiate(array[i], base.transform.position, Random.rotation);
			Vector3 force = new Vector3(Random.Range(-45, 45), Random.Range(-45, 45), Random.Range(-45, 45));
			gameObject.GetComponent<Rigidbody>()?.AddForce(force, ForceMode.VelocityChange);
			if (componentInParent != null)
			{
				gameObject.transform.SetParent(componentInParent.gibZone);
			}
		}
	}
}
