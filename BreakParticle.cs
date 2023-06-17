using UnityEngine;

public class BreakParticle : MonoBehaviour
{
	public GameObject particle;

	private void OnDestroy()
	{
		if (base.gameObject.activeInHierarchy)
		{
			Object.Instantiate(particle, base.transform.position, base.transform.rotation);
		}
	}
}
